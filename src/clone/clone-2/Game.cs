[Cloneable]
internal class Game
{
    public Player Player { get; set; }

    [Child]
    public GameSettings Settings { get; set; }
}