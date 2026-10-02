[Cloneable]
internal class Game
{
    public List<Player> Players { get; private set; } = new();

    [Child]
    public GameSettings Settings { get; set; }

    private void CloneMembers( Game clone ) => clone.Players = new List<Player>( this.Players );
}