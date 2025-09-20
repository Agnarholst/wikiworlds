using Npgsql;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string? connString = Environment.GetEnvironmentVariable("AEDRASBANE_DB");

Console.WriteLIne($"DEBUG: Connection string is: {connString}");



