using Npgsql;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string? connString = Environment.GetEnvironmentVariable("AEDRASBANE_DB");

Console.WriteLine($"DEBUG: Connection string is: {connString}");

Greet.SayHello();

// Testkommentar for å se om jeg kan pushe fra mac