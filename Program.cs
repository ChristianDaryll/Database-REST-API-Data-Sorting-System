using InventoryApi.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using InventoryApi.Validators;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

//Register EF Core with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Register FluentValidation
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

// Built-in .NET OpenAPI (replaces Swagger)
builder.Services.AddOpenApi(); ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
