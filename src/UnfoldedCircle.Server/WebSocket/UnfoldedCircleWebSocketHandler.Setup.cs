using Microsoft.Extensions.Logging;

using UnfoldedCircle.Models.Events;
using UnfoldedCircle.Models.Sync;
using UnfoldedCircle.Server.Configuration;
using UnfoldedCircle.Server.Response;

namespace UnfoldedCircle.Server.WebSocket;

public abstract partial class UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>
{
    /// <summary>
    /// Called when a <c>setup_driver</c> request is received.
    /// </summary>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Returns the found entity with its connection state, or null if not found.</returns>
    protected virtual async ValueTask<OnSetupResult?> OnSetupDriverAsync(SetupDriverMsg payload, string wsId, CancellationToken cancellationToken)
    {
        if (payload.MsgData.Reconfigure is true)
        {
            SessionHolder.NextSetupSteps[wsId] = SetupStep.ReconfigureEntity;
            var configuration = await _configurationService.GetConfigurationAsync(cancellationToken);
            return new OnSetupResult(SetupDriverResult.UserInputRequired, new RequireUserAction { Input = CreateReconfigurePage(wsId, configuration) });
        }

        return new OnSetupResult(SetupDriverResult.UserInputRequired, new RequireUserAction { Input = CreateNewEntitySettingsPageCore(wsId) });
    }

    /// <summary>
    /// Record representing the result of a lookup of configuration item during setup.
    /// </summary>
    /// <param name="SetupDriverResult">Result of the current setup step.</param>
    /// <param name="NextSetupStep">Information about the next setup step. Must be sent if <paramref name="SetupDriverResult"/> is set to <see cref="SetupDriverResult.UserInputRequired"/>.</param>
    // ReSharper disable once ClassNeverInstantiated.Global
    protected sealed record OnSetupResult(in SetupDriverResult SetupDriverResult, RequireUserAction? NextSetupStep = null);

    /// <summary>
    /// Setup driver result.
    /// </summary>
    protected enum SetupDriverResult
    {
        /// <summary>
        /// Setup finished successfully.
        /// </summary>
        Finalized,

        /// <summary>
        /// User input is required to continue the setup process.
        /// </summary>
        UserInputRequired,

        /// <summary>
        /// Error occurred during setup.
        /// </summary>
        Error
    }

    /// <summary>
    /// Called when a <c>set_driver_user_data</c> request is received with confirm data.
    /// </summary>
    /// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> to send events on.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask OnSetupDriverUserDataConfirmAsync(System.Net.WebSockets.WebSocket socket, SetDriverUserDataMsg payload, string wsId, CancellationToken cancellationToken);

    private const string ActionKey = "action";
    private const string DeviceKey = "device";
    private const string ActionAdd = "add";
    private const string ActionConfigure = "configure";
    private const string ActionDelete = "delete";
    private const string ActionReset = "reset";

    /// <summary>
    /// Called when a <c>set_driver_user_data</c> request is received during reconfiguration step.
    /// </summary>
    /// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> to send events on.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected virtual async ValueTask HandleReconfigureSetup(System.Net.WebSockets.WebSocket socket, SetDriverUserDataMsg payload, string wsId, CancellationToken cancellationToken)
    {
        await SendMessageAsync(socket, ResponsePayloadHelpers.CreateCommonResponsePayload(payload), wsId, cancellationToken);
        var action = payload.MsgData.InputValues![ActionKey];
        var entityId = payload.MsgData.InputValues[DeviceKey];
        SessionHolder.ReconfigureEntityMap[wsId] = entityId;
        UnfoldedCircleConfiguration<TConfigurationItem> configuration;
        TConfigurationItem entity;
        switch (action)
        {
            case ActionAdd:
                await SendMessageAsync(socket, ResponsePayloadHelpers.CreateDeviceSetupChangeUserInputResponsePayload(CreateNewEntitySettingsPageCore(wsId)), wsId,
                    cancellationToken);
                return;
            case ActionConfigure:
                configuration = await _configurationService.GetConfigurationAsync(cancellationToken);
                entity = configuration.Entities.Single(x => x.EntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase));
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateDeviceSetupChangeUserInputResponsePayload(CreateReconfigureEntitySettingsPage(entity)),
                    wsId,
                    cancellationToken);
                SessionHolder.NextSetupSteps[wsId] = SetupStep.SaveReconfiguredEntity;
                return;
            case ActionDelete:
                configuration = await _configurationService.GetConfigurationAsync(cancellationToken);
                entity = configuration.Entities.Single(x => x.EntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase));
                configuration.Entities.Remove(entity);
                await _configurationService.UpdateConfigurationAsync(configuration, cancellationToken);
                await FinishSetupAsync(socket, wsId, true, payload, cancellationToken);
                return;
            case ActionReset:
                configuration = await _configurationService.GetConfigurationAsync(cancellationToken);
                configuration.Entities.Clear();
                await _configurationService.UpdateConfigurationAsync(configuration, cancellationToken);
                await FinishSetupAsync(socket, wsId, true, payload, cancellationToken);
                return;
        }
    }

    /// <summary>
    /// Creates a reconfigure settings page for the <paramref name="configuration"/>.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="configuration">The configuration that should be reconfigured.</param>
    protected virtual SettingsPage CreateReconfigurePage(string wsId, UnfoldedCircleConfiguration<TConfigurationItem> configuration)
    {
        // No prior entities configured, go to new entity page.
        if (configuration.Entities.Count == 0)
        {
            return CreateNewEntitySettingsPageCore(wsId);
        }

        var settingsPage = new SettingsPage
        {
            Title = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Choose action" },
            Settings =
            [
                new Setting
                {
                    Id = DeviceKey,
                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Configured devices" },
                    Field = new SettingTypeDropdown
                    {
                        Dropdown = new SettingTypeDropdownInner
                        {
                            Items = configuration.Entities.Select(static x => new SettingTypeDropdownItem
                            {
                                Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = $"{x.EntityName} ({x.Host})" },
                                Value = x.EntityId
                            }).ToArray(),
                            Value = configuration.Entities[0].EntityId
                        }
                    }
                },
                new Setting
                {
                    Id = ActionKey,
                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Action" },
                    Field = new SettingTypeDropdown
                    {
                        Dropdown = new SettingTypeDropdownInner
                        {
                            Items =
                            [
                                new SettingTypeDropdownItem
                                {
                                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Add a new device" }, Value = ActionAdd
                                },
                                new SettingTypeDropdownItem
                                {
                                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Configure selected device" }, Value = ActionConfigure
                                },
                                new SettingTypeDropdownItem
                                {
                                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Delete selected device" }, Value = ActionDelete
                                },
                                new SettingTypeDropdownItem
                                {
                                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Reset and reconfigure" }, Value = ActionReset
                                },
                            ],
                            Value = ActionConfigure
                        }
                    }
                }
            ]
        };
        return settingsPage;
    }

    private SettingsPage CreateNewEntitySettingsPageCore(string wsId)
    {
        SessionHolder.NextSetupSteps[wsId] = SetupStep.NewEntity;
        return CreateNewEntitySettingsPage();
    }

    /// <summary>
    /// Creates a new entity settings page for adding a new entity.
    /// </summary>
    protected abstract SettingsPage CreateNewEntitySettingsPage();

    /// <summary>
    /// Creates a settings page for reconfiguring an existing entity.
    /// </summary>
    /// <param name="configurationItem">The configuration tied to the entity.</param>
    protected abstract SettingsPage CreateReconfigureEntitySettingsPage(TConfigurationItem configurationItem);

    private async Task FinishSetupAsync(System.Net.WebSockets.WebSocket socket,
        string wsId,
        bool isSuccess,
        CommonReq payload,
        CancellationToken cancellationToken)
    {
        SessionHolder.ReconfigureEntityMap.TryRemove(wsId, out _);
        await Task.WhenAll(
            SendMessageAsync(socket,
                ResponsePayloadHelpers.CreateCommonResponsePayload(payload),
                wsId,
                cancellationToken),
            SendMessageAsync(socket,
                ResponsePayloadHelpers.CreateDeviceSetupChangeResponsePayload(isSuccess),
                wsId,
                cancellationToken)
        );
    }

    /// <summary>
    /// Called when a <c>set_driver_user_data</c> request is received during save reconfigured entity step.
    /// </summary>
    /// <param name="socket">The socket for the current session.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="configurationItem">The configuration tied to the entity.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask<bool> HandleEntityReconfigured(System.Net.WebSockets.WebSocket socket,
        SetDriverUserDataMsg payload,
        string wsId,
        TConfigurationItem configurationItem,
        CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>set_driver_user_data</c> request is received during new entity step.
    /// </summary>
    /// <param name="socket">The socket for the current session.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <remarks>Implementer must save the new configuration in this step.</remarks>
    protected abstract ValueTask HandleCreateNewEntity(System.Net.WebSockets.WebSocket socket, SetDriverUserDataMsg payload, string wsId,
        CancellationToken cancellationToken);

    private async Task HandleSetupDriverUserData(System.Net.WebSockets.WebSocket socket, string wsId, SetDriverUserDataMsg payload, CancellationTokenWrapper cancellationTokenWrapper)
    {
        if (!SessionHolder.NextSetupSteps.TryGetValue(wsId, out var step))
        {
            _logger.LogError("[{WSId}] No setup step found.", wsId);
            await SendMessageAsync(socket,
                ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                    new ValidationError
                    {
                        Code = "404",
                        Message = "Could not find setup step. Please start the setup process again."
                    }),
                wsId,
                cancellationTokenWrapper.RequestAborted);
            return;
        }

        if (payload.MsgData is { Confirm: null, InputValues: null })
        {
            _logger.LogError("[{WSId}] No confirm or input_values found in payload.", wsId);
            await SendMessageAsync(socket,
                ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                    new ValidationError
                    {
                        Code = "400",
                        Message = "confirm or input_values is required for this step."
                    }),
                wsId,
                cancellationTokenWrapper.RequestAborted);
            return;
        }

        await SendMessageAsync(socket,
            ResponsePayloadHelpers.CreateDeviceSetupChangeResponseSetupPayload(),
            wsId,
            cancellationTokenWrapper.RequestAborted);

        if (payload.MsgData is { Confirm: not null })
        {
            await OnSetupDriverUserDataConfirmAsync(socket, payload, wsId, cancellationTokenWrapper.RequestAborted);
            return;
        }

        switch (step)
        {
            case SetupStep.NewEntity:
                await SendMessageAsync(socket, ResponsePayloadHelpers.CreateCommonResponsePayload(payload), wsId, cancellationTokenWrapper.RequestAborted);
                await HandleCreateNewEntity(socket, payload, wsId, cancellationTokenWrapper.RequestAborted);
                await SendMessageAsync(socket, ResponsePayloadHelpers.CreateDeviceSetupChangeResponseSetupPayload(), wsId, cancellationTokenWrapper.RequestAborted);
                return;
            case SetupStep.ReconfigureEntity:
                await HandleReconfigureSetup(socket, payload, wsId, cancellationTokenWrapper.RequestAborted);
                return;
            case SetupStep.SaveReconfiguredEntity:
                if (!SessionHolder.ReconfigureEntityMap.TryGetValue(wsId, out var entityId) || string.IsNullOrEmpty(entityId))
                {
                    _logger.LogError("[{WSId}] No entity ID found during save reconfigured entity step.", wsId);
                    await SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                            new ValidationError
                            {
                                Code = "404",
                                Message = "Could not find entity to reconfigure. Please start the setup process again."
                            }),
                        wsId,
                        cancellationTokenWrapper.RequestAborted);
                    return;
                }
                var configuration = await _configurationService.GetConfigurationAsync(cancellationTokenWrapper.RequestAborted);
                var configurationItem = configuration.Entities.SingleOrDefault(x => x.EntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase));
                if (configurationItem is null)
                {
                    _logger.LogError("[{WSId}] WS: Could not find entity with ID: {EntityId}.", wsId, entityId);
                    await SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                            new ValidationError
                            {
                                Code = "404",
                                Message = "Could not find setup step. Please start the setup process again."
                            }),
                        wsId,
                        cancellationTokenWrapper.RequestAborted);
                    return;
                }
                await SendMessageAsync(socket, ResponsePayloadHelpers.CreateCommonResponsePayload(payload), wsId, cancellationTokenWrapper.RequestAborted);
                if (await HandleEntityReconfigured(socket, payload, wsId, configurationItem, cancellationTokenWrapper.RequestAborted))
                {
                    SessionHolder.NextSetupSteps.TryRemove(wsId, out _);
                    SessionHolder.ReconfigureEntityMap.TryRemove(wsId, out _);
                }
                return;
            default:
                _logger.LogError("[{WSId}] No valid setup step found. Current step: {Step}.", wsId, step.ToString());
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                        new ValidationError
                        {
                            Code = "400",
                            Message = "Invalid setup step. Please start the setup process again."
                        }),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
                return;
        }
    }
}

internal enum SetupStep
{
    /// <summary>
    /// Next step is to configure a new entity.
    /// </summary>
    NewEntity,

    /// <summary>
    /// Next step is to reconfigure an existing entity.
    /// </summary>
    ReconfigureEntity,

    /// <summary>
    /// Next step is to save the reconfigured configured entity.
    /// </summary>
    SaveReconfiguredEntity
}