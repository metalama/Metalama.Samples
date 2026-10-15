namespace Metalama.Samples.Authorization;

/// <summary>
/// Gives access to the current <see cref="ISecurityContext"/>. Both the context and the system scope
/// flow with the logical call (<see cref="AsyncLocal{T}"/>), so they follow <c>async</c> code and stay
/// isolated between concurrent operations.
/// </summary>
/// <remarks>
/// <para>The framework enforces authorization inside a process against missing checks. It is not a sandbox: any
/// code of the process can call <see cref="Use"/> or <see cref="RunAsSystem"/>. Only trusted infrastructure, such
/// as an authentication middleware or a database seeder, should call them.</para>
/// <para>A context or a system scope also applies to the tasks started while it is active, because
/// <see cref="AsyncLocal{T}"/> values flow into them.</para>
/// </remarks>
public static class SecurityContext
{
    private static readonly AsyncLocal<ISecurityContext?> _current = new();
    private static readonly AsyncLocal<bool> _isSystem = new();

    /// <summary>
    /// Gets the current security context. When it is <c>null</c>, every secured operation fails,
    /// except in a system scope. Set it with <see cref="Use"/>.
    /// </summary>
    public static ISecurityContext? Current => _current.Value;

    /// <summary>
    /// Gets a value indicating whether the code runs in a system scope, where no permission is checked.
    /// </summary>
    public static bool IsSystem => _isSystem.Value;

    /// <summary>
    /// Sets <see cref="Current"/> until the returned object is disposed, then restores the previous context.
    /// </summary>
    public static IDisposable Use( ISecurityContext? context )
    {
        var previous = _current.Value;
        _current.Value = context;

        return new Scope( () => _current.Value = previous );
    }

    /// <summary>
    /// Disables permission checks until the returned object is disposed. Use it for trusted code such as
    /// seeding a database. The framework also uses it while a policy runs.
    /// </summary>
    public static IDisposable RunAsSystem()
    {
        var previous = _isSystem.Value;
        _isSystem.Value = true;

        return new Scope( () => _isSystem.Value = previous );
    }

    private sealed class Scope( Action restore ) : IDisposable
    {
        private Action? _restore = restore;

        public void Dispose()
        {
            this._restore?.Invoke();
            this._restore = null;
        }
    }
}
