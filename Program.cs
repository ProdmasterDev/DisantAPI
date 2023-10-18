using DBF.Configuration;
using DisantAPI.Repository;
using DisantAPI.Services.Classes;
using DisantAPI.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IRFIDService, RFIDService>();

var configuration = builder.Configuration;
builder.Services.Configure<RFIDLoggerOptions>(configuration.GetSection(RFIDLoggerOptions.Position));

builder.Services.AddSingleton(x=> new RFIDLoggerService(x.GetService<IOptions<RFIDLoggerOptions>>()?.Value.LogPath ?? string.Empty));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DBFOptions>(configuration.GetSection(DBFOptions.Position));

#if DEBUG
builder.Services.AddTransient(x =>
    new DisantDBFRepository(x.GetService<IOptions<DBFOptions>>()?.Value.ConnectionStringDev ?? string.Empty));
#else
    builder.Services.AddTransient(x =>
        new DisantDBFRepository(x.GetService<IOptions<DBFOptions>>()?.Value.ConnectionString ?? string.Empty));
#endif

var app = builder.Build();
    
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();