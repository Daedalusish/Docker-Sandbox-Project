using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException(
        "Missing connection string 'Database'. Run inside the dev container.");

builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddSingleton<History>();
builder.Services.AddHostedService<CreateHistoryTable>();

var app = builder.Build();

app.MapGet("/fizzbuzz/{count:int}", async (int count, History history) =>
{
    if (count is < 1 or > 100)
    {
        return Results.BadRequest("Count must be between 1 and 100.");
    }

    await history.SaveAsync(count);
    return Results.Ok(FizzBuzz.Range(count));
});

app.MapGet("/history", (History history) => history.LatestAsync());

app.Run();
