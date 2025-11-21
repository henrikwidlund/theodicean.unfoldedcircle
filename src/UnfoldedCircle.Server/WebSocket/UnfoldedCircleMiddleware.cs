using System.Globalization;
using System.Net.WebSockets;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using UnfoldedCircle.Server.Configuration;

namespace UnfoldedCircle.Server.WebSocket;

internal sealed class UnfoldedCircleMiddleware<TUnfoldedCircleWebSocketHandler, TMediaPlayerCommandId, TConfigurationItem>(
    UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem> unfoldedCircleWebSocketHandler,
    IHostApplicationLifetime applicationLifetime,
    ILoggerFactory loggerFactory,
    ILogger<UnfoldedCircleMiddleware<TUnfoldedCircleWebSocketHandler, TMediaPlayerCommandId, TConfigurationItem>> logger) : IMiddleware
    where TUnfoldedCircleWebSocketHandler : UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>
    where TMediaPlayerCommandId : struct, Enum
    where TConfigurationItem : UnfoldedCircleConfigurationItem
{
    private readonly UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem> _unfoldedCircleWebSocketHandler = unfoldedCircleWebSocketHandler;
    private readonly IHostApplicationLifetime _applicationLifetime = applicationLifetime;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private readonly ILogger<UnfoldedCircleMiddleware<TUnfoldedCircleWebSocketHandler, TMediaPlayerCommandId, TConfigurationItem>> _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var socket = await context.WebSockets.AcceptWebSocketAsync();
                var wsId = $"{context.Connection.RemoteIpAddress?.ToString()}:{context.Connection.RemotePort.ToString(NumberFormatInfo.InvariantInfo)}";

                _logger.WebSocketNewConnection(wsId);

                using var cancellationTokenWrapper = new CancellationTokenWrapper(_loggerFactory.CreateLogger<CancellationTokenWrapper>(), _applicationLifetime.ApplicationStopping, context.RequestAborted);
                var result = await _unfoldedCircleWebSocketHandler.HandleWebSocketAsync(socket, wsId, cancellationTokenWrapper);
                await socket.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, context.RequestAborted);

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