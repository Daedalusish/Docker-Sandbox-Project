using Dapper;
using Npgsql;

// Remembers which counts have been asked for. Stands in for a real data layer.
public class History(NpgsqlDataSource database)
{
    public async Task SaveAsync(int count)
    {
        await using var connection = await database.OpenConnectionAsync();
        await connection.ExecuteAsync("INSERT INTO history (count) VALUES (@count)", new { count });
    }

    public async Task<IEnumerable<int>> LatestAsync()
    {
        await using var connection = await database.OpenConnectionAsync();
        return await connection.QueryAsync<int>("SELECT count FROM history ORDER BY id DESC LIMIT 10");
    }
}

// Creates the table when the app starts. Real projects use a migration tool, such as FluentMigrator.
public class CreateHistoryTable(NpgsqlDataSource database) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS history (
                id       serial PRIMARY KEY,
                count    int NOT NULL,
                asked_at timestamptz NOT NULL DEFAULT now()
            )
            """);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
