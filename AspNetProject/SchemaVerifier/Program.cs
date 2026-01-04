using AspNetProject.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("--- VERIFYING DATABASE SCHEMA ---");

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql("Host=localhost;Database=dummy;Username=postgres;Password=postgres")
    .Options;

using var context = new ApplicationDbContext(options);

try 
{
    var script = context.Database.GenerateCreateScript();
    Console.WriteLine("SCHEMA GENERATION SUCCESSFUL!");
    Console.WriteLine("-----------------------------");
    Console.WriteLine(script);
    Console.WriteLine("-----------------------------");
}
catch (Exception ex)
{
    Console.Error.WriteLine("SCHEMA GENERATION FAILED: " + ex.ToString());
    Environment.Exit(1);
}
