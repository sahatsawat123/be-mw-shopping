using System.Data;
using Npgsql;


var builder = WebApplication.CreateBuilder(args);
 
// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IPdfTemplate, PurchaseOrderTemplate>();

builder.Services.AddScoped<IDbConnection>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connectionString); // สำหรับ PostgreSQL
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}
 
app.MapControllers();
 
app.Run();