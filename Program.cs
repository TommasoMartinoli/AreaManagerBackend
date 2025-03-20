using ADLoginAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);

string environment = builder.Environment.EnvironmentName?.Trim() ?? "Development";
Console.WriteLine($"[DEBUG] Using environment: '{environment}'");

//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    .WriteTo.File("logs/app_log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
//    .CreateLogger();
//
//builder.Host.UseSerilog();

AppConfig.Configuration = builder.Configuration;

builder.Configuration
       .SetBasePath(AppContext.BaseDirectory)
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       //.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
       .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
       .AddEnvironmentVariables();

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var catalogConnection = builder.Configuration.GetConnectionString("CatalogConnection");

if (string.IsNullOrEmpty(defaultConnection) || string.IsNullOrEmpty(catalogConnection))
{
    Log.Error("DefaultConnection string not found!");
}
else
{
    builder.Services.AddDbContext<CatalogDbContext>(options =>
        options.UseSqlServer(catalogConnection));

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(defaultConnection));

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp",
            builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
    });

    builder.Services.AddControllers();

    var app = builder.Build();

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error != null)
            {
                Log.Error(exceptionHandlerPathFeature.Error, "Unhandled Exception");
            }
        });
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseCors("AllowAngularApp");
    app.UseRouting();
    app.MapControllers();

    try
    {
        app.Run();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "L'applicazione ha avuto un crash inaspettato");
    }
    finally
    {
        Log.CloseAndFlush();
    }
}
