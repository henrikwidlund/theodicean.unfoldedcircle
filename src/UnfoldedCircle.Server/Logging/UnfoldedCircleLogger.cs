using Microsoft.Extensions.Logging;

using UnfoldedCircle.Models.Shared;
using UnfoldedCircle.Models.Sync;
using UnfoldedCircle.Server.Configuration;
using UnfoldedCircle.Server.Event;
using UnfoldedCircle.Server.WebSocket;

namespace UnfoldedCircle.Server.Logging;

internal static partial class UnfoldedCircleLogger
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Broadcast canceled for {@Entities}")]
    public static partial void BroadcastCanceled(this ILogger logger, IEnumerable<string> entities);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "[{WSId}] WS: New connection")]
    public static partial void WebSocketNewConnection(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "[{WSId}] WS: Connection closed")]
    public static partial void WebSocketConnectionClosed(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "An error occurred while handling WebSocket connection")]
    public static partial void UnfoldedCircleMiddlewareException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5, Level = LogLevel.Trace, Message = "[{WSId}] WS: Sending message '{Message}'")]
    public static partial void SendingMessage(this ILogger logger, string wsId, string message);

    [LoggerMessage(EventId = 6, Level = LogLevel.Trace, Message = "[{WSId}] WS: Received message '{Message}'")]
    public static partial void ReceivedMessage(this ILogger logger, string wsId, string message);

    [LoggerMessage(EventId = 7, Level = LogLevel.Trace, Message = "[{WSId}] WS: Received message is not JSON.")]
    public static partial void NotJson(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "[{WSId}] WS: Received message does not contain 'msg' property.")]
    public static partial void MissingMessageProperty(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "[{WSId}] WS: Unknown msg type '{Message}")]
    public static partial void UnknownMessageType(this ILogger logger, string wsId, string? message);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information,
        Message = "[{WSId}] WS: Received message does not contain 'kind' property.")]
    public static partial void MissingKindProperty(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 11, Level = LogLevel.Error, Message = "[{WSId}] WS: Error while handling message.")]
    public static partial void HandleWebSocketAsyncException(this ILogger logger, string wsId, Exception exception);

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "[{WSId}] Removing entity {@Entity}")]
    public static partial void RemovingEntity(this ILogger logger, string wsId, UnfoldedCircleConfigurationItem entity);

    [LoggerMessage(EventId = 13, Level = LogLevel.Warning, Message = "[{WSId}] WS: Unknown entity command type {PayloadType}")]
    public static partial void UnknownEntityCommand(this ILogger logger, string wsId, string payloadType);

    [LoggerMessage(EventId = 14, Level = LogLevel.Error, Message = "[{WSId}] WS: Error while handling remote entity command {@MsgData}")]
    public static partial void RemoteEntityCommandHandlingException(this ILogger logger, string wsId, EntityCommandMsgData<string, RemoteEntityCommandParams> msgData, Exception exception);

    [LoggerMessage(EventId = 15, Level = LogLevel.Error, Message = "[{WSId}] WS: Unsupported entity type {EntityType} for entity {EntityId}.")]
    public static partial void UnsupportedEntityTypeWithEntityId(this ILogger logger, string wsId, EntityType entityType, string entityId);

    [LoggerMessage(EventId = 16, Level = LogLevel.Information, Message = "[{WSId}] WS: Removed configuration for {EntityId}")]
    public static partial void RemovedConfiguration(this ILogger logger, string wsId, string entityId);

    [LoggerMessage(EventId = 18, Level = LogLevel.Error, Message = "[{WSId}] WS: Setup driver failed. MsgData: {@MsgData}.")]
    public static partial void DriverSetupFailed(this ILogger logger, string wsId, SetupDriverMsgData msgData);

    [LoggerMessage(EventId = 19, Level = LogLevel.Error,
        Message = "[{WSId}] WS: Setup driver user input required but no next setup step provided. Setup will be aborted. MsgData: {@MsgData}.")]
    public static partial void UserInputNoNextStep(this ILogger logger, string wsId, SetupDriverMsgData msgData);

    [LoggerMessage(EventId = 20, Level = LogLevel.Error, Message = "[{WSId}] WS: Unsupported entity type {EntityType}.")]
    public static partial void UnsupportedEntityType(this ILogger logger, string wsId, EntityType? entityType);

    [LoggerMessage(EventId = 21, Level = LogLevel.Error, Message = "[{WSId}] No setup step found.")]
    public static partial void NoSetupStepFound(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 22, Level = LogLevel.Error, Message = "[{WSId}] No confirm or input_values found in payload.")]
    public static partial void NoConfirmOrInputValuesFound(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 23, Level = LogLevel.Error, Message = "[{WSId}] No entity ID found during save reconfigured entity step.")]
    public static partial void NoEntityIdFoundSaveReconfigure(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 24, Level = LogLevel.Error, Message = "[{WSId}] WS: Could not find entity with ID: {EntityId}.")]
    public static partial void EntityWithIdNotFound(this ILogger logger, string wsId, string entityId);

    [LoggerMessage(EventId = 25, Level = LogLevel.Error, Message = "[{WSId}] No valid setup step found. Current step: {SetupStep}.")]
    public static partial void NoValidSetupStepFound(this ILogger logger, string wsId, SetupStep setupStep);

    [LoggerMessage(EventId = 26, Level = LogLevel.Error, Message = "[{WSId}] Error during setup process.")]
    public static partial void ErrorDuringSetupProcess(this ILogger logger, string wsId, Exception exception);

    [LoggerMessage(EventId = 30, Level = LogLevel.Information, Message = "[{WSId}] WS: Resetting event processing due to task status {TaskStatus}.")]
    public static partial void ResettingEventProcessing(this ILogger logger, string wsId, TaskStatus taskStatus);

    [LoggerMessage(EventId = 31, Level = LogLevel.Error, Message = "[{WSId}] Unhandled exception during event.")]
    public static partial void UnhandledExceptionDuringEvent(this ILogger logger, string wsId, Exception exception);

    [LoggerMessage(EventId = 33, Level = LogLevel.Error, Message = "[{WSId}] Unhandled exception during start event.")]
    public static partial void UnhandledExceptionDuringStartEvent(this ILogger logger, string wsId, Exception exception);

    [LoggerMessage(EventId = 34, Level = LogLevel.Error, Message = "[{WSId}] Unhandled exception during stop event.")]
    public static partial void UnhandledExceptionDuringStopEvent(this ILogger logger, string wsId, Exception exception);

    [LoggerMessage(EventId = 35, Level = LogLevel.Information, Message = "[{WSId}] WS: Failed to acquire semaphore lock for start event within the timeout.")]
    public static partial void StartEventProcessorSemaphoreTimeout(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 36, Level = LogLevel.Information, Message = "[{WSId}] WS: Failed to acquire semaphore lock for stop event within the timeout.")]
    public static partial void StopEventProcessorSemaphoreTimeout(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 37, Level = LogLevel.Information, Message = "[{WSId}] WS: Events are already running.")]
    public static partial void EventProcessingAlreadyRunning(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 38, Level = LogLevel.Error, Message = "[{WSId}] WS: Error while handling climate entity command {@MsgData}")]
    public static partial void ClimateEntityCommandHandlingException(this ILogger logger, string wsId, EntityCommandMsgData<ClimateCommandId, ClimateEntityCommandParams> msgData, Exception exception);

    [LoggerMessage(EventId = 39, Level = LogLevel.Error, Message = "[{WSId}] WS: Error while handling select entity command {@MsgData}")]
    public static partial void SelectEntityCommandHandlingException(this ILogger logger, string wsId, EntityCommandMsgData<SelectCommandId, SelectEntityCommandParams> msgData, Exception exception);

    [LoggerMessage(EventId = 40, Level = LogLevel.Warning, Message = "[{WSId}] WS: Received message is too large to process.")]
    public static partial void MessageTooLarge(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 41, Level = LogLevel.Warning, Message = "[{WSId}] WS: Unhandled message event '{MessageEvent}'.")]
    public static partial void UnhandledMessageEvent(this ILogger logger, string wsId, MessageEvent messageEvent);
}
