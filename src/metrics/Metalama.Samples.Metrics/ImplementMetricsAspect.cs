using System.Diagnostics.Metrics;
using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Samples.Metrics;

internal class ImplementMetricsAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        // Create the Metrics type.
        var metricsType = builder
            .With( builder.Target.Compilation )
            .WithNamespace( "Metrics" )
            .IntroduceClass(
                builder.Target.Name + "Metrics",
                whenExists: OverrideStrategy.New,
                buildType: t => t.Accessibility = Accessibility.Public );

        metricsType.IntroduceConstructor( nameof(this.MetricsConstructorTemplate) );

        // Pull dependencies into the metric type.
        var meterFactoryField = metricsType.IntroduceDependency( typeof(IMeterFactory) ).Declaration;
        var meterHostField = metricsType.IntroduceDependency( typeof(IMetricHost) ).Declaration;

        // Introduce a dependency to the Metrics type into the target type.
        var metricsField = builder.IntroduceDependency(
                metricsType.Declaration.ToNullable(),
                new DependencyOptions() { IsRequired = false } )
            .Declaration;

        // Iterate metrics and simultaneously build the Metrics type and override the instrumented methods.
        var metrics = new List<MetricMetadata>();

        var predecessors = builder.AspectInstance.Predecessors
            .Select( p => (IAspectInstance) p.Instance )
            .Select( i => (Aspect: (MetricAttribute) i.Aspect,
                           TargetMethod: (IMethod) i.TargetDeclaration.GetTarget()) )
            .OrderBy( x => x.TargetMethod )
            .ThenBy( x => x.Aspect
                         .MetricKind ); // TODO: It would be better to order by aspect execution order instead of alphabetical name, but we don't have access to this data.

        foreach ( var predecessor in predecessors )
        {
            var fieldName = predecessor.TargetMethod.Name + predecessor.Aspect.MetricKind;
            var metricName = predecessor.TargetMethod.Name + "." + predecessor.Aspect.MetricKind;

            // Introduce a field to store the metric.
            var metricProperty = metricsType
                .IntroduceField(
                    fieldName,
                    predecessor.Aspect.MetricType,
                    IntroductionScope.Instance,
                    buildField: p =>
                    {
                        p.Accessibility = Accessibility.Internal;
                        p.Writeability = Writeability.ConstructorOnly;
                    } )
                .Declaration;

            // Override the method.
            builder.With( predecessor.TargetMethod )
                .WithTemplateProvider( predecessor.Aspect )
                .Override(
                    nameof(predecessor.Aspect.OverrideMethodTemplate),
                    args: new { metricsField, metricProperty } );

            // Remember the field metadata to advice the constructor.
            metrics.Add(
                new MetricMetadata(
                    predecessor.TargetMethod,
                    predecessor.Aspect,
                    metricProperty,
                    metricName,
                    metrics.Count ) );
        }

        metricsType.AddInitializer(
            nameof(this.InitializeMetrics),
            InitializerKind.BeforeInstanceConstructor,
            args: new { metrics, meterFactoryField, meterHostField } );

        builder.With( builder.Target.Compilation )
            .AddAnnotation( new MetricTypeAnnotation( metricsType.Declaration.ToRef() ) );
    }

    [Template]
    public void MetricsConstructorTemplate() { }

    [Template]
    private void InitializeMetrics( List<MetricMetadata> metrics, IField meterFactoryField, IField meterHostField )
    {
        var meter = ((IMeterFactory) meterFactoryField.Value!).Create(
            ((IMetricHost) meterHostField.Value!).ApplicationName,
            ((IMetricHost) meterHostField.Value!).ApplicationVersion,
            ((IMetricHost) meterHostField.Value!).Tags );

        foreach ( var metric in metrics )
        {
            metric.Aspect.CreateMetricTemplate(
                ExpressionFactory.Capture( meter ),
                metric.MetricProperty,
                metric.MetricName );

            ((IMetricHost) meterHostField.Value).RegisterInstrument(
                metric.MetricProperty.Value,
                metric.Method.ToMethodInfo(),
                metric.Aspect.MetricKind );
        }
    }
}