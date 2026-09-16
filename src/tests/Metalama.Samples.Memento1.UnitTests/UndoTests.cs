using Xunit;

namespace Metalama.Samples.Memento1.UnitTests;

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

    [Fact]
    public void Undo_AfterRemove_RestoresListAndKeepsPositionalSelection()
    {
        var caretaker = new Caretaker();
        var viewModel = CreateViewModelWithTwoFish( caretaker );

        var originalFirst = viewModel.Fishes[0];
        var originalSecond = viewModel.Fishes[1];

        // Second New auto-selected its fish.
        Assert.Same( originalSecond, viewModel.CurrentFish );

        // Act: remove the selected fish. ExecuteRemove picks the next item at
        // the same index, falling back to the last one — here, originalFirst.
        viewModel.RemoveCommand.Execute();
        Assert.Single( viewModel.Fishes );
        Assert.Same( originalFirst, viewModel.CurrentFish );

        // Undo restores the list, and ExecuteUndo's positional fix-up keeps
        // the pre-undo selection by index (index 0 → originalFirst).
        viewModel.UndoCommand.Execute();

        Assert.Equal( 2, viewModel.Fishes.Count );
        Assert.Same( originalFirst, viewModel.Fishes[0] );
        Assert.Same( originalSecond, viewModel.Fishes[1] );
        Assert.Same( originalFirst, viewModel.CurrentFish );
    }

    [Fact]
    public void Undo_AfterNew_RestoresPreviousList()
    {
        var caretaker = new Caretaker();
        var viewModel = CreateViewModelWithTwoFish( caretaker );

        var originalFirst = viewModel.Fishes[0];

        // Act: undo the second New.
        viewModel.UndoCommand.Execute();

        // Assert: the list is restored to one fish; the pre-undo selection
        // (index 1) is clamped to the last item in the restored list.
        Assert.Single( viewModel.Fishes );
        Assert.Same( originalFirst, viewModel.Fishes[0] );
        Assert.Same( originalFirst, viewModel.CurrentFish );
    }
}
