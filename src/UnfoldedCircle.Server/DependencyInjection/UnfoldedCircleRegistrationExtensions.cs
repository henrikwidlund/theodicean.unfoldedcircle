using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using UnfoldedCircle.Models.Sync;
using UnfoldedCircle.Server.Configuration;
using UnfoldedCircle.Server.DependencyInjection;
using UnfoldedCircle.Server.Dns;
using UnfoldedCircle.Server.Extensions;
using UnfoldedCircle.Server.WebSocket;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for registering the Unfolded Circle server in an ASP.NET Core application.
/// </summary>
// ReSharper disable once UnusedType.Global
public static class UnfoldedCircleRegistrationExtensions
{
    /// <param name="builder">The <see cref="WebApplicationBuilder"/>.</param>
    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// Adds the Unfolded Circle server to the application builder.
        /// </summary>
        /// <param name="configureOptions">Optional configuration options for the server.</param>
        /// <typeparam name="TUnfoldedCircleWebSocketHandler">The type of socket handler to use.</typeparam>
        /// <typeparam name="TConfigurationService">The type of configuration service to use.</typeparam>
        /// <typeparam name="TConfigurationItem">The type of configuration item to use.</typeparam>
        /// <returns>A <see cref="WebApplicationBuilder"/> with the Unfolded Circle server added to it.</returns>
        // ReSharper disable once UnusedMember.Global
        public WebApplicationBuilder AddUnfoldedCircleServer<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TUnfoldedCircleWebSocketHandler,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TConfigurationService,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TConfigurationItem>(Action<UnfoldedCircleOptions>? configureOptions = null)
            where TConfigurationItem : UnfoldedCircleConfigurationItem
            where TConfigurationService : class, IConfigurationService<TConfigurationItem>
            where TUnfoldedCircleWebSocketHandler : UnfoldedCircleWebSocketHandler<MediaPlayerCommandId, TConfigurationItem> =>
            builder.AddUnfoldedCircleServer<
                TUnfoldedCircleWebSocketHandler,
                MediaPlayerCommandId,
                TConfigurationService,
                TConfigurationItem>(configureOptions);

        /// <summary>
        /// Adds the Unfolded Circle server to the application builder.
        /// </summary>
        /// <param name="configureOptions">Optional configuration options for the server.</param>
        /// <typeparam name="TUnfoldedCircleWebSocketHandler">The type of socket handler to use.</typeparam>
        /// <typeparam name="TMediaPlayerCommandId">The type of media player command id to use.</typeparam>
        /// <typeparam name="TConfigurationService">The type of configuration service to use.</typeparam>
        /// <typeparam name="TConfigurationItem">The type of configuration item to use.</typeparam>
        /// <returns>A <see cref="WebApplicationBuilder"/> with the Unfolded Circle server added to it.</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public WebApplicationBuilder AddUnfoldedCircleServer<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TUnfoldedCircleWebSocketHandler,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TMediaPlayerCommandId,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TConfigurationService,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TConfigurationItem>(Action<UnfoldedCircleOptions>? configureOptions = null)
            where TConfigurationItem : UnfoldedCircleConfigurationItem
            where TConfigurationService : class, IConfigurationService<TConfigurationItem>
            where TMediaPlayerCommandId : struct, Enum
            where TUnfoldedCircleWebSocketHandler : UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>
        {
            var unfoldedCircleOptions = new UnfoldedCircleOptions
            {
                MaxMessageHandlingWaitTimeInSeconds = builder.Configuration.GetOrDefault("UC_MAX_MESSAGE_HANDLING_WAIT_TIME_IN_SECONDS", 9.5)
            };
            configureOptions?.Invoke(unfoldedCircleOptions);
            if (unfoldedCircleOptions.DisableEntityIdPrefixing)
                ValueExtensions.DisableEntityIdPrefixing = true;

            builder.Services.AddOptions<UnfoldedCircleOptions>();
            if (configureOptions is not null)
                builder.Services.PostConfigure(configureOptions);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(builder.Configuration.GetOrDefault("UC_INTEGRATION_HTTP_PORT", unfoldedCircleOptions.ListeningPort));
                options.AddServerHeader = false;
            });

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ConsoleFormatter, CustomSystemdConsoleFormatter>());

            if (OperatingSystem.IsLinux())
                builder.Logging.AddConsole(static options =>
                {
                    options.FormatterName = "CustomSystemd";
                });

            builder.Services.AddSingleton<IConfigurationService<TConfigurationItem>, TConfigurationService>();
            builder.Services.AddHostedService<MDnsBackgroundService<TConfigurationItem>>();

            builder.Services.AddSingleton<UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>, TUnfoldedCircleWebSocketHandler>();
            builder.Services.AddSingleton<UnfoldedCircleMiddleware<TUnfoldedCircleWebSocketHandler, TMediaPlayerCommandId, TConfigurationItem>>();

            return builder;
        }
    }

    /// <param name="builder">The <see cref="IApplicationBuilder"/>.</param>
    extension(IApplicationBuilder builder)
    {
        /// <summary>
        /// Uses the Unfolded Circle server middleware in the application pipeline.
        /// </summary>
        /// <param name="webSocketOptions">Optional options to customize the websocket behaviour.</param>
        /// <typeparam name="TUnfoldedCircleWebSocketHandler">The type of socket handler to use.</typeparam>
        /// <typeparam name="TConfigurationItem">The type of configuration item to use.</typeparam>
        // ReSharper disable once UnusedMember.Global
        public IApplicationBuilder UseUnfoldedCircleServer<TUnfoldedCircleWebSocketHandler, TConfigurationItem>(WebSocketOptions? webSocketOptions = null)
            where TUnfoldedCircleWebSocketHandler : UnfoldedCircleWebSocketHandler<MediaPlayerCommandId, TConfigurationItem>
            where TConfigurationItem : UnfoldedCircleConfigurationItem =>
            builder.UseUnfoldedCircleServer<TUnfoldedCircleWebSocketHandler, MediaPlayerCommandId, TConfigurationItem>(webSocketOptions);

        /// <summary>
        /// Uses the Unfolded Circle server middleware in the application pipeline.
        /// </summary>
        /// <param name="webSocketOptions">Optional options to customize the websocket behaviour.</param>
        /// <typeparam name="TUnfoldedCircleWebSocketHandler">The type of socket handler to use.</typeparam>
        /// <typeparam name="TMediaPlayerCommandId">The type of media player command id to use.</typeparam>
        /// <typeparam name="TConfigurationItem">The type of configuration item to use.</typeparam>
        // ReSharper disable once MemberCanBePrivate.Global
        public IApplicationBuilder UseUnfoldedCircleServer<TUnfoldedCircleWebSocketHandler, TMediaPlayerCommandId, TConfigurationItem>(WebSocketOptions? webSocketOptions = null)
            where TUnfoldedCircleWebSocketHandler : UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>
            where TConfigurationItem : UnfoldedCircleConfigurationItem
            where TMediaPlayerCommandId : struct, Enum
        {
            webSocketOptions ??= new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            };

            builder.UseWebSockets(webSocketOptions);
            builder.UseMiddleware<UnfoldedCircleMiddleware<TUnfoldedCircleWebSocketHandler, TMediaPlayerCommandId, TConfigurationItem>>();

            return builder;
        }
    }
}