using System.ComponentModel;

public interface IMementoCaretaker : INotifyPropertyChanged
{
    bool CanUndo { get; }

    void CaptureMemento( IMementoable originator );

    void Undo();
}