namespace Metalama.Samples.Authorization;

/// <summary>
/// A scope of <see cref="SecurityContext"/>, returned by <see cref="SecurityContext.Use"/>,
/// <see cref="SecurityContext.Grant"/> and <see cref="SecurityContext.RunAsSystem"/>.
/// Disposing it restores the state that was current when it was created. Dispose it with a <c>using</c> statement, so
/// that the scopes end in the reverse order of their creation.
/// </summary>
/// <remarks>
/// The scope is a structure, so that opening it does not allocate memory. Disposing the default value does nothing.
/// </remarks>
public readonly struct SecurityScope : IDisposable
{
    private readonly Kind _kind;
    private readonly SecurityContext.ExecutionContext? _previousContext;
    private readonly bool _previousIsSystem;

    internal SecurityScope( SecurityContext.ExecutionContext? previousContext )
    {
        this._kind = Kind.Context;
        this._previousContext = previousContext;
    }

    internal SecurityScope( bool previousIsSystem )
    {
        this._kind = Kind.System;
        this._previousIsSystem = previousIsSystem;
    }

    // Used by SecurityContext.Use, which also ends a system scope.
    internal SecurityScope( SecurityContext.ExecutionContext? previousContext, bool previousIsSystem )
    {
        this._kind = Kind.ContextAndSystem;
        this._previousContext = previousContext;
        this._previousIsSystem = previousIsSystem;
    }

    public void Dispose()
    {
        if ( this._kind is Kind.Context or Kind.ContextAndSystem )
        {
            SecurityContext.Restore( this._previousContext );
        }

        if ( this._kind is Kind.System or Kind.ContextAndSystem )
        {
            SecurityContext.RestoreIsSystem( this._previousIsSystem );
        }
    }

    private enum Kind
    {
        None,
        Context,
        System,
        ContextAndSystem
    }
}
