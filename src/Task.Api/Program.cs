using Microsoft.EntityFrameworkCore;
using Tasks.Api.Data;
using Tasks.Api.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var endpoint = builder.Configuration["CosmosDb:Endpoint"];
    var key = builder.Configuration["CosmosDb:Key"];
    var databaseName = builder.Configuration["CosmosDb:DatabaseName"];

    options.UseCosmos(endpoint!, key!, databaseName!);
});
builder.Services.AddHttpClient<IProjectsClient, ProjectsClient>(client =>
{
    var baseUrl = builder.Configuration["ProjectsApi:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl!);
});
builder.Services.AddScoped<ITaskService, TaskService>();

var cosmosEndpoint = builder.Configuration["CosmosDb:Endpoint"];
var cosmosKey = builder.Configuration["CosmosDb:Key"];
var cosmosConnectionString = $"AccountEndpoint={cosmosEndpoint};AccountKey={cosmosKey};";

builder.Services.AddHealthChecks()
    .AddAzureCosmosDB(
        sp => new Microsoft.Azure.Cosmos.CosmosClient(cosmosConnectionString),
        name: "cosmosdb"
    );
builder.Logging.AddConsole();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tasks API v1");

    options.RoutePrefix = "docs/swagger";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
