using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[CompileTime]
public static class DependencyHelper
{
    /// <summary>
    /// Gets a graph mapping referenced properties to referencing properties.
    /// </summary>
    public static Dictionary<string, string[]> GetPropertyDependencyGraph( INamedType type )
        => type.Properties
            .SelectMany( p =>
                             p.GetReferencedProperties().Select( x => (Referenced: x, Referencing: p.Name) ) )
            .GroupBy( r => r.Referenced )
            .ToDictionary( g => g.Key, g => g.Select( x => x.Referencing ).ToArray() );

    /// <summary>
    /// Gets the name of all properties referenced by a given property. Only the properties of the same declaring type are returned.
    /// </summary>
    private static IEnumerable<string> GetReferencedProperties( this IProperty property )
    {
        var compilation = property.Compilation.GetRoslynCompilation();

        var propertySymbol = property.GetSymbol();

        if ( propertySymbol == null )
        {
            return Enumerable.Empty<string>();
        }

        var body = propertySymbol
            .DeclaringSyntaxReferences
            .Select( r => r.GetSyntax() )
            .Cast<PropertyDeclarationSyntax>()
            .Select( GetGetterBody )
            .SingleOrDefault();

        if ( body == null )
        {
            return Enumerable.Empty<string>();
        }

        var semanticModel = property.Compilation.GetSemanticModel( body.SyntaxTree );

        var properties = new HashSet<IPropertySymbol>( SymbolEqualityComparer.Default );
        var visitor = new Visitor( properties, semanticModel );
        visitor.Visit( body );

        // We only take into account properties of the current type or any base type.
        return properties
            .Where( p =>
                        compilation.HasImplicitConversion(
                            propertySymbol.ContainingType,
                            p.ContainingType ) )
            .Select( p => p.Name );
    }

    private class Visitor : CSharpSyntaxWalker
    {
        private readonly HashSet<IPropertySymbol> _properties;
        private readonly SemanticModel _semanticModel;

        public Visitor( HashSet<IPropertySymbol> properties, SemanticModel semanticModel )
        {
            this._properties = properties;
            this._semanticModel = semanticModel;
        }

        public override void VisitIdentifierName( IdentifierNameSyntax node )
        {
            var symbol = this._semanticModel.GetSymbolInfo( node ).Symbol;

            if ( symbol is IPropertySymbol property )
            {
                this._properties.Add( property );
            }
        }
    }

    /// <summary>
    /// Gets the body of the property getter, if any.
    /// </summary>
    private static SyntaxNode? GetGetterBody( PropertyDeclarationSyntax property )
    {
        if ( property.ExpressionBody != null )
        {
            return property.ExpressionBody;
        }

        if ( property.AccessorList == null )
        {
            return null;
        }

        // We are not using LINQ to work around a bug (#33676) with lambda expressions in compile-time code.
        foreach ( var accessor in property.AccessorList.Accessors )
        {
            if ( accessor.Keyword.IsKind( SyntaxKind.GetKeyword ) )
            {
                return (SyntaxNode?) accessor.ExpressionBody ?? accessor.Body;
            }
        }

        return null;
    }
}