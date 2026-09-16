using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[CompileTime]
public static class CachingOptionsExtensions
{
    private static readonly ConditionalWeakTable<CachingOptions,
            ImmutableDictionary<string, CacheBuilderRegistration>>
        _cache = new();

    private static readonly DiagnosticDefinition<IType> _error = new(
        "CACHE01",
        Severity.Error,
        "The type '{0}' cannot be a part of a cache key. Implement ICacheKey, use [CacheKeyMember] or register a cache key builder." );

    private static ImmutableDictionary<string, CacheBuilderRegistration> GetRegistrations(
        CachingOptions cachingOptions )
    {
        lock ( _cache )
        {
            if ( _cache.TryGetValue( cachingOptions, out var dictionary ) )
            {
                return dictionary;
            }

            dictionary =
                cachingOptions.Registrations.ToImmutableDictionary( x => x.KeyType, x => x );

            _cache.Add( cachingOptions, dictionary );

            return dictionary;
        }
    }

    internal static bool VerifyCacheKeyMember<T>(
        this CachingOptions cachingOptions,
        T expression,
        ScopedDiagnosticSink diagnosticSink )
        where T : IExpression, IDeclaration
    {
        // Check supported intrinsics.
        switch ( expression.Type.SpecialType )
        {
            case SpecialType.Boolean:
            case SpecialType.Byte:
            case SpecialType.Decimal:
            case SpecialType.Double:
            case SpecialType.SByte:
            case SpecialType.Int16:
            case SpecialType.UInt16:
            case SpecialType.Int32:
            case SpecialType.UInt32:
            case SpecialType.Int64:
            case SpecialType.UInt64:
            case SpecialType.String:
            case SpecialType.Single:
                return true;
        }

        // Check registered types.
        var registrations = GetRegistrations( cachingOptions );

        var typeId = expression.Type.ToNonNullable()
            .ToDisplayString( CodeDisplayFormat.FullyQualified );

        if ( registrations.ContainsKey( typeId ) )
        {
            return true;
        }

        // Check ICacheKey.
        if ( expression.Type.IsConvertibleTo( typeof(ICacheKey) ) ||
             (expression.Type is INamedType { BelongsToCurrentProject: true } namedType &&
              namedType.Enhancements().HasAspect<GenerateCacheKeyAspect>()) )
        {
            return true;
        }

        diagnosticSink.Report( _error.WithArguments( expression.Type ), expression );

        return false;
    }

    internal static bool TryGetCacheKeyExpression(
        this CachingOptions cachingOptions,
        IExpression expression,
        IExpression cacheKeyBuilderProvider,
        [NotNullWhen( true )] out IExpression? cacheKeyExpression )
    {
        var expressionBuilder = new ExpressionBuilder();

        var typeId = expression.Type.ToNonNullable()
            .ToDisplayString( CodeDisplayFormat.FullyQualified );

        if ( GetRegistrations( cachingOptions )
            .TryGetValue( typeId, out var registration ) )
        {
            if ( registration.UseToString )
            {
                expressionBuilder.AppendExpression( expression );

                if ( expression.Type.IsNullable == true )
                {
                    expressionBuilder.AppendVerbatim( "?.ToString() ?? \"null\"" );
                }
            }
            else
            {
                expressionBuilder.AppendExpression( cacheKeyBuilderProvider );
                expressionBuilder.AppendVerbatim( ".GetCacheKeyBuilder<" );
                expressionBuilder.AppendTypeName( expression.Type );
                expressionBuilder.AppendVerbatim( ", " );

                expressionBuilder.AppendTypeName(
                    registration.BuilderType!.Value.Resolve( expression.Type.Compilation ) );

                expressionBuilder.AppendVerbatim( ">(" );
                expressionBuilder.AppendExpression( expression );
                expressionBuilder.AppendVerbatim( ")" );

                if ( expression.Type.IsNullable == true )
                {
                    expressionBuilder.AppendVerbatim( "?? \"null\"" );
                }
            }
        }
        else if ( expression.Type.IsConvertibleTo( typeof(ICacheKey) ) ||
                  (expression.Type is INamedType namedType &&
                   namedType.Enhancements().HasAspect<GenerateCacheKeyAspect>()) )
        {
            expressionBuilder.AppendExpression( expression );
            expressionBuilder.AppendVerbatim( ".ToCacheKey(" );
            expressionBuilder.AppendExpression( cacheKeyBuilderProvider );
            expressionBuilder.AppendVerbatim( ")" );

            if ( expression.Type.IsNullable == true )
            {
                expressionBuilder.AppendVerbatim( "?? \"null\"" );
            }
        }
        else
        {
            cacheKeyExpression = null;

            return false;
        }

        cacheKeyExpression = expressionBuilder.ToExpression();

        return true;
    }
}