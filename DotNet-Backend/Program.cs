using API.Data;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// 🟦 Configure MySQL DbContext with retry during runtime
// -------------------------------------------------------
var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));

string host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
string port = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "myapp";
string user = Environment.GetEnvironmentVariable("DB_USER") ?? "root";

string passwordFile = Environment.GetEnvironmentVariable("DB_PASSWORD_FILE");
string password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

if (!string.IsNullOrEmpty(passwordFile) && File.Exists(passwordFile))
{
    password = File.ReadAllText(passwordFile).Trim();
}

string connectionString =
    $"Server={host};Port={port};Database={dbName};User Id={user};Password={password};";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion,
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(3),
                errorNumbersToAdd: null
            );
        }
    )
);

// ----------------------------
// Add Controllers & Swagger
// ----------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🟦 STEP 1: Register CORS Service (Crucial for Frontend connection)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// -------------------------------------------------------
// 🔥 Apply migrations with retry during startup
// -------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    const int maxRetries = 10;
    int retry = 0;

    while (true)
    {
        try
        {
            Console.WriteLine("⏳ Applying migrations...");
            db.Database.Migrate();
            Console.WriteLine("✅ Migrations applied successfully!");
            break;
        }
        catch (Exception ex)
        {
            retry++;
            Console.WriteLine($"⛔ Migration failed ({retry}/{maxRetries}). Error: {ex.Message}");
            if (retry >= maxRetries) throw;
            await Task.Delay(3000);
        }
    }
}

// ----------------------------
// HTTP Request Pipeline
// ----------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🟦 STEP 2: Use CORS Middleware (Must be BEFORE Authorization)
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
