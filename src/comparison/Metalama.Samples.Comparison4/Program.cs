using Metalama.Samples.Comparison4;

Console.WriteLine(
    new Person { Name = "Alan Turing", DateOfBirth = new DateTime( 1912, 6, 23 ) }
    == new Person { Name = "Alan Turing", DateOfBirth = new DateTime( 1912, 6, 23 ) } );