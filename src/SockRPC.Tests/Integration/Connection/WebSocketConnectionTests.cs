using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SockRPC.Core.Configuration;
using SockRPC.Core.Connection;
using SockRPC.Core.Connection.Interfaces;

namespace SockRPC.Tests.Integration.Connection;

[TestFixture]
public class WebSocketConnectionTests
{
    private IHost _host;
    private int _port;
    private IWebSocketServer _webSocketServer;

    [SetUp]
    public async Task SetUp()
    {
        _port = GetFreeTcpPort();
        var serviceCollection = new ServiceCollection();

        // Load appsettings.json
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("Configuration/appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Bind WebSocketSettings
        var webSocketSettings = configuration.GetSection("WebSocketSettings").Get<WebSocketSettings>();
        if (webSocketSettings == null)
            throw new InvalidOperationException("WebSocketSettings section is missing in the configuration.");

        // Register dependencies
        serviceCollection.AddSingleton(webSocketSettings);
        serviceCollection.AddSingleton<IWebSocketConnectionAcceptor, WebSocketConnectionAcceptor>();
        serviceCollection.AddSingleton<IWebSocketMessageProcessor, WebSocketMessageProcessor>();
        serviceCollection.AddSingleton<IWebSocketBufferManager, WebSocketBufferManager>();
        serviceCollection.AddSingleton<IWebSocketServer, WebSocketServer>();

        // Add logging
        serviceCollection.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _webSocketServer = serviceProvider.GetRequiredService<IWebSocketServer>();

        _host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel()
                    .UseUrls($"http://localhost:{_port}")
                    .Configure(app =>
                    {
                        app.UseWebSockets();
                        app.Use(async (context, next) =>
                        {
                            if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
                            {
                                await _webSocketServer.HandleRequest(context);
                            }
                            else
                            {
                                await next();
                            }
                        });
                    });
            })
            .ConfigureServices(services =>
            {
                foreach (var service in serviceCollection)
                {
                    services.Add(service);
                }
            })
            .Build();

        await _host.StartAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _host.StopAsync();
        _host.Dispose();
    }

    [Test]
    public async Task WebSocket_ShouldConnectSuccessfully()
    {
        // Arrange
        var clientWebSocket = new ClientWebSocket();
        var serverUri = new Uri($"ws://localhost:{_port}/ws");

        // Act
        await clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);

        // Assert
        clientWebSocket.State.Should().Be(WebSocketState.Open);

        // Cleanup
        await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
    }

    private int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}