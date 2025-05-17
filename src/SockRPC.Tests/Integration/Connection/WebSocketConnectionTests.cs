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
using SockRPC.Core.Connection.Interfaces;
using SockRPC.Core.Extensions;

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
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("Configuration/appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var serviceCollection = new ServiceCollection();

        // Use HostExtensions to configure services
        serviceCollection.ConfigureServices(configuration);

        // Add logging
        serviceCollection.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
        });

        // Manually configure WebSocketSettings
        var webSocketSettings = new WebSocketSettings
        {
            BufferSize = 8192,
            MaxMessageSize = 1048576
        };
        serviceCollection.AddSingleton(webSocketSettings);

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
                            if (context.WebSockets.IsWebSocketRequest)
                            {
                                var webSocketServer = context.RequestServices.GetRequiredService<IWebSocketServer>();
                                await webSocketServer.HandleRequest(context);
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
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
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