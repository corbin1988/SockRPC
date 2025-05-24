using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SockRPC.Core.Configuration;
using SockRPC.Core.Connection.Interfaces;
using SockRPC.Core.Extensions;
using SockRPC.Core.Testing;

namespace SockRPC.Tests.Integration;

[SetUpFixture]
public class WebSocketIntegrationTestsBase
{
    private IHost _host;
    internal int _port;
    protected ClientWebSocket ClientWebSocket;
    protected IWebSocketServer WebSocketServer;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        TestAcknowledgmentHelper.IsTestEnvironment = true; 
        
        _port = GetFreeTcpPort();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("Configuration/appsettings.json", false, true)
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
        WebSocketServer = serviceProvider.GetRequiredService<IWebSocketServer>();

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
                foreach (var service in serviceCollection) services.Add(service);
            })
            .Build();

        await _host.StartAsync();

        // Initialize and connect the WebSocket client
        ClientWebSocket = new ClientWebSocket();
        var serverUri = new Uri($"ws://localhost:{_port}/ws");
        await ClientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        // Close the WebSocket connection
        if (ClientWebSocket.State == WebSocketState.Open)
            await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Tests complete",
                CancellationToken.None);

        ClientWebSocket?.Dispose();

        // Stop and dispose of the host
        await _host.StopAsync();
        _host.Dispose();
        
        TestAcknowledgmentHelper.IsTestEnvironment = false; 
    }

    private int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}