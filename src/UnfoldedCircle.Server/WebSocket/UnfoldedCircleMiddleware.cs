using System.Buffers;
using System.Globalization;
using System.Net.WebSockets;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using UnfoldedCircle.Server.Configuration;
using UnfoldedCircle.Server.DependencyInjection;

namespace UnfoldedCircle.Server.WebSocket;

internal sealed class UnfoldedCircleMiddleware<TUnfoldedCircleWebSocketHandler, TMediaPlayerCommandId, TConfigurationItem>(
    UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem> unfoldedCircleWebSocketHandler,
    IHostApplicationLifetime applicationLifetime,
    ILoggerFactory loggerFactory,
    ILogger<UnfoldedCircleMiddleware<TUnfoldedCircleWebSocketHandler, TMediaPlayerCommandId, TConfigurationItem>> logger,
    IOptions<UnfoldedCircleOptions> options) : IMiddleware
    where TUnfoldedCircleWebSocketHandler : UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>
    where TMediaPlayerCommandId : struct, Enum
    where TConfigurationItem : UnfoldedCircleConfigurationItem
{
    private readonly UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem> _unfoldedCircleWebSocketHandler = unfoldedCircleWebSocketHandler;
    private readonly IHostApplicationLifetime _applicationLifetime = applicationLifetime;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private readonly ILogger<UnfoldedCircleMiddleware<TUnfoldedCircleWebSocketHandler, TMediaPlayerCommandId, TConfigurationItem>> _logger = logger;
    private readonly IOptions<UnfoldedCircleOptions> _options = options;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var socket = await context.WebSockets.AcceptWebSocketAsync();
                var wsId = $"{context.Connection.RemoteIpAddress?.ToString()}:{context.Connection.RemotePort.ToString(NumberFormatInfo.InvariantInfo)}";

                _logger.WebSocketNewConnection(wsId);

                await using var cancellationTokenWrapper = new CancellationTokenWrapper(wsId,
                    socket,
                    _loggerFactory.CreateLogger<CancellationTokenWrapper>(),
                    _unfoldedCircleWebSocketHandler.HandleEventUpdatesAsync,
                    _options,
                    _applicationLifetime.ApplicationStopping,
                    context.RequestAborted);
                using var memoryStream = new MemoryStream();
                var buffer = ArrayPool<byte>.Shared.Rent(1024 * 4);
                try
                {
                    var result = await _unfoldedCircleWebSocketHandler.HandleWebSocketAsync(socket, memoryStream, buffer, wsId, cancellationTokenWrapper);
                    await socket.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, context.RequestAborted);
                }
                catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
                {
                    // Normal client disconnect
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }

                _logger.WebSocketConnectionClosed(wsId);
            }
            
            await next(context);
        }
        catch (Exception e)
        {
            _logger.UnfoldedCircleMiddlewareException(e);
        }
    }
}