namespace UnfoldedCircle.Server.Event;

/// <summary>
/// Helpers for determining the type of a message event based on its JSON representation.
/// </summary>
public static class MessageEventHelpers
{
    private static readonly byte[] GetDriverVersion = "get_driver_version"u8.ToArray();
    private static readonly byte[] GetDriverMetaData = "get_driver_metadata"u8.ToArray();
    private static readonly byte[] Connect = "connect"u8.ToArray();
    private static readonly byte[] Disconnect = "disconnect"u8.ToArray();
    private static readonly byte[] GetDeviceState = "get_device_state"u8.ToArray();
    private static readonly byte[] GetAvailableEntities = "get_available_entities"u8.ToArray();
    private static readonly byte[] SubscribeEvents = "subscribe_events"u8.ToArray();
    private static readonly byte[] UnsubscribeEvents = "unsubscribe_events"u8.ToArray();
    private static readonly byte[] GetEntityStates = "get_entity_states"u8.ToArray();
    private static readonly byte[] SetupDriver = "setup_driver"u8.ToArray();
    private static readonly byte[] SetupDriverUserData = "set_driver_user_data"u8.ToArray();
    private static readonly byte[] AbortDriverSetup = "abort_driver_setup"u8.ToArray();
    private static readonly byte[] EntityCommand = "entity_command"u8.ToArray();
    private static readonly byte[] EnterStandby = "enter_standby"u8.ToArray();
    private static readonly byte[] ExitStandby = "exit_standby"u8.ToArray();
    private static readonly byte[] SupportedEntityTypes = "supported_entity_types"u8.ToArray();

    /// <summary>
    /// Gets the <see cref="MessageEvent"/> type from the provided JSON element.
    /// </summary>
    /// <param name="jsonElement">The <see cref="JsonDocument"/> to read from.</param>
    /// <param name="rawValue">Raw value provided if the event can't be matched to a known value.</param>
    public static MessageEvent GetMessageEvent(in JsonElement jsonElement, out string? rawValue)
    {
        rawValue = null;
        if (jsonElement.ValueEquals(GetDriverVersion))
            return MessageEvent.GetDriverVersion;

        if (jsonElement.ValueEquals(GetDriverMetaData))
            return MessageEvent.GetDriverMetaData;

        if (jsonElement.ValueEquals(Connect))
            return MessageEvent.Connect;
        
        if (jsonElement.ValueEquals(Disconnect))
            return MessageEvent.Disconnect;

        if (jsonElement.ValueEquals(GetDeviceState))
            return MessageEvent.GetDeviceState;

        if (jsonElement.ValueEquals(GetAvailableEntities))
            return MessageEvent.GetAvailableEntities;

        if (jsonElement.ValueEquals(SubscribeEvents))
            return MessageEvent.SubscribeEvents;
        
        if (jsonElement.ValueEquals(UnsubscribeEvents))
            return MessageEvent.UnsubscribeEvents;

        if (jsonElement.ValueEquals(GetEntityStates))
            return MessageEvent.GetEntityStates;

        if (jsonElement.ValueEquals(SetupDriver))
            return MessageEvent.SetupDriver;

        if (jsonElement.ValueEquals(SetupDriverUserData))
            return MessageEvent.SetupDriverUserData;

        if (jsonElement.ValueEquals(AbortDriverSetup))
            return MessageEvent.AbortDriverSetup;

        if (jsonElement.ValueEquals(EntityCommand))
            return MessageEvent.EntityCommand;

        if (jsonElement.ValueEquals(EnterStandby))
            return MessageEvent.EnterStandby;

        if (jsonElement.ValueEquals(ExitStandby))
            return MessageEvent.ExitStandby;

        if (jsonElement.ValueEquals(SupportedEntityTypes))
            return MessageEvent.SupportedEntityTypes;

        rawValue = jsonElement.GetString();
        return MessageEvent.Other;
    }
}

/// <summary>
/// Specifies the type of message events that can be received by the integration.
/// </summary>
public enum MessageEvent
{
    /// <summary>
    /// Other/Unknown message event type.
    /// </summary>
    Other,

    /// <summary>
    /// get_driver_version
    /// </summary>
    GetDriverVersion,

    /// <summary>
    /// get_driver_metadata
    /// </summary>
    GetDriverMetaData,

    /// <summary>
    /// connect
    /// </summary>
    Connect,

    /// <summary>
    /// disconnect
    /// </summary>
    Disconnect,

    /// <summary>
    /// get_device_state
    /// </summary>
    GetDeviceState,

    /// <summary>
    /// get_available_entities
    /// </summary>
    GetAvailableEntities,

    /// <summary>
    /// subscribe_events
    /// </summary>
    SubscribeEvents,

    /// <summary>
    /// unsubscribe_events
    /// </summary>
    UnsubscribeEvents,

    /// <summary>
    /// get_entity_states
    /// </summary>
    GetEntityStates,

    /// <summary>
    /// setup_driver
    /// </summary>
    SetupDriver,

    /// <summary>
    /// set_driver_user_data
    /// </summary>
    SetupDriverUserData,

    /// <summary>
    /// abort_driver_setup
    /// </summary>
    AbortDriverSetup,

    /// <summary>
    /// entity_command
    /// </summary>
    EntityCommand,

    /// <summary>
    /// enter_standby
    /// </summary>
    EnterStandby,

    /// <summary>
    /// exit_standby
    /// </summary>
    ExitStandby,

    /// <summary>
    /// supported_entity_types
    /// </summary>
    SupportedEntityTypes
}