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

    public void Dispose()
    {
        switch ( this._kind )
        {
            case Kind.Context:
                SecurityContext.Restore( this._previousContext );

                break;

            case Kind.System:
                SecurityContext.RestoreIsSystem( this._previousIsSystem );

                break;
        }
    }

    private enum Kind
    {
        None,
        Context,
        System
    }
}
