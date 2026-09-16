using System.ComponentModel;
using System.Runtime.CompilerServices;

public sealed class Caretaker : IMementoCaretaker
{
    private readonly Stack<IMemento> _mementos = new();

    public void CaptureMemento( IMementoable originator )
    {
        this._mementos.Push( originator.SaveToMemento() );

        this.OnPropertyChanged( nameof(this.CanUndo) );
    }

    public void Undo()
    {
        if ( this._mementos.Count > 0 )
        {
            var memento = this._mementos.Pop();
            memento.Originator.RestoreMemento( memento );
        }

        this.OnPropertyChanged( nameof(this.CanUndo) );
    }

    public bool CanUndo => this._mementos.Count > 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged( [CallerMemberName] string? propertyName = null )
        => this.PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
}