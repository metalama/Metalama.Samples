using Metalama.Samples.Comparison1;

Console.WriteLine(
    new Person { Name = "Alan Turing", DateOfBirth = new DateTime( 1912, 6, 23 ) }
    == new Person { Name = "Alan Turing", DateOfBirth = new DateTime( 1912, 6, 23 ) } );