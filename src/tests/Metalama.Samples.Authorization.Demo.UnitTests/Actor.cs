namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// The people of <see cref="DemoOrganization"/>, as test data. An enum keeps test case names short and readable.
/// </summary>
public enum Actor
{
    /// <summary>Administrator of the company.</summary>
    Alice,

    /// <summary>Accountant of the company.</summary>
    Ada,

    /// <summary>Auditor of the company.</summary>
    Otto,

    /// <summary>Auditor of the company and manager of Marketing.</summary>
    Grace,

    /// <summary>Manager of Sales and Marketing, owner of the marketing invoice.</summary>
    Mia,

    /// <summary>Assistant in Sales, owner of the sales invoice.</summary>
    Sam,

    /// <summary>Manager of Engineering.</summary>
    Erin,

    /// <summary>Assistant in Engineering, owner of the engineering invoice.</summary>
    Tom,

    /// <summary>A user without any role.</summary>
    Vera
}

public static class ActorExtensions
{
    public static IReadOnlyList<Actor> All { get; } = Enum.GetValues<Actor>();

    public static User GetUser( this DemoOrganization org, Actor actor )
        => actor switch
        {
            Actor.Alice => org.Alice,
            Actor.Ada => org.Ada,
            Actor.Otto => org.Otto,
            Actor.Grace => org.Grace,
            Actor.Mia => org.Mia,
            Actor.Sam => org.Sam,
            Actor.Erin => org.Erin,
            Actor.Tom => org.Tom,
            Actor.Vera => org.Vera,
            _ => throw new ArgumentOutOfRangeException( nameof(actor), actor, null )
        };
}
