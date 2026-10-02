using Metalama.Patterns.Observability;
using Metalama.Patterns.Wpf;
using NameGenerator.Generators;
using System.Collections.Immutable;

[Memento]
[Observable]
public sealed partial class MainViewModel
{
    private readonly IMementoCaretaker? _caretaker;
    private readonly IFishGenerator _fishGenerator;

    public bool IsEditing { get; set; }

    public ImmutableList<Fish> Fishes { get; private set; } = ImmutableList<Fish>.Empty;

    public Fish? CurrentFish { get; set; }

    // Design-time.
    public MainViewModel() : this( new FishGenerator( new RealNameGenerator() ), null ) { }

    public MainViewModel( IFishGenerator fishGenerator, IMementoCaretaker? caretaker )
    {
        this._fishGenerator = fishGenerator;
        this._caretaker = caretaker;
    }

    [Command]
    private void ExecuteNew()
    {
        this._caretaker?.CaptureMemento( this );

        var newFish = new Fish()
        {
            Name = this._fishGenerator.GetNewName(),
            Species = this._fishGenerator.GetNewSpecies(),
            DateAdded = DateTime.Now
        };

        this.Fishes = this.Fishes.Add( newFish );
        this.CurrentFish = newFish;
        this.IsEditing = true;
    }

    public bool CanExecuteNew => !this.IsEditing;

    [Command]
    private void ExecuteRemove()
    {
        if ( this.CurrentFish != null )
        {
            this._caretaker?.CaptureMemento( this );

            var index = this.Fishes.IndexOf( this.CurrentFish );
            this.Fishes = this.Fishes.RemoveAt( index );

            if ( index < this.Fishes.Count )
            {
                this.CurrentFish = this.Fishes[index];
            }
            else if ( this.Fishes.Count > 0 )
            {
                this.CurrentFish = this.Fishes[^1];
            }
            else
            {
                this.CurrentFish = null;
            }
        }
    }

    public bool CanExecuteRemove => this.CurrentFish != null && !this.IsEditing;

    [Command]
    private void ExecuteEdit()
    {
        this.IsEditing = true;
        this._caretaker?.CaptureMemento( this.CurrentFish! );
    }

    public bool CanExecuteEdit => this.CurrentFish != null && !this.IsEditing;

    [Command]
    private void ExecuteSave() => this.IsEditing = false;

    public bool CanExecuteSave => this.IsEditing;

    [Command]
    private void ExecuteCancel()
    {
        this.IsEditing = false;
        this._caretaker?.Undo();
    }

    public bool CanExecuteCancel => this.IsEditing;

    [Command]
    private void ExecuteUndo()
    {
        this.IsEditing = false;
        this._caretaker?.Undo();
    }

    public bool CanExecuteUndo => this._caretaker?.CanUndo == true;
}