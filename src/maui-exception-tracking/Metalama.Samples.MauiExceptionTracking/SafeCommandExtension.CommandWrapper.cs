using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Metalama.Samples.MauiExceptionTracking;

public partial class SafeCommandExtension
{
    /// <summary>
    /// Wraps an <see cref="ICommand"/> to provide visual context tracking and exception handling.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This wrapper is created by <see cref="CommandWrapperConverter"/> when the binding value is converted.
    /// It intercepts command execution to:
    /// <list type="bullet">
    /// <item>Push the source visual element onto the <see cref="IVisualContextTracker"/></item>
    /// <item>Catch and report exceptions to <see cref="IExceptionReporter"/></item>
    /// <item>For <see cref="AsyncRelayCommand"/>, properly await the async operation</item>
    /// </list>
    /// </para>
    /// </remarks>
    private sealed class CommandWrapper : ICommand
    {
        private readonly ICommand _innerCommand;
        private readonly VisualElement _sourceElement;
        private readonly IVisualContextTracker _visualContextTracker;
        private readonly IExceptionReporter _exceptionReporter;

        public CommandWrapper(
            ICommand innerCommand,
            VisualElement sourceElement,
            IVisualContextTracker visualContextTracker,
            IExceptionReporter exceptionReporter)
        {
            this._innerCommand = innerCommand ?? throw new ArgumentNullException(nameof(innerCommand));
            this._sourceElement = sourceElement ?? throw new ArgumentNullException(nameof(sourceElement));
            this._visualContextTracker = visualContextTracker ?? throw new ArgumentNullException(nameof(visualContextTracker));
            this._exceptionReporter = exceptionReporter ?? throw new ArgumentNullException(nameof(exceptionReporter));
        }

        public event EventHandler? CanExecuteChanged
        {
            add => this._innerCommand.CanExecuteChanged += value;
            remove => this._innerCommand.CanExecuteChanged -= value;
        }

        public bool CanExecute(object? parameter)
        {
            try
            {
                return this._innerCommand.CanExecute(parameter);
            }
            catch (Exception ex)
            {
                this._exceptionReporter.Report(ex, this._sourceElement);
                return false;
            }
        }

        public void Execute(object? parameter)
        {
            using var context = this._visualContextTracker.Push(this._sourceElement);

            if ( this._innerCommand is AsyncRelayCommand asyncRelayCommand )
            {
                _ = this.ExecuteAsync( asyncRelayCommand, parameter );
            }
            else
            {

                try
                {
                    this._innerCommand.Execute( parameter );
                  
                }
                catch ( Exception ex )
                {
                    this._exceptionReporter.Report( ex, this._sourceElement );
                }
            }
        }

        private async Task ExecuteAsync( AsyncRelayCommand command, object? parameter )
        {
            try
            {
                await command.ExecuteAsync( parameter );
            }
            catch ( Exception ex )
            {
                this._exceptionReporter.Report( ex, this._sourceElement );
            }
        }

    }
}
