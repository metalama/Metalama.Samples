[Cloneable]
internal class Game
{
    public List<Player> Players { get; } = new();

    public GameSettings Settings { get; set; }
}