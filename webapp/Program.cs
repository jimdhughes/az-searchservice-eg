using Microsoft.Extensions.Azure;
using webapp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add Azure Clients
builder.Services.AddAzureClients(b =>
{
  // Add Search Client
  b.AddSearchClient(builder.Configuration.GetSection("SearchClient"));
});

builder.Services.AddScoped<ISearchService, SearchService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
