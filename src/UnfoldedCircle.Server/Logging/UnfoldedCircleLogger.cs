using Microsoft.Extensions.Logging;

using UnfoldedCircle.Models.Shared;
using UnfoldedCircle.Models.Sync;
using UnfoldedCircle.Server.Configuration;
using UnfoldedCircle.Server.WebSocket;

namespace UnfoldedCircle.Server.Logging;

internal static partial class UnfoldedCircleLogger
{
    [LoggerMessage(EventId = 1, EventName = nameof(BroadcastCancelled), Level = LogLevel.Information,
        Message = "Broadcast cancelled for {@Entities}")]
    public static partial void BroadcastCancelled(this ILogger logger, IEnumerable<string> entities);

    [LoggerMessage(EventId = 2, EventName = nameof(WebSocketNewConnection), Level = LogLevel.Debug,
        Message = "[{WSId}] WS: New connection")]
    public static partial void WebSocketNewConnection(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 3, EventName = nameof(WebSocketConnectionClosed), Level = LogLevel.Debug,
        Message = "[{WSId}] WS: Connection closed")]
    public static partial void WebSocketConnectionClosed(this ILogger logger, string wsId);

    private static readonly Action<ILogger, Exception> UnfoldedCircleMiddlewareExceptionAction = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(4, nameof(UnfoldedCircleMiddlewareException)),
        "An error occurred while handling WebSocket connection");

    public static void UnfoldedCircleMiddlewareException(this ILogger logger, Exception exception) =>
        UnfoldedCircleMiddlewareExceptionAction(logger, exception);

    [LoggerMessage(EventId = 5, EventName = nameof(SendingMessage), Level = LogLevel.Trace,
        Message = "[{WSId}] WS: Sending message '{Message}'")]
    public static partial void SendingMessage(this ILogger logger, string wsId, string message);

    [LoggerMessage(EventId = 6, EventName = nameof(ReceivedMessage), Level = LogLevel.Trace,
        Message = "[{WSId}] WS: Received message '{Message}'")]
    public static partial void ReceivedMessage(this ILogger logger, string wsId, string message);

    [LoggerMessage(EventId = 7, EventName = nameof(NotJson), Level = LogLevel.Trace,
        Message = "[{WSId}] WS: Received message is not JSON.")]
    public static partial void NotJson(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 8, EventName = nameof(MissingMessageProperty), Level = LogLevel.Debug,
        Message = "[{WSId}] WS: Received message does not contain 'msg' property.")]
    public static partial void MissingMessageProperty(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 9, EventName = nameof(UnknownMessageType), Level = LogLevel.Information,
        Message = "[{WSId}] WS: Unknown msg type '{Message}")]
    public static partial void UnknownMessageType(this ILogger logger, string wsId, string? message);

    [LoggerMessage(EventId = 10, EventName = nameof(MissingKindProperty), Level = LogLevel.Information,
        Message = "[{WSId}] WS: Received message does not contain 'kind' property.")]
    public static partial void MissingKindProperty(this ILogger logger, string wsId);

    private static readonly Action<ILogger, string, Exception> HandleWebSocketAsyncExceptionAction = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(11, nameof(HandleWebSocketAsyncException)),
        "[{WSId}] WS: Error while handling message.");

    public static void HandleWebSocketAsyncException(this ILogger logger, string wsId, Exception exception) =>
        HandleWebSocketAsyncExceptionAction(logger, wsId, exception);

    [LoggerMessage(EventId = 12, EventName = nameof(RemovingEntity), Level = LogLevel.Information,
        Message = "[{WSId}] Removing entity {@Entity}")]
    public static partial void RemovingEntity(this ILogger logger, string wsId, UnfoldedCircleConfigurationItem entity);

    [LoggerMessage(EventId = 13, EventName = nameof(UnknownEntityCommand), Level = LogLevel.Warning,
        Message = "[{WSId}] WS: Unknown entity command type {PayloadType}")]
    public static partial void UnknownEntityCommand(this ILogger logger, string wsId, string payloadType);

    private static readonly Action<ILogger, string, EntityCommandMsgData<string, RemoteEntityCommandParams>, Exception> EntityCommandHandlingExceptionAction = LoggerMessage.Define<string, EntityCommandMsgData<string, RemoteEntityCommandParams>>(
        LogLevel.Error,
        new EventId(14, nameof(EntityCommandHandlingException)),
        "[{WSId}] WS: Error while handling entity command {@MsgData}");

    public static void EntityCommandHandlingException(this ILogger logger, string wsId, EntityCommandMsgData<string, RemoteEntityCommandParams> msgData, Exception exception) =>
        EntityCommandHandlingExceptionAction(logger, wsId, msgData, exception);

    [LoggerMessage(EventId = 15, EventName = nameof(UnsupportedEntityTypeWithEntityId), Level = LogLevel.Error,
        Message = "[{WSId}] WS: Unsupported entity type {EntityType} for entity {EntityId}.")]
    public static partial void UnsupportedEntityTypeWithEntityId(this ILogger logger, string wsId, in EntityType entityType, string entityId);

    [LoggerMessage(EventId = 16, EventName = nameof(RemovedConfiguration), Level = LogLevel.Information,
        Message = "[{WSId}] WS: Removed configuration for {EntityId}")]
    public static partial void RemovedConfiguration(this ILogger logger, string wsId, string entityId);

    [LoggerMessage(EventId = 18, EventName = nameof(DriverSetupFailed), Level = LogLevel.Error,
        Message = "[{WSId}] WS: Setup driver failed. MsgData: {@MsgData}.")]
    public static partial void DriverSetupFailed(this ILogger logger, string wsId, SetupDriverMsgData msgData);

    [LoggerMessage(EventId = 19, EventName = nameof(UserInputNoNextStep), Level = LogLevel.Error,
        Message = "[{WSId}] WS: Setup driver user input required but no next setup step provided. Setup will be aborted. MsgData: {@MsgData}.")]
    public static partial void UserInputNoNextStep(this ILogger logger, string wsId, SetupDriverMsgData msgData);

    [LoggerMessage(EventId = 20, EventName = nameof(UnsupportedEntityType), Level = LogLevel.Error,
        Message = "[{WSId}] WS: Unsupported entity type {EntityType}.")]
    public static partial void UnsupportedEntityType(this ILogger logger, string wsId, EntityType? entityType);

    [LoggerMessage(EventId = 21, EventName = nameof(NoSetupStepFound), Level = LogLevel.Error,
        Message = "[{WSId}] No setup step found.")]
    public static partial void NoSetupStepFound(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 22, EventName = nameof(NoConfirmOrInputValuesFound), Level = LogLevel.Error,
        Message = "[{WSId}] No confirm or input_values found in payload.")]
    public static partial void NoConfirmOrInputValuesFound(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 23, EventName = nameof(NoEntityIdFoundSaveReconfigure), Level = LogLevel.Error,
        Message = "[{WSId}] No entity ID found during save reconfigured entity step.")]
    public static partial void NoEntityIdFoundSaveReconfigure(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 24, EventName = nameof(EntityWithIdNotFound), Level = LogLevel.Error,
        Message = "[{WSId}] WS: Could not find entity with ID: {EntityId}.")]
    public static partial void EntityWithIdNotFound(this ILogger logger, string wsId, string entityId);

    [LoggerMessage(EventId = 25, EventName = nameof(NoValidSetupStepFound), Level = LogLevel.Error,
        Message = "[{WSId}] No valid setup step found. Current step: {SetupStep}.")]
    public static partial void NoValidSetupStepFound(this ILogger logger, string wsId, in SetupStep setupStep);

    private static readonly Action<ILogger, string, Exception> ErrorDuringSetupProcessAction = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(26, nameof(EntityCommandHandlingException)),
        "[{WSId}] Error during setup process.");

    public static void ErrorDuringSetupProcess(this ILogger logger, string wsId, Exception exception) =>
        ErrorDuringSetupProcessAction(logger, wsId, exception);

    [LoggerMessage(EventId = 30, EventName = nameof(ResettingEventProcessing), Level = LogLevel.Information,
        Message = "[{WSId}] WS: Resetting event processing due to task status {TaskStatus}.")]
    public static partial void ResettingEventProcessing(this ILogger logger, string wsId, TaskStatus taskStatus);

    private static readonly Action<ILogger, string, Exception> UnhandledExceptionDuringEventAction = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(31, nameof(UnhandledExceptionDuringEvent)),
        "[{WSId}] Unhandled exception during event.");

    public static void UnhandledExceptionDuringEvent(this ILogger logger, string wsId, Exception exception) =>
        UnhandledExceptionDuringEventAction(logger, wsId, exception);

    [LoggerMessage(EventId = 32, EventName = nameof(EventProcessorNotRegistered), Level = LogLevel.Information,
        Message = "[{WSId}] WS: Event processor not registered.")]
    public static partial void EventProcessorNotRegistered(this ILogger logger, string wsId);

    private static readonly Action<ILogger, string, Exception> UnhandledExceptionDuringStartEventAction = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(33, nameof(UnhandledExceptionDuringStartEvent)),
        "[{WSId}] Unhandled exception during start event.");

    public static void UnhandledExceptionDuringStartEvent(this ILogger logger, string wsId, Exception exception) =>
        UnhandledExceptionDuringStartEventAction(logger, wsId, exception);

    private static readonly Action<ILogger, string, Exception> UnhandledExceptionDuringStopEventAction = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(34, nameof(UnhandledExceptionDuringStopEvent)),
        "[{WSId}] Unhandled exception during stop event.");

    public static void UnhandledExceptionDuringStopEvent(this ILogger logger, string wsId, Exception exception) =>
        UnhandledExceptionDuringStopEventAction(logger, wsId, exception);

    [LoggerMessage(EventId = 35, EventName = nameof(StartEventProcessorSemaphoreTimeout), Level = LogLevel.Information,
        Message = "[{WSId}] WS: Failed to acquire semaphore lock for start event within the timeout.")]
    public static partial void StartEventProcessorSemaphoreTimeout(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 36, EventName = nameof(StopEventProcessorSemaphoreTimeout), Level = LogLevel.Information,
        Message = "[{WSId}] WS: Failed to acquire semaphore lock for stop event within the timeout.")]
    public static partial void StopEventProcessorSemaphoreTimeout(this ILogger logger, string wsId);

    [LoggerMessage(EventId = 36, EventName = nameof(EventProcessingAlreadyRunning), Level = LogLevel.Information,
        Message = "[{WSId}] WS: Events are already running.")]
    public static partial void EventProcessingAlreadyRunning(this ILogger logger, string wsId);
}