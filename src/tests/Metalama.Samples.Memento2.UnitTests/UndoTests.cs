using Xunit;

namespace Metalama.Samples.Memento2.UnitTests;

public class UndoTests
{
    private sealed class StubFishGenerator : IFishGenerator
    {
        private int _counter;

        public string GetNewName() => $"Fish{++this._counter}";

        public string GetNewSpecies() => "Species";
    }

    private static MainViewModel CreateViewModelWithTwoFish( Caretaker caretaker )
    {
        var viewModel = new MainViewModel( new StubFishGenerator(), caretaker );

        // New enters edit mode on the new fish; Save exits edit mode so the
        // next New is executable.
        viewModel.NewCommand.Execute();
        viewModel.SaveCommand.Execute();
        viewModel.NewCommand.Execute();
        viewModel.SaveCommand.Execute();

        return viewModel;
    }

    [Fact]
    public void Undo_AfterRemove_RestoresCurrentFishFromMemento()
    {
        var caretaker = new Caretaker();
        var viewModel = CreateViewModelWithTwoFish( caretaker );

        Assert.Equal( 2, viewModel.Fishes.Count );
        var originalFirst = viewModel.Fishes[0];
        var originalSecond = viewModel.Fishes[1];

        // Second New auto-selected its fish.
        Assert.Same( originalSecond, viewModel.CurrentFish );

        // Act: remove the selected fish, then undo the removal.
        viewModel.RemoveCommand.Execute();
        Assert.Single( viewModel.Fishes );
        Assert.Same( originalFirst, viewModel.CurrentFish );

        viewModel.UndoCommand.Execute();

        // Assert: the memento restores both the list and the selection
        // that were current at the time the memento was captured.
        Assert.Equal( 2, viewModel.Fishes.Count );
        Assert.Same( originalFirst, viewModel.Fishes[0] );
        Assert.Same( originalSecond, viewModel.Fishes[1] );
        Assert.Same( originalSecond, viewModel.CurrentFish );
    }

    [Fact]
    public void Undo_AfterNew_RestoresSelectionFromMemento()
    {
        var caretaker = new Caretaker();
        var viewModel = CreateViewModelWithTwoFish( caretaker );

        Assert.Equal( 2, viewModel.Fishes.Count );
        var originalFirst = viewModel.Fishes[0];

        // User navigates to the first fish before undoing.
        viewModel.CurrentFish = originalFirst;

        // Act: undo the second New.
        viewModel.UndoCommand.Execute();

        // Assert: the list is restored and CurrentFish matches what was selected
        // when the second New was captured — the first fish, which the first New
        // had auto-selected.
        Assert.Single( viewModel.Fishes );
        Assert.Same( originalFirst, viewModel.Fishes[0] );
        Assert.Same( originalFirst, viewModel.CurrentFish );
    }

    [Fact]
    public void New_EntersEditModeOnNewFish()
    {
        var caretaker = new Caretaker();
        var viewModel = new MainViewModel( new StubFishGenerator(), caretaker );

        viewModel.NewCommand.Execute();

        Assert.Single( viewModel.Fishes );
        Assert.Same( viewModel.Fishes[0], viewModel.CurrentFish );
        Assert.True( viewModel.IsEditing );
    }

    [Fact]
    public void Cancel_AfterNew_DiscardsNewFish()
    {
        var caretaker = new Caretaker();
        var viewModel = new MainViewModel( new StubFishGenerator(), caretaker );

        viewModel.NewCommand.Execute();
        Assert.Single( viewModel.Fishes );

        viewModel.CancelCommand.Execute();

        Assert.Empty( viewModel.Fishes );
        Assert.Null( viewModel.CurrentFish );
        Assert.False( viewModel.IsEditing );
    }
}
