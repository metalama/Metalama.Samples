namespace ErrorNotCloneableChild;

[Cloneable]
internal class Game
{
    [Child]
    public Player Player { get; }

    [Child]
    public GameSettings Settings { get; private set; }
}

[Cloneable]
internal class GameSettings
{
    public int Level { get; set; }

    public string World { get; set; }
}

internal class Player
{
    public string Name { get; }
}