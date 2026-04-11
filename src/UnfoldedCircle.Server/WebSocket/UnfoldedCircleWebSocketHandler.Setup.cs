using UnfoldedCircle.Models.Events;
using UnfoldedCircle.Models.Shared;
using UnfoldedCircle.Models.Sync;
using UnfoldedCircle.Server.Configuration;
using UnfoldedCircle.Server.Response;

namespace UnfoldedCircle.Server.WebSocket;

public abstract partial class UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>
{
    private const string RestoreFailedValidationErrorCode = "RESTORE_FAILED";
    private const string RestoreFailedValidationErrorMessage = "Restore failed or invalid restore data. Please try again.";

    private static ValidationError CreateRestoreFailedValidationError() =>
        new()
        {
            Code = RestoreFailedValidationErrorCode,
            Message = RestoreFailedValidationErrorMessage
        };

    /// <summary>
    /// Called when a <c>setup_driver</c> request is received.
    /// </summary>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Returns the found entity with its connection state, or null if not found.</returns>
    protected virtual async ValueTask<OnSetupResult?> OnSetupDriverAsync(SetupDriverMsg payload, string wsId, CancellationToken cancellationToken)
    {
        var configuration = await _configurationService.GetConfigurationAsync(cancellationToken);

        if (configuration.Entities.Count == 0)
        {
            // Show initial restore step first if no entities
            SessionHolder.NextSetupSteps[wsId] = SetupStep.RestoreFromBackup;
            return new OnSetupResult(SetupDriverResult.UserInputRequired, new RequireUserAction { Input = CreateRestoreSettingsPage() });
        }

        if (payload.MsgData.Reconfigure is true)
        {
            SessionHolder.NextSetupSteps[wsId] = SetupStep.ReconfigureEntity;
            return new OnSetupResult(SetupDriverResult.UserInputRequired,
                new RequireUserAction
                {
                    Input = await CreateReconfigurePageAsync(wsId, configuration, cancellationToken)
                });
        }

        // Otherwise, go to new entity page
        return new OnSetupResult(SetupDriverResult.UserInputRequired, new RequireUserAction { Input = await CreateNewEntitySettingsPageCoreAsync(wsId, cancellationToken) });
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
    protected enum SetupDriverResult : sbyte
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
    /// Setup driver user data result.
    /// </summary>
    protected enum SetupDriverUserDataResult : sbyte
    {
        /// <summary>
        /// Setup finished successfully. Integration will send any necessary signals to the remote.
        /// </summary>
        Finalized,

        /// <summary>
        /// Method handled the request, no further action required.
        /// </summary>
        Handled,

        /// <summary>
        /// Error occurred during setup.
        /// </summary>
        Error
    }

    /// <summary>
    /// Result of a restore operation from backup.
    /// </summary>
    protected enum RestoreResult : sbyte
    {
        /// <summary>
        /// Operation succeeded
        /// </summary>
        Success,

        /// <summary>
        /// Operation failed
        /// </summary>
        Failure,

        /// <summary>
        /// Operation not applicable.
        /// </summary>
        NotApplicable
    }

    /// <summary>
    /// Called when a <c>set_driver_user_data</c> request is received with confirm data.
    /// </summary>
    /// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> to send events on.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask<SetupDriverUserDataResult> OnSetupDriverUserDataConfirmAsync(System.Net.WebSockets.WebSocket socket, SetDriverUserDataMsg payload, string wsId, CancellationToken cancellationToken);

    private const string ActionKey = "action";
    private const string EntityIdKey = "choice";
    private const string ActionAdd = "add";
    private const string ActionConfigure = "configure";
    private const string ActionDelete = "delete";
    private const string ActionReset = "reset";
    private const string ActionBackup = "backup";
    private const string ActionRestore = "restore";
    private const string RestoreFromBackup = "restore_from_backup";
    private const string RestoreData = "restore_data";

    /// <summary>
    /// Called when a <c>set_driver_user_data</c> request is received during reconfiguration step.
    /// </summary>
    /// <param name="socket">The <see cref="System.Net.WebSockets.WebSocket"/> to send events on.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected virtual async ValueTask<SetupDriverUserDataResult> HandleReconfigureSetup(System.Net.WebSockets.WebSocket socket, SetDriverUserDataMsg payload, string wsId, CancellationToken cancellationToken)
    {
        await SendMessageAsync(socket, ResponsePayloadHelpers.CreateCommonResponsePayload(payload), wsId, cancellationToken);

        var inputValues = payload.MsgData.InputValues;
        if (inputValues is null ||
            !inputValues.TryGetValue(ActionKey, out var action) ||
            string.IsNullOrWhiteSpace(action))
            return SetupDriverUserDataResult.Error;

        string? entityId = null;
        if (!action.Equals(ActionBackup, StringComparison.Ordinal) &&
            !action.Equals(ActionAdd, StringComparison.Ordinal) &&
            !action.Equals(ActionRestore, StringComparison.Ordinal) &&
            !action.Equals(ActionReset, StringComparison.Ordinal))
        {
            if (!inputValues.TryGetValue(EntityIdKey, out entityId) || string.IsNullOrWhiteSpace(entityId))
                return SetupDriverUserDataResult.Error;

            SessionHolder.ReconfigureEntityMap[wsId] = entityId;
        }

        UnfoldedCircleConfiguration<TConfigurationItem> configuration;
        TConfigurationItem entity;
        switch (action)
        {
            case ActionAdd:
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateDeviceSetupChangeUserInputResponsePayload(await CreateNewEntitySettingsPageCoreAsync(wsId, cancellationToken)),
                    wsId,
                    cancellationToken);
                return SetupDriverUserDataResult.Handled;
            case ActionConfigure:
                if (string.IsNullOrWhiteSpace(entityId))
                    return SetupDriverUserDataResult.Error;

                configuration = await _configurationService.GetConfigurationAsync(cancellationToken);
                entity = configuration.Entities.Single(x => x.EntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase));
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateDeviceSetupChangeUserInputResponsePayload(await CreateReconfigureEntitySettingsPageAsync(entity, cancellationToken)),
                    wsId,
                    cancellationToken);
                SessionHolder.NextSetupSteps[wsId] = SetupStep.SaveReconfiguredEntity;
                return SetupDriverUserDataResult.Handled;
            case ActionDelete:
                if (string.IsNullOrWhiteSpace(entityId))
                    return SetupDriverUserDataResult.Error;

                configuration = await _configurationService.GetConfigurationAsync(cancellationToken);
                entity = configuration.Entities.Single(x => x.EntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase));
                configuration.Entities.Remove(entity);
                await _configurationService.UpdateConfigurationAsync(configuration, cancellationToken);
                await Task.WhenAll(CreateEntityUnavailableSignals(socket, wsId, entity.EntityId, cancellationToken));
                return SetupDriverUserDataResult.Finalized;
            case ActionReset:
                configuration = await _configurationService.GetConfigurationAsync(cancellationToken);
                await Task.WhenAll(configuration.Entities.SelectMany(x => CreateEntityUnavailableSignals(socket, wsId, x.EntityId, cancellationToken)));
                configuration.Entities.Clear();
                await _configurationService.UpdateConfigurationAsync(configuration, cancellationToken);
                return SetupDriverUserDataResult.Finalized;
            case ActionBackup:
                var driverMetadata = await _configurationService.GetDriverMetadataAsync(cancellationToken);
                var jsonBackupData = await GetJsonBackupDataAsync(cancellationToken);
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateDeviceSetupChangeUserInputResponsePayload(CreateBackupSettingsPage(driverMetadata.Name, jsonBackupData)),
                    wsId,
                    cancellationToken);
                SessionHolder.NextSetupSteps[wsId] = SetupStep.BackupEntity;
                return SetupDriverUserDataResult.Handled;
            case ActionRestore:
                driverMetadata = await _configurationService.GetDriverMetadataAsync(cancellationToken);
                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateDeviceSetupChangeUserInputResponsePayload(CreateReconfigureRestoreSettingsPage(driverMetadata.Name)),
                    wsId,
                    cancellationToken);
                SessionHolder.NextSetupSteps[wsId] = SetupStep.RestoreFromBackupData;
                return SetupDriverUserDataResult.Handled;
            default:
                return SetupDriverUserDataResult.Error;
        }
    }

    private IEnumerable<Task> CreateEntityUnavailableSignals(System.Net.WebSockets.WebSocket socket, string wsId, string entityId, CancellationToken cancellationToken)
    {
        return GetEntities(entityId).Select(x =>
        {
            if (x.EntityType == EntityType.Sensor && SessionHolder.SensorTypesMap.TryGetValue(x.EntityId, out var sensorSuffixes))
            {
                return Task.WhenAll(sensorSuffixes.Select(suffix => SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateSensorStateChangedResponsePayload(
                        new SensorStateChangedEventMessageDataAttributes<string> { State = SensorState.Unavailable, Value = null }, x.EntityId,
                        suffix), wsId, cancellationToken)));
            }

            if (x.EntityType == EntityType.Select && SessionHolder.SelectTypesMap.TryGetValue(x.EntityId, out var selectSuffixes))
            {
                return Task.WhenAll(selectSuffixes.Select(suffix => SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateSelectStateChangedResponsePayload(
                        new SelectStateChangedEventMessageDataAttributes { State = SelectState.Unavailable }, x.EntityId,
                        suffix), wsId, cancellationToken)));
            }

            return x.EntityType switch
            {
                EntityType.MediaPlayer => SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateMediaPlayerStateChangedResponsePayload(new DeltaMediaPlayerStateChangedEventMessageDataAttributes { State = State.Unavailable }, x.EntityId),
                    wsId, cancellationToken),
                EntityType.Remote => SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateRemoteStateChangedResponsePayload(new RemoteStateChangedEventMessageDataAttributes { State = RemoteState.Unavailable }, x.EntityId),
                    wsId, cancellationToken),
                EntityType.Climate => SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateClimateStateChangedResponsePayload(new ClimateStateChangedEventMessageDataAttributes { State = ClimateState.Unavailable }, x.EntityId),
                    wsId, cancellationToken),
                _ => Task.CompletedTask
            };
        });
    }

    /// <summary>
    /// Registers a sensor with the given <paramref name="sensorSuffix"/> for the <paramref name="entityId"/>.
    /// </summary>
    /// <param name="entityId">The entity_id.</param>
    /// <param name="sensorSuffix">The suffix identifying the sensor.</param>
    protected static void RegisterSensor(string entityId, string sensorSuffix)
    {
        if (!SessionHolder.SensorTypesMap.TryGetValue(entityId, out var existingSuffixes))
        {
            existingSuffixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { sensorSuffix };
            SessionHolder.SensorTypesMap[entityId] = existingSuffixes;
        }
        else
            existingSuffixes.Add(sensorSuffix);
    }

    /// <summary>
    /// Registers a select with the given <paramref name="selectSuffix"/> for the <paramref name="entityId"/>.
    /// </summary>
    /// <param name="entityId">The entity_id.</param>
    /// <param name="selectSuffix">The suffix identifying the select.</param>
    protected static void RegisterSelect(string entityId, string selectSuffix)
    {
        if (!SessionHolder.SelectTypesMap.TryGetValue(entityId, out var existingSuffixes))
        {
            existingSuffixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { selectSuffix };
            SessionHolder.SelectTypesMap[entityId] = existingSuffixes;
        }
        else
            existingSuffixes.Add(selectSuffix);
    }

    /// <summary>
    /// Creates a reconfigure settings page for the <paramref name="configuration"/>.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="configuration">The configuration that should be reconfigured.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected virtual ValueTask<SettingsPage> CreateReconfigurePageAsync(string wsId, UnfoldedCircleConfiguration<TConfigurationItem> configuration, CancellationToken cancellationToken)
    {
        // No prior entities configured, go to new entity page.
        if (configuration.Entities.Count == 0)
            return CreateNewEntitySettingsPageCoreAsync(wsId, cancellationToken);

        var settingsPage = new SettingsPage
        {
            Title = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Choose Action" },
            Settings =
            [
                new Setting
                {
                    Id = EntityIdKey,
                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Configured Devices" },
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
                                new SettingTypeDropdownItem
                                {
                                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Backup configuration to clipboard" }, Value = ActionBackup
                                },
                                new SettingTypeDropdownItem
                                {
                                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Restore configuration from backup" }, Value = ActionRestore
                                }
                            ],
                            Value = ActionConfigure
                        }
                    }
                }
            ]
        };
        return ValueTask.FromResult(settingsPage);
    }

    private ValueTask<SettingsPage> CreateNewEntitySettingsPageCoreAsync(string wsId, CancellationToken cancellationToken)
    {
        SessionHolder.NextSetupSteps[wsId] = SetupStep.NewEntity;
        return CreateNewEntitySettingsPageAsync(cancellationToken);
    }

    private static Setting CreateRestoreFromBackupItem() =>
        new()
        {
            Id = RestoreFromBackup,
            Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Restore from backup" },
            Field = new SettingTypeCheckbox
            {
                Checkbox = new SettingTypeCheckboxInner
                {
                    Value = false
                }
            }
        };

    /// <summary>
    /// Creates a JSON backup of the current configuration to be sent to the user.
    /// This will be used when user selects backup option during reconfiguration.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask<string> GetJsonBackupDataAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new entity settings page for adding a new entity.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask<SettingsPage> CreateNewEntitySettingsPageAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a settings page for reconfiguring an existing entity.
    /// </summary>
    /// <param name="configurationItem">The configuration tied to the entity.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask<SettingsPage> CreateReconfigureEntitySettingsPageAsync(TConfigurationItem configurationItem, CancellationToken cancellationToken);

    private static SettingsPage CreateBackupSettingsPage(Dictionary<string, string> driverName, string jsonBackupData) =>
        new()
        {
            Title = driverName,
            Settings =
            [
                new Setting
                {
                    Id = "info",
                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Configuration Backup" },
                    Field = new SettingTypeLabel
                    {
                        Label = new SettingTypeLabelItem
                        {
                            Value = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["en"] = "Copy the configuration data below and save it in a safe place. " +
                                         "You can use this to restore your configuration after an integration update."
                            }
                        }
                    }
                },
                new Setting
                {
                    Id = "backup_data",
                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Backup Contents" },
                    Field = new SettingTypeTextArea { TextArea = new SettingTypeTextAreaInner { Value = jsonBackupData } }
                }
            ]
        };

    private static SettingsPage CreateRestoreSettingsPage() =>
        new()
        {
            Title = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Restore" },
            Settings =
            [
                new Setting
                {
                    Id = "info",
                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Integration Upgrade" },
                    Field = new SettingTypeLabel
                    {
                        Label = new SettingTypeLabelItem
                        {
                            Value = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["en"] = "Are you upgrading this integration?" +
                                         " If you have a configuration backup, you can restore it now." +
                                         " Otherwise, continue with the setup process to add a new device." +
                                         " Once configured, you can create a backup from the integration settings screen by running the Setup again."
                            }
                        }
                    }
                },
                CreateRestoreFromBackupItem(),
            ]
        };

    private static SettingsPage CreateReconfigureRestoreSettingsPage(Dictionary<string, string> driverName) =>
        new()
        {
            Title = driverName,
            Settings =
            [
                new Setting
                {
                    Id = "info",
                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Restore Configuration" },
                    Field = new SettingTypeLabel
                    {
                        Label = new SettingTypeLabelItem
                        {
                            Value = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["en"] = "Paste the configuration backup data below to restore your devices."
                            }
                        }
                    }
                },
                new Setting
                {
                    Id = RestoreData,
                    Label = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Configuration Backup Data" },
                    Field = new SettingTypeTextArea { TextArea = new SettingTypeTextAreaInner() }
                }
            ]
        };

    private async Task FinishSetupAsync(System.Net.WebSockets.WebSocket socket,
        string wsId,
        bool isSuccess,
        CommonReq payload,
        CancellationToken cancellationToken)
    {
        SessionHolder.ReconfigureEntityMap.TryRemove(wsId, out _);
        SessionHolder.NextSetupSteps.TryRemove(wsId, out _);
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
    protected abstract ValueTask<SetupDriverUserDataResult> HandleEntityReconfigured(System.Net.WebSockets.WebSocket socket,
        SetDriverUserDataMsg payload,
        string wsId,
        TConfigurationItem configurationItem,
        CancellationToken cancellationToken);

    /// <summary>
    /// Handle restore operation from a JSON backup.
    /// It is up to the implementor to decode the backup data, restore the configuration and save necessary data.
    /// </summary>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="jsonRestoreData">JSON backup data provided by the user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected abstract ValueTask<RestoreResult> HandleRestoreFromBackupAsync(string wsId, string jsonRestoreData, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a <c>set_driver_user_data</c> request is received during new entity step.
    /// </summary>
    /// <param name="socket">The socket for the current session.</param>
    /// <param name="payload">Payload of the request.</param>
    /// <param name="wsId">ID of the websocket.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <remarks>Implementer must save the new configuration in this step.</remarks>
    protected abstract ValueTask<SetupDriverUserDataResult> HandleCreateNewEntity(System.Net.WebSockets.WebSocket socket, SetDriverUserDataMsg payload, string wsId,
        CancellationToken cancellationToken);

    private async Task HandleSetupDriverUserData(System.Net.WebSockets.WebSocket socket, string wsId, SetDriverUserDataMsg payload, CancellationTokenWrapper cancellationTokenWrapper)
    {
        try
        {
            if (!SessionHolder.NextSetupSteps.TryGetValue(wsId, out var step))
            {
                _logger.NoSetupStepFound(wsId);

                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                        new ValidationError
                        {
                            Code = "SETUP_STEP_NOT_FOUND",
                            Message = "Could not find setup step. Please start the setup process again."
                        }),
                    wsId,
                    cancellationTokenWrapper.RequestAborted);
                return;
            }

            if (payload.MsgData is { Confirm: null, InputValues: null })
            {
                _logger.NoConfirmOrInputValuesFound(wsId);

                await SendMessageAsync(socket,
                    ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                        new ValidationError
                        {
                            Code = "INVALID_ARGUMENT",
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
                var confirmationResult = await OnSetupDriverUserDataConfirmAsync(socket, payload, wsId, cancellationTokenWrapper.RequestAborted);
                if (confirmationResult != SetupDriverUserDataResult.Handled)
                {
                    SessionHolder.NextSetupSteps.TryRemove(wsId, out _);
                    await FinishSetupAsync(socket, wsId, confirmationResult == SetupDriverUserDataResult.Finalized, payload, cancellationTokenWrapper.RequestAborted);
                }
                return;
            }

            switch (step)
            {
                case SetupStep.NewEntity:
                    switch (await HandleCreateNewEntity(socket, payload, wsId, cancellationTokenWrapper.RequestAborted))
                    {
                        case SetupDriverUserDataResult.Finalized:
                            await FinishSetupAsync(socket, wsId, isSuccess: true, payload, cancellationTokenWrapper.RequestAborted);
                            break;
                        case SetupDriverUserDataResult.Error:
                            await FinishSetupAsync(socket, wsId, isSuccess: false, payload, cancellationTokenWrapper.RequestAborted);
                            break;
                    }

                    return;
                case SetupStep.ReconfigureEntity:
                    var setupDriverUserDataResult = await HandleReconfigureSetup(socket, payload, wsId, cancellationTokenWrapper.RequestAborted);
                    if (setupDriverUserDataResult != SetupDriverUserDataResult.Handled)
                        await FinishSetupAsync(socket, wsId, setupDriverUserDataResult == SetupDriverUserDataResult.Finalized, payload, cancellationTokenWrapper.RequestAborted);
                    return;
                case SetupStep.SaveReconfiguredEntity:
                    if (!SessionHolder.ReconfigureEntityMap.TryGetValue(wsId, out var entityId) || string.IsNullOrEmpty(entityId))
                    {
                        _logger.NoEntityIdFoundSaveReconfigure(wsId);

                        await SendMessageAsync(socket,
                            ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                                new ValidationError
                                {
                                    Code = "ENTITY_NOT_FOUND",
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
                        _logger.EntityWithIdNotFound(wsId, entityId);

                        await SendMessageAsync(socket,
                            ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                                new ValidationError
                                {
                                    Code = "SETUP_STEP_NOT_FOUND",
                                    Message = "Could not find setup step. Please start the setup process again."
                                }),
                            wsId,
                            cancellationTokenWrapper.RequestAborted);
                        return;
                    }
                    var driverUserDataResult = await HandleEntityReconfigured(socket, payload, wsId, configurationItem, cancellationTokenWrapper.RequestAborted);
                    if (driverUserDataResult != SetupDriverUserDataResult.Handled)
                    {
                        SessionHolder.NextSetupSteps.TryRemove(wsId, out _);
                        SessionHolder.ReconfigureEntityMap.TryRemove(wsId, out _);
                        await FinishSetupAsync(socket, wsId, driverUserDataResult == SetupDriverUserDataResult.Finalized, payload, cancellationTokenWrapper.RequestAborted);
                    }
                    return;
                case SetupStep.RestoreFromBackup:
                    if (payload.MsgData.InputValues is not null &&
                        payload.MsgData.InputValues.TryGetValue(RestoreFromBackup, out var restoreFlag) &&
                        string.Equals(restoreFlag, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        var driverMetaData = await _configurationService.GetDriverMetadataAsync(cancellationTokenWrapper.RequestAborted);
                        SessionHolder.NextSetupSteps[wsId] = SetupStep.RestoreFromBackupData;
                        await SendMessageAsync(socket,
                            ResponsePayloadHelpers.CreateDeviceSetupChangeUserInputResponsePayload(CreateReconfigureRestoreSettingsPage(driverMetaData.Name)),
                            wsId,
                            cancellationTokenWrapper.RequestAborted);
                        return;
                    }
                    // If restore was not chosen, return to action/choice page
                    SessionHolder.NextSetupSteps[wsId] = SetupStep.ReconfigureEntity;
                    var currentConfig = await _configurationService.GetConfigurationAsync(cancellationTokenWrapper.RequestAborted);
                    await SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateDeviceSetupChangeUserInputResponsePayload(await CreateReconfigurePageAsync(wsId, currentConfig, cancellationTokenWrapper.RequestAborted)),
                        wsId,
                        cancellationTokenWrapper.RequestAborted);
                    return;

                case SetupStep.RestoreFromBackupData:
                    if (payload.MsgData.InputValues is not null &&
                        payload.MsgData.InputValues.TryGetValue(RestoreData, out var restoreData) &&
                        !string.IsNullOrWhiteSpace(restoreData))
                    {
                        var restoreResult = await HandleRestoreFromBackupAsync(wsId, restoreData, cancellationTokenWrapper.RequestAborted);
                        if (restoreResult == RestoreResult.Success)
                        {
                            SessionHolder.NextSetupSteps.TryRemove(wsId, out _);
                            await FinishSetupAsync(socket, wsId, isSuccess: true, payload, cancellationTokenWrapper.RequestAborted);
                            return;
                        }
                    }
                    // Always show error if missing/invalid restore data
                    await SendMessageAsync(socket,
                        ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                            CreateRestoreFailedValidationError()),
                        wsId,
                        cancellationTokenWrapper.RequestAborted);
                    return;
                case SetupStep.BackupEntity:
                    // If the user submits from the backup page, finalize the setup flow
                    SessionHolder.NextSetupSteps.TryRemove(wsId, out _);
                    await FinishSetupAsync(socket, wsId, isSuccess: true, payload, cancellationTokenWrapper.RequestAborted);
                    return;
                default:
                    _logger.NoValidSetupStepFound(wsId, step);

                    await SendValidationErrorResponse(socket, wsId, payload, cancellationTokenWrapper);
                    return;
            }
        }
        catch (Exception e)
        {
            _logger.ErrorDuringSetupProcess(wsId, e);

            await SendValidationErrorResponse(socket, wsId, payload, cancellationTokenWrapper);
        }
    }

    private Task SendValidationErrorResponse(System.Net.WebSockets.WebSocket socket, string wsId, SetDriverUserDataMsg payload, CancellationTokenWrapper cancellationTokenWrapper)
    {
        return SendMessageAsync(socket,
            ResponsePayloadHelpers.CreateValidationErrorResponsePayload(payload,
                new ValidationError
                {
                    Code = "INVALID_SETUP_STEP",
                    Message = "Invalid setup step. Please start the setup process again."
                }),
            wsId,
            cancellationTokenWrapper.RequestAborted);
    }

    private static SettingsPage CreateReconfigureRestoreFromBackupFlagPage()
    {
        return new SettingsPage
        {
            Title = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = "Restore Configuration" },
            Settings = [ CreateRestoreFromBackupItem() ]
        };
    }
}

internal enum SetupStep : sbyte
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
    /// Next step is to save the reconfigured entity.
    /// </summary>
    SaveReconfiguredEntity,

    /// <summary>
    /// Next step is to show the backup page.
    /// </summary>
    BackupEntity,

    /// <summary>
    /// Next step is to restore from backup in either the initial setup flow or the reconfigure flow.
    /// </summary>
    RestoreFromBackup,

    /// <summary>
    /// Next step is to input restore data after choosing restore in either the initial setup flow or the reconfigure flow.
    /// </summary>
    RestoreFromBackupData
}