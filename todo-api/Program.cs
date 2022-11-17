using Azure.Identity;
using Microsoft.AspNetCore.DataProtection;
using Azure.Extensions.AspNetCore.DataProtection.Blobs;

string? dpEndpoint = Environment.GetEnvironmentVariable("DataProtectionEndpoint");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (string.IsNullOrEmpty(dpEndpoint)) throw new Exception("missing protection endpoint");

builder.Services.AddDataProtection().PersistKeysToAzureBlobStorage(new Uri(dpEndpoint),
        new DefaultAzureCredential());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run("http://+:3000");

