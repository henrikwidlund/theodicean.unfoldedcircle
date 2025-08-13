# Theodicean.UnfoldedCircle

[![Release](https://img.shields.io/github/actions/workflow/status/henrikwidlund/theodicean.unfoldedcircle/github-release.yml?label=Release&logo=github)](https://github.com/henrikwidlund/theodicean.sourcegenerators/actions/workflows/github-release.yml)
[![CI](https://img.shields.io/github/actions/workflow/status/henrikwidlund/theodicean.unfoldedcircle/ci.yml?label=CI&logo=github)](https://github.com/henrikwidlund/theodicean.unfoldedcircle/actions/workflows/ci.yml)
![Sonar Quality Gate](https://img.shields.io/sonar/quality_gate/henrikwidlund_theodicean.unfoldedcircle?server=https%3A%2F%2Fsonarcloud.io&label=Sonar%20Quality%20Gate&logo=sonarqube)
[![Qodana](https://img.shields.io/github/actions/workflow/status/henrikwidlund/theodicean.unfoldedcircle/qodana_code_quality.yml?branch=main&label=Qodana&logo=github)](https://github.com/henrikwidlund/theodicean.unfoldedcircle/actions/workflows/qodana_code_quality.yml)
[![Version](https://img.shields.io/nuget/v/Theodicean.UnfoldedCircle.svg)](https://www.nuget.org/packages/Theodicean.UnfoldedCircle)

ASP.NET SDK for hosting integration drivers for the [Unfolded Circle Remotes](https://www.unfoldedcircle.com).

## Features
- mDNS broadcasting for discovery on remotes
- Configuration handling
- Event broadcasting
- Multiple entities in the same instance
- Media Player and Remote entity support
- Strongly typed models
- NativeAOT and trimming friendly
- Can be used for installation on the remote or on a server

## Limitations
- Only supports Media Player and Remote entities (I do not have any other devices to test with)

## Requirements
- dotnet 9 SDK

## Install

- **Package (project using the generator):**
    - Add a PackageReference to `Theodicean.UnfoldedCircle`
    - Example `csproj` snippet:

      ```xml
      <ItemGroup>
        <PackageReference Include="Theodicean.UnfoldedCircle" Version="x.y.z" />
      </ItemGroup>
      ```
## Usage

Start by adding a `driver.json` file in the root of your server project.
See [here](https://github.com/unfoldedcircle/core-api/blob/main/doc/integration-driver/driver-installation.md#metadata-file) for documentation on the file format.
You must make sure that the file is copied as part of the publishing process.

### Note
If you're publishing with NativeAOT, you must add the following to your server's `.csproj` file to ensure the `driver.json` is included in the output:

```xml
<PropertyGroup>
    <IncludeAllContentForSelfExtract>true</IncludeAllContentForSelfExtract>
    <ExcludeFromSingleFile>appsettings.json</ExcludeFromSingleFile>
    <ExcludeFromSingleFile>driver.json</ExcludeFromSingleFile>
</PropertyGroup>
```

The integration requires that you implement a few abstract classes and register them in your `Program.cs`:

- `UnfoldedCircle.Server.Configuration.ConfigurationService<TConfigurationItem>` - Handles configuration for the integration.
- `UnfoldedCircle.Server.WebSocket.UnfoldedCircleWebSocketHandler<TMediaPlayerCommandId, TConfigurationItem>` - Handles requests and events from and to the remotes.

Implement the abstract methods to work with your entities and configuration. You can use [unfoldedcircle-oppo](https://github.com/henrikwidlund/unfoldedcircle-oppo) as a reference implementation.

In your `Program.cs`, you can register the services like this:

```csharp
builder.AddUnfoldedCircleServer<CustomWebSocketHandler, CustomConfigurationService, CustomConfigurationItem>();

...
app.UseUnfoldedCircleServer<CustomWebSocketHandler, CustomConfigurationItem>();

// Or if you want to use custom media player command ids (where CustomCommandId is an enum and must have the members defined in `MediaPlayerCommandId`):
builder.AddUnfoldedCircleServer<CustomWebSocketHandler, CustomCommandId, CustomConfigurationService, CustomConfigurationItem>();

...
app.UseUnfoldedCircleServer<CustomWebSocketHandler, CustomCommandId, CustomConfigurationItem>();
```

## Useful links
- [Unfolded Circle Remotes](https://www.unfoldedcircle.com)
- [Unfolded Circle Integration Driver Documentation](https://github.com/unfoldedcircle/core-api/tree/main/doc/integration-driver)
- [Unfolded Circle Integration API Specification](https://github.com/unfoldedcircle/core-api/blob/main/integration-api/UCR-integration-asyncapi.yaml)
- [Unfolded Circle Entities Documentation](https://github.com/unfoldedcircle/core-api/tree/main/doc/entities)