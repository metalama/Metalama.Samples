using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Samples.MauiExceptionTracking;

public sealed partial class VisualContextTracker
{
    /// <summary>
    /// Disposable scope that restores the previous visual context when disposed.
    /// Returned by <see cref="VisualContextTracker.Push"/>.
    /// </summary>
    private sealed class ContextScope : IDisposable
    {
        public static ContextScope Empty { get; } = new();

        private readonly AsyncLocal<VisualElement?> _asyncLocal;
        private readonly VisualElement? _previousValue;
        private bool _disposed;

        public ContextScope(AsyncLocal<VisualElement?> asyncLocal, VisualElement? previousValue)
        {
            this._asyncLocal = asyncLocal;
            this._previousValue = previousValue;
        }

        private ContextScope()
        {
            this._disposed = true;
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this._asyncLocal.Value = this._previousValue;
                this._disposed = true;
            }
        }
    }
}
