using Microsoft.Extensions.Logging;

namespace Models;

public class Repository<T>// : IRepository<T> 
    where T : IIdentifiable
{
    private List<T> _entities = new();
    private readonly ILogger _logger;

    public Repository(ILogger<Repository<T>> logger)
    {
        _logger = logger;
    }

    public void Create(T entity)
    {
        _entities.Add(entity);
    }

    public T? Get(Guid id)
    {
        return _entities.FirstOrDefault(a => a.Id == id);
    }

    public List<T> List()
    {
        return _entities;
    }

    public void Update(T entity)
    {
        for (var a = 0; a < _entities.Count; a++)
        {
            if (_entities[a].Id == entity.Id)
            {
                _entities[a] = entity;
            }
        }
    }

    public void Delete(Guid id)
    {
        _entities = _entities.Where(e => e.Id != id).ToList();
    }
}