using Microsoft.EntityFrameworkCore;
using Projects.Api.Data;
using Projects.Api.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

// Add services to the container.
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseCosmos(
        builder.Configuration["CosmosDb:Endpoint"]!,
        builder.Configuration["CosmosDb:Key"]!,
        builder.Configuration["CosmosDb:DatabaseName"]!
    ));

var cosmosEndpoint = builder.Configuration["CosmosDb:Endpoint"];
var cosmosKey = builder.Configuration["CosmosDb:Key"];
var cosmosConnectionString = $"AccountEndpoint={cosmosEndpoint};AccountKey={cosmosKey};";

builder.Services.AddHealthChecks()
    .AddAzureCosmosDB(
        sp => new Microsoft.Azure.Cosmos.CosmosClient(cosmosConnectionString),
        name: "cosmosdb"
    );
builder.Logging.AddConsole();
builder.Services.AddProblemDetails();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbContext.Database.EnsureCreatedAsync();
}


app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Projects API v1");

    options.RoutePrefix = "docs/swagger";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.Run();
