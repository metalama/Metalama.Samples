using Microsoft.Extensions.Logging;
using Models;

namespace App;

public class Activity
{
    private readonly Repository<Person> _people;
    private readonly ILogger _logger;

    public Activity(Repository<Person> peopleRepo, ILogger<Activity> logger)
    {
        _people = peopleRepo;
        _logger = logger;
    }

    public void DoOperations()
    {
        //Create
        _people.Create(new Person("Andy"));
        _people.Create(new Person("Bill"));
        _people.Create(new Person("Charlie"));

        //List
        var people = _people.List();

        //Get
        var bill = _people.Get(people.First(person => person.Name == "Bill").Id);
        if (bill is not null)
        {
            var updatedBill = new Person("Billy")
            {
                Id = bill.Id
            };

            //Update
            _people.Update(updatedBill);
        }

        //Delete
        _people.Delete(people.First(person => person.Name == "Andy").Id);

        //List #2
        var remainingPeople = _people.List();
        Console.WriteLine($"There are {remainingPeople.Count} remaining people in the repository");
    }
}