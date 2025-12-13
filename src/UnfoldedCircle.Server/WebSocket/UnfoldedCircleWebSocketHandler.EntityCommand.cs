using Microsoft.Extensions.Logging;

using UnfoldedCircle.Models.Events;
using UnfoldedCircle.Models.Shared;
using UnfoldedCircle.Models.Sync;
using UnfoldedCircle.Server.Extensions;
using UnfoldedCircle.Server.Response;

namespace UnfoldedCircle.Server.WebSocket;

public abstract partial class UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>
{
    /// <summary>
    /// Executed when a <c>entity_command</c> is received for a media player entity.
    /// </summary>
    /// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> that the request was sent to.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="command">The identifier for the command to execute.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationTokenWrapper">The <see cref="CancellationTokenWrapper"/> for the session.</param>
    /// <param name="commandCancellationToken">The <see cref="CancellationToken"/> for when commands should be aborted.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    /// <remarks>You must emit power on/off events accordingly.</remarks>
    protected abstract ValueTask<EntityCommandResult> OnRemoteCommandAsync(
        System.Net.WebSockets.WebSocket socket,
        RemoteEntityCommandMsgData payload,
        string command,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper,
        CancellationToken commandCancellationToken);

    /// <summary>
    /// Determines if the entity with the given <paramref name="entityId"/> is reachable.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="entityId">The entity_id.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask<bool> IsEntityReachableAsync(
        string wsId,
        string entityId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns all the different variants of entities that are valid for the given <paramref name="entityId"/>.
    /// </summary>
    /// <param name="entityId">The entity_id to return values for.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    protected IEnumerable<(string EntityId, EntityType EntitType)> GetEntities(string entityId)
        => SupportedEntityTypes.Select(supportedEntityType => (entityId.GetIdentifier(supportedEntityType),
            supportedEntityType));

    /// <summary>
    /// Executed when a <c>entity_command</c> is received for a media player entity.
    /// </summary>
    /// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> that the request was sent to.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationTokenWrapper">The <see cref="CancellationTokenWrapper"/> for the session.</param>
    /// <param name="commandCancellationToken">The <see cref="CancellationToken"/> for when commands should be aborted.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    protected abstract ValueTask<EntityCommandResult> OnMediaPlayerCommandAsync(System.Net.WebSockets.WebSocket socket,
        MediaPlayerEntityCommandMsgData<TMediaPlayerCommandId> payload,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper,
        CancellationToken commandCancellationToken);

    /// <summary>
    /// Enum representing the result of an entity command.
    /// </summary>
    protected enum EntityCommandResult : sbyte
    {
        /// <summary>
        /// The command was successful and the entity was powered on.
        /// </summary>
        PowerOn,

        /// <summary>
        /// The command was successful and the entity was powered off.
        /// </summary>
        PowerOff,

        /// <summary>
        /// Command was handled and response was sent to the remote.
        /// </summary>
        Handled,

        /// <summary>
        /// The command was successful.
        /// </summary>
        Other,

        /// <summary>
        /// The command failed to execute.
        /// </summary>
        Failure
    }

    private async Task HandleEntityCommandAsync<TCommandId, TEntityCommandParams>(
        System.Net.WebSockets.WebSocket socket,
        CommonReq<EntityCommandMsgData<TCommandId, TEntityCommandParams>> payload,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper,
        CancellationToken commandCancellationToken)
    {
        if (!await IsEntityReachableAsync(wsId, payload.MsgData.EntityId.GetBaseIdentifier(), commandCancellationToken))
        {
            await SendMessageAsync(socket,
                ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                    new ValidationError
                    {
                        Code = "ENTITY_UNAVAILABLE",
                        Message = "Could not reach entity"
                    }),
                wsId,
                commandCancellationToken);
            return;
        }

        switch (payload)
        {
            case MediaPlayerEntityCommandMsgData<TMediaPlayerCommandId> mediaPlayerEntityCommandMsgData:
                await HandleMediaPlayerCommandAsync(socket, mediaPlayerEntityCommandMsgData, wsId, cancellationTokenWrapper, commandCancellationToken);
                break;
            case RemoteEntityCommandMsgData remoteEntityCommandMsgData:
                await HandleRemoteCommandAsync(socket, remoteEntityCommandMsgData, wsId, cancellationTokenWrapper, commandCancellationToken);
                break;
            default:
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.UnknownEntityCommand(wsId, payload.GetType().Name);
                break;
        }
    }

    private async Task HandleMediaPlayerCommandAsync(System.Net.WebSockets.WebSocket socket,
        MediaPlayerEntityCommandMsgData<TMediaPlayerCommandId> payload,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper,
        CancellationToken commandCancellationToken)
    {
        var entityCommandResult = await OnMediaPlayerCommandAsync(socket, payload, wsId, cancellationTokenWrapper, commandCancellationToken);
        if (entityCommandResult != EntityCommandResult.Failure)
            await HandleCommandResultCoreAsync(socket, wsId, payload, entityCommandResult, cancellationTokenWrapper, commandCancellationToken);
        else
        {
            await SendMessageAsync(socket,
                ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                    new ValidationError
                    {
                        Code = "INV_ARGUMENT",
                        Message = "Unknown command"
                    }),
                wsId,
                commandCancellationToken);
        }
    }

    private async Task HandleRemoteCommandAsync(System.Net.WebSockets.WebSocket socket,
        RemoteEntityCommandMsgData payload,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper,
        CancellationToken commandCancellationToken)
    {
        try
        {
            var entityCommandResult = payload.MsgData.CommandId switch
            {
                "on" => await OnRemoteCommandAsync(socket, payload, "on", wsId, cancellationTokenWrapper, commandCancellationToken),
                "off" => await OnRemoteCommandAsync(socket, payload, "off", wsId, cancellationTokenWrapper, commandCancellationToken),
                "toggle" => await OnRemoteCommandAsync(socket, payload, "toggle", wsId, cancellationTokenWrapper, commandCancellationToken),
                "send_cmd" => await HandleSendCommandAsync(socket, payload, wsId, cancellationTokenWrapper, commandCancellationToken),
                "send_cmd_sequence" => await HandleSendCommandSequenceAsync(socket, payload, wsId, cancellationTokenWrapper, commandCancellationToken),
                _ => EntityCommandResult.Failure
            };

            if (entityCommandResult is not EntityCommandResult.Failure and not EntityCommandResult.Handled)
                await HandleCommandResultCoreAsync(socket, wsId, payload, entityCommandResult, cancellationTokenWrapper, commandCancellationToken);
            else if (entityCommandResult is EntityCommandResult.Failure)
            {
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                        new ValidationError
                        {
                            Code = "INV_ARGUMENT",
                            Message = "Unknown command"
                        }),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
            }
        }
        catch (Exception e)
        {
            _logger.EntityCommandHandlingException(wsId, payload.MsgData, e);

            await SendMessageAsync(socket,
                ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                    new ValidationError
                    {
                        Code = "ERROR",
                        Message = "Error while handling command"
                    }),
                wsId,
                cancellationTokenWrapper.RequestAborted);
        }
    }

    private async Task HandleCommandResultCoreAsync<TCommandId, TEntityCommandParams>(System.Net.WebSockets.WebSocket socket,
        string wsId,
        CommonReq<EntityCommandMsgData<TCommandId, TEntityCommandParams>> payload,
        EntityCommandResult entityCommandResult,
        CancellationTokenWrapper cancellationTokenWrapper,
        CancellationToken commandCancellationToken)
    {
        await SendMessageAsync(socket,
            ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
            wsId,
            commandCancellationToken);

        if (entityCommandResult is EntityCommandResult.PowerOn or EntityCommandResult.PowerOff)
        {
            await Task.WhenAll(GetEntities(payload.MsgData.EntityId)
                .Select(SendPowerStatusAndBroadcastAsync));
        }

        return;

        async Task SendPowerStatusAndBroadcastAsync((string EntityId, EntityType EntitType) entity)
        {
            if (entity.EntitType == EntityType.MediaPlayer)
            {
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateMediaPlayerStateChangedResponsePayload(
                        new MediaPlayerStateChangedEventMessageDataAttributes { State = entityCommandResult == EntityCommandResult.PowerOn ? State.On : State.Off },
                        entity.EntityId.GetIdentifier(EntityType.MediaPlayer)),
                    wsId,
                    commandCancellationToken);
            }
            else if (entity.EntitType == EntityType.Remote)
            {
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateRemoteStateChangedResponsePayload(
                        new RemoteStateChangedEventMessageDataAttributes { State = entityCommandResult == EntityCommandResult.PowerOn ? RemoteState.On :RemoteState.Off },
                        entity.EntityId.GetIdentifier(EntityType.Remote)),
                    wsId,
                    commandCancellationToken);
            }
            else if (entity.EntitType == EntityType.Sensor)
            {
                if (SessionHolder.SensorTypesMap.TryGetValue(entity.EntityId.GetBaseIdentifier(), out var sensorTypes))
                {
                    await Task.WhenAll(sensorTypes.Select(x => SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateSensorStateChangedResponsePayload<string>(
                            new SensorStateChangedEventMessageDataAttributes<string> { State = SensorState.On, Value = null },
                            entity.EntityId.GetIdentifier(EntityType.Sensor, x),
                            x),
                        wsId,
                        commandCancellationToken)));
                }
            }
            else
                _logger.UnsupportedEntityTypeWithEntityId(wsId, entity.EntitType, entity.EntityId);

            _ = Task.Factory.StartNew(() => HandleEventUpdatesAsync(socket, entity.EntityId.GetBaseIdentifier(), wsId, cancellationTokenWrapper),
                TaskCreationOptions.LongRunning);
        }
    }

    private async Task<EntityCommandResult> HandleSendCommandAsync(
        System.Net.WebSockets.WebSocket socket,
        RemoteEntityCommandMsgData payload,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper,
        CancellationToken commandCancellationToken)
    {
        var command = payload.MsgData.Params?.Command;
        if (string.IsNullOrEmpty(command))
            return EntityCommandResult.Failure;

        var delay = payload.MsgData.Params?.Delay ?? 0;
        EntityCommandResult? commandResult = null;
        payload = GetEntityCommandWithFixedRepeat(payload);
        if (payload.MsgData.Params?.Repeat.HasValue is true)
        {
            // Acknowledge the command early if more than two repeats to avoid errors caused by timeouts on the remote side.
            if (payload.MsgData.Params!.Repeat > 2)
            {
                await HandleCommandResultCoreAsync(socket, wsId, payload, EntityCommandResult.Other, cancellationTokenWrapper, commandCancellationToken);
                commandResult = EntityCommandResult.Handled;
            }

            var entityId = payload.MsgData.EntityId.AsMemory().GetBaseIdentifier();
            await SafeCancelRepeat(entityId);

            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var lookup = SessionHolder.CurrentRepeatCommandMap.GetAlternateLookup<ReadOnlySpan<char>>();
            lookup[entityId.Span] = cancellationTokenSource;
            for (var i = 0; i < payload.MsgData.Params.Repeat; i++)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                    break;

                // ReSharper disable once PossiblyMistakenUseOfCancellationToken
                await OnRemoteCommandAsync(socket, payload, command, wsId, cancellationTokenWrapper, cancellationTokenWrapper.RequestAborted);
                if (delay> 0)
                    await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationTokenWrapper.RequestAborted);
            }
        }
        else
            await OnRemoteCommandAsync(socket, payload, command, wsId, cancellationTokenWrapper, commandCancellationToken);

        return commandResult ?? EntityCommandResult.Other;
    }

    private async Task<EntityCommandResult> HandleSendCommandSequenceAsync(System.Net.WebSockets.WebSocket socket,
        RemoteEntityCommandMsgData payload,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper,
        CancellationToken commandCancellationToken)
    {
        if (payload.MsgData.Params is not { Sequence: { Length: > 0 } sequence })
            return EntityCommandResult.Failure;

        var delay = payload.MsgData.Params?.Delay ?? 0;
        var shouldRepeat = payload.MsgData.Params?.Repeat.HasValue is true;
        var entityId = payload.MsgData.EntityId.AsMemory().GetBaseIdentifier();
        await SafeCancelRepeat(entityId);

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        var lookup = SessionHolder.CurrentRepeatCommandMap.GetAlternateLookup<ReadOnlySpan<char>>();
        lookup[entityId.Span] = cancellationTokenSource;
        EntityCommandResult? commandResult = null;
        if (sequence.Length > 2)
        {
            // Acknowledge the command early if more than two commands to avoid errors caused by timeouts on the remote side.
            // ReSharper disable once PossiblyMistakenUseOfCancellationToken
            await HandleCommandResultCoreAsync(socket, wsId, payload, EntityCommandResult.Other, cancellationTokenWrapper, commandCancellationToken);
            commandResult = EntityCommandResult.Handled;
        }

        payload = GetEntityCommandWithFixedRepeat(payload);

        foreach (var command in sequence.Where(static x => !string.IsNullOrEmpty(x)))
        {
            if (shouldRepeat)
            {
                for (var i = 0; i < payload.MsgData.Params!.Repeat!.Value; i++)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                        break;

                    await OnRemoteCommandAsync(socket, payload, command, wsId, cancellationTokenWrapper, cancellationTokenWrapper.RequestAborted);
                    if (delay > 0)
                        await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationTokenWrapper.RequestAborted);
                }

                if (cancellationTokenSource.IsCancellationRequested)
                    break;
            }
            else
            {
                await OnRemoteCommandAsync(socket, payload, command, wsId, cancellationTokenWrapper, cancellationTokenWrapper.RequestAborted);
                if (delay > 0)
                    await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationTokenWrapper.RequestAborted);
            }

            if (cancellationTokenSource.IsCancellationRequested)
                break;
        }

        return commandResult ?? EntityCommandResult.Other;
    }

    /// <summary>
    /// Temporary workaround for always repeating 4 times due to bug in core
    /// </summary>
    private static RemoteEntityCommandMsgData GetEntityCommandWithFixedRepeat(RemoteEntityCommandMsgData payload) =>
        payload.MsgData.Params?.Repeat == 4
            ? payload with { MsgData = payload.MsgData with { Params = payload.MsgData.Params with { Repeat = 1 } } }
            : payload;

    private static async Task SafeCancelRepeat(ReadOnlyMemory<char> entityId)
    {
        var lookup = SessionHolder.CurrentRepeatCommandMap.GetAlternateLookup<ReadOnlySpan<char>>();
        if (lookup.TryGetValue(entityId.Span, out var value))
        {
            try
            {
                await value.CancelAsync();
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }
    }
}