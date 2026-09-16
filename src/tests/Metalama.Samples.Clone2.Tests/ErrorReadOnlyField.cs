namespace ErrorReadOnlyField;

[Cloneable]
internal class Game
{
    public Player Player { get; }

    [Child]
    public GameSettings Settings { get; }
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