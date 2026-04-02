using PanoPos.Application;
using PanoPos.Infrastructure;
using PanoPos.WebApi.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWebApiServices();

var app = builder.Build();

app.UseWebApiPipeline();

app.Run();

public partial class Program;
