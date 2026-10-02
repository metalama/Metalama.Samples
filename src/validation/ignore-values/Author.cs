internal class Author
{
    [IgnoreValues( "", null )]
    public string Name { get; set; }

    public Author( string name )
    {
        this.Name = name;
    }
}