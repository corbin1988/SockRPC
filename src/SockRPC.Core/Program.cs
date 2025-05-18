using SockRPC.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices(builder.Configuration);
builder.Configuration.AddJsonFile("Configuration/appsettings.json", false, true);

var app = builder.Build();

app.ConfigureMiddleware();
app.Run();