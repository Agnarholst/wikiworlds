using Npgsql;
using DotNetEnv;
using Sprache;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string? connString = Environment.GetEnvironmentVariable("AEDRASBANE_DB");

if (string.IsNullOrEmpty(connString))
{
    Console.WriteLine("No connection string found in evn!");
    return;
}


app.MapGet("/", () => "Hello from the server!");

app.MapGet("/dbtest", async () =>
{
    try
    {
        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT version()", conn);

        var version = await cmd.ExecuteScalarAsync();

        return Results.Ok(new { message = "Connected to database!", version });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

app.MapGet("/characters", async () =>
{
    try
    {
        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT id, name FROM characters", conn);

        await using var reader = await cmd.ExecuteReaderAsync();

        var characters = new List<object>();

        while (await reader.ReadAsync())
        {
            characters.Add(new { id = reader.GetInt32(0), name = reader.GetString(1) });
        }

        return Results.Ok(characters);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database query failed: {ex.Message}");
    }
});


app.MapPost("/characters", async (CharacterInput input) =>
{
    try
    {
        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        var sql = "INSERT INTO characters (name, age) VALUES (@name, @age) RETURNING id;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", input.Name);

        cmd.Parameters.AddWithValue("age", (object?)input.Age ?? DBNull.Value);

#pragma warning disable CS8605 // Unboxing a possibly null value.
        var newId = (int)(await cmd.ExecuteScalarAsync()!);
#pragma warning restore CS8605 // Unboxing a possibly null value.

        return Results.Created($"/characters/{newId}", new { id = newId, name = input.Name, age = input.Age });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Insert failed: {ex.Message}");
    }
});

app.Run();
public record CharacterInput(string Name, int? Age);
