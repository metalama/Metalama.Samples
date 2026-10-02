public interface IMementoable
{
    IMemento SaveToMemento();

    void RestoreMemento( IMemento memento );
}