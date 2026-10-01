namespace Models;
public class Person : IIdentifiable
{
    public Person(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    public Guid Id { get; init; }

    public string Name { get; init; }
}