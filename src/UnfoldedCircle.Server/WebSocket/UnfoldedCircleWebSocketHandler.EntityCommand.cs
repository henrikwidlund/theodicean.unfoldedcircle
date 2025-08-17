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
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    /// <remarks>You must emit power on/off events accordingly.</remarks>
    protected abstract ValueTask<EntityCommandResult> OnRemoteCommandAsync(
        System.Net.WebSockets.WebSocket socket,
        RemoteEntityCommandMsgData payload,
        string command,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper);

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
    protected IEnumerable<(string EntityId, EntityType EntitType)> GetEntities(string entityId)
        => SupportedEntityTypes.Select(supportedEntityType => (entityId.GetIdentifier(supportedEntityType), supportedEntityType));

    /// <summary>
    /// Executed when a <c>entity_command</c> is received for a media player entity.
    /// </summary>
    /// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> that the request was sent to.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationTokenWrapper">The <see cref="CancellationTokenWrapper"/> for the session.</param>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    protected abstract ValueTask<EntityCommandResult> OnMediaPlayerCommandAsync(System.Net.WebSockets.WebSocket socket,
        MediaPlayerEntityCommandMsgData<TMediaPlayerCommandId> payload,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper);

    /// <summary>
    /// Enum representing the result of an entity command.
    /// </summary>
    protected enum EntityCommandResult
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
        CancellationTokenWrapper cancellationTokenWrapper)
    {
        if (!await IsEntityReachableAsync(wsId, payload.MsgData.EntityId, cancellationTokenWrapper.RequestAborted))
        {
            await SendMessageAsync(socket,
                ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                    new ValidationError
                    {
                        Code = "INV_ARGUMENT",
                        Message = "Could not reach entity"
                    }),
                wsId,
                cancellationTokenWrapper.RequestAborted);
            return;
        }

        if (payload is MediaPlayerEntityCommandMsgData<TMediaPlayerCommandId> mediaPlayerEntityCommandMsgData)
            await HandleMediaPlayerCommandAsync(socket, mediaPlayerEntityCommandMsgData, wsId, cancellationTokenWrapper);
        else if (payload is RemoteEntityCommandMsgData remoteEntityCommandMsgData)
            await HandleRemoteCommandAsync(socket, remoteEntityCommandMsgData, wsId, cancellationTokenWrapper);
        else
            _logger.LogWarning("[{WSId}] WS: Unknown entity command type {PayloadType}",
                wsId, payload.GetType().Name);
    }

    private async Task HandleMediaPlayerCommandAsync(System.Net.WebSockets.WebSocket socket,
        MediaPlayerEntityCommandMsgData<TMediaPlayerCommandId> payload,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper)
    {
        var entityCommandResult = await OnMediaPlayerCommandAsync(socket, payload, wsId, cancellationTokenWrapper);
        if (entityCommandResult != EntityCommandResult.Failure)
        {
            await HandleCommandResultCoreAsync(socket, wsId, payload, entityCommandResult, cancellationTokenWrapper);
        }
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
                cancellationTokenWrapper.RequestAborted);
        }
    }

    private async Task HandleRemoteCommandAsync(System.Net.WebSockets.WebSocket socket,
        RemoteEntityCommandMsgData payload,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper)
    {
        try
        {
            var entityCommandResult = payload.MsgData.CommandId switch
            {
                "on" => await OnRemoteCommandAsync(socket, payload, "on", wsId, cancellationTokenWrapper),
                "off" => await OnRemoteCommandAsync(socket, payload, "off", wsId, cancellationTokenWrapper),
                "toggle" => await OnRemoteCommandAsync(socket, payload, "toggle", wsId, cancellationTokenWrapper),
                "send_cmd" => await HandleSendCommandAsync(socket, payload, wsId, cancellationTokenWrapper),
                "send_cmd_sequence" => await HandleSendCommandSequenceAsync(socket, payload, wsId, cancellationTokenWrapper),
                _ => EntityCommandResult.Failure
            };

            if (entityCommandResult != EntityCommandResult.Failure)
            {
                await HandleCommandResultCoreAsync(socket, wsId, payload, entityCommandResult, cancellationTokenWrapper);
            }
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
                    cancellationTokenWrapper.RequestAborted);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[{WSId}] WS: Error while handling entity command {EntityCommand}", wsId, payload.MsgData);
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
        CancellationTokenWrapper cancellationTokenWrapper)
    {
        await SendMessageAsync(socket,
            ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
            wsId,
            cancellationTokenWrapper.RequestAborted);

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
                    ResponsePayloadHelpers.CreateStateChangedResponsePayload(
                        new MediaPlayerStateChangedEventMessageDataAttributes { State = entityCommandResult == EntityCommandResult.PowerOn ? State.On : State.Off },
                        entity.EntityId.GetIdentifier(EntityType.MediaPlayer),
                        EntityType.MediaPlayer),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
            }
            else if (entity.EntitType == EntityType.Remote)
            {
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateStateChangedResponsePayload(
                        new RemoteStateChangedEventMessageDataAttributes { State = entityCommandResult == EntityCommandResult.PowerOn ? RemoteState.On :RemoteState.Off },
                        entity.EntityId.GetIdentifier(EntityType.Remote),
                        EntityType.Remote),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
            }
            else
            {
                _logger.LogError("[{WSId}] WS: Unsupported entity type {EntityType} for entity {EntityId}.",
                    wsId, entity.EntitType.ToString(), entity.EntityId);
            }

            if (!IsBroadcastingEvents(entity.EntityId))
                _ = Task.Factory.StartNew(() => HandleEventUpdatesAsync(socket, entity.EntityId, wsId, cancellationTokenWrapper),
                    TaskCreationOptions.LongRunning);
        }
    }

    private async Task<EntityCommandResult> HandleSendCommandAsync(
        System.Net.WebSockets.WebSocket socket,
        RemoteEntityCommandMsgData payload,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper)
    {
        var command = payload.MsgData.Params?.Command;
        if (string.IsNullOrEmpty(command))
            return EntityCommandResult.Failure;

        var delay = payload.MsgData.Params?.Delay ?? 0;
        if (payload.MsgData.Params?.Repeat.HasValue is true)
        {
            for (var i = 0; i < payload.MsgData.Params.Repeat.Value; i++)
            {
                await OnRemoteCommandAsync(socket, payload, command, wsId, cancellationTokenWrapper);
                if (delay> 0)
                    await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationTokenWrapper.RequestAborted);
            }
        }
        else
        {
            await OnRemoteCommandAsync(socket, payload, command, wsId, cancellationTokenWrapper);
        }

        return EntityCommandResult.Other;
    }

    private async Task<EntityCommandResult> HandleSendCommandSequenceAsync(System.Net.WebSockets.WebSocket socket,
        RemoteEntityCommandMsgData payload,
        string wsId,
        CancellationTokenWrapper cancellationTokenWrapper)
    {
        if (payload.MsgData.Params is not { Sequence: { Length: > 0 } sequence })
            return EntityCommandResult.Failure;

        var delay = payload.MsgData.Params?.Delay ?? 0;
        var shouldRepeat = payload.MsgData.Params?.Repeat.HasValue is true;
        foreach (var command in sequence.Where(static x => !string.IsNullOrEmpty(x)))
        {
            if (shouldRepeat)
            {
                for (var i = 0; i < payload.MsgData.Params!.Repeat!.Value; i++)
                {
                    await OnRemoteCommandAsync(socket, payload, command, wsId, cancellationTokenWrapper);
                    if (delay > 0)
                        await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationTokenWrapper.RequestAborted);
                }
            }
            else
            {
                await OnRemoteCommandAsync(socket, payload, command, wsId, cancellationTokenWrapper);
                if (delay > 0)
                    await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationTokenWrapper.RequestAborted);
            }
        }

        return EntityCommandResult.Other;
    }
}