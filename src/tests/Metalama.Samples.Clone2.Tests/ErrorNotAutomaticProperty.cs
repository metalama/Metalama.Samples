namespace ErrorNotAutomaticProperty;

[Cloneable]
internal class Game
{
    private GameSettings _settings;

    public Player Player { get; }

    [Child]
    public GameSettings Settings
    {
        get => this._settings;

        private set
        {
            Console.WriteLine( "Setting the value." );
            this._settings = value;
        }
    }
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