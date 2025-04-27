using SockRPC.Core.Configuration;
using SockRPC.Core.Connection;
using SockRPC.Core.Connection.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IWebSocketServer, WebSocketServer>();

// TODO: Allow developers to specify their own configuration file
builder.Configuration.AddJsonFile("Configuration/appsettings.json", false, true);


builder.Services.AddSingleton<IWebSocketConnectionAcceptor, WebSocketConnectionAcceptor>();
builder.Services.AddSingleton<IWebSocketMessageProcessor, WebSocketMessageProcessor>();
builder.Services.AddSingleton<IWebSocketServer, WebSocketServer>();
builder.Services.Configure<WebSocketSettings>(builder.Configuration.GetSection("WebSocketSettings"));

var configuration = builder.Configuration.GetSection("WebSocketSettings").Get<WebSocketSettings>();
if (configuration == null)
    throw new InvalidOperationException("WebSocketSettings section is missing in the configuration.");

var app = builder.Build();

app.MapWebSocketEndpoint("/ws");

app.Run();