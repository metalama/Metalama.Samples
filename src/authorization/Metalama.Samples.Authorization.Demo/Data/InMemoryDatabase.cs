using Microsoft.Data.Sqlite;

namespace Metalama.Samples.Authorization.Demo.Data;

/// <summary>
/// A named in-memory SQLite database. SQLite deletes an in-memory database when its last connection closes, so
/// this object keeps one connection open for its lifetime. Each database context opens its own connection, which
/// is required because a connection cannot be used by two requests at the same time.
/// </summary>
public sealed class InMemoryDatabase : IDisposable
{
    private readonly SqliteConnection _keepAliveConnection;

    public InMemoryDatabase()
    {
        // A unique name gives each application instance, for instance each test, its own database.
        this.ConnectionString = $"Data Source=Invoicing-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        this._keepAliveConnection = new SqliteConnection( this.ConnectionString );
        this._keepAliveConnection.Open();
    }

    public string ConnectionString { get; }

    public void Dispose() => this._keepAliveConnection.Dispose();
}
