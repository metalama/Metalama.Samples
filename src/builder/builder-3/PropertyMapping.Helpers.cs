using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Samples.Builder3;

internal abstract partial class PropertyMapping
{
    protected bool TryFindBuilderPropertyInBaseType(
        INamedType baseType,
        ScopedDiagnosticSink diagnosticSink,
        [NotNullWhen( true )] out IProperty? baseProperty )
    {
        baseProperty =
            baseType.AllProperties.OfName( this.SourceProperty.Name )
                .SingleOrDefault();

        if ( baseProperty == null )
        {
            diagnosticSink.Report(
                BuilderDiagnosticDefinitions.BaseBuilderMustContainProperty.WithArguments(
                    (
                        baseType, this.SourceProperty.Name) ) );

            return false;
        }

        return true;
    }
}