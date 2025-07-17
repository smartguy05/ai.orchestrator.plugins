# Ai.Orchestrator.Plugins
Plugins created to be used with Ai Orchestrator

---

## 🚧 Future Work

Planned enhancements include:
- **Telnyx Integration**: A plugin for SMS, voice, and communications via Telnyx.
- Additional external integrations and automation plugins.
- Improved configuration and extensibility for plugin management.

---

## Available Plugins

This repository currently includes plugins for integrating external services and enhancing AI Orchestrator workflows. The available plugins are:

- **Ai.Orchestrator.Plugins.UseMemos**  
  Memo-style storage and retrieval for textual or structured data within orchestrator flows. [Details](./Ai.Orchestrator.Plugins.UseMemos/README.md)

- **Ai.Orchestrator.Plugins.GoogleCalendar**  
  Integrates Google Calendar for creating, updating, and retrieving events via orchestrator logic. [Details](./Ai.Orchestrator.Plugins.GoogleCalendar/README.md)

- **Ai.Orchestrator.Plugins.SupportChannelKb**  
  (See plugin directory for support channel knowledge base integration.)

- **Ai.Orchestrator.Plugins.WebSearch**  
  Enables web search via Kagi or Google Custom Search API. [Details](./Ai.Orchestrator.Plugins.WebSearch/README.md)

- **Ai.Orchestrator.Plugins.PythonRunner**  
  Run Python scripts or automation tasks from orchestrator workflows.

- **Ai.Orchestrator.Plugins.HomeAssistantAssist**  
  Integrate with Home Assistant for smart home automation.

- **Ai.Orchestrator.Plugins.Telegram**  
  Send and receive Telegram messages as part of your orchestration logic.

- **Ai.Orchestrator.Logging.FileLogger**  
  File-based logging plugin for AI Orchestrator events and flows.

> For plugin-specific usage and configuration instructions, see each plugin’s directory and README file.

---

# ICommand

# Config files

# CsProj changes
1. The .csproj file needs to be updated to allow for dynamic loading to do so edit the .csproj file of the project and add the following:

```
<PropertyGroup>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <AppendRuntimeIdentifierToOutputPath>false</AppendRuntimeIdentifierToOutputPath>
</PropertyGroup>
```

2. Update the property group with the dotnet version and add EnableDynamicLoading, like below:
```
<PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <EnableDynamicLoading>true</EnableDynamicLoading>
</PropertyGroup>
```

3. Update references. Add Private = false and ExcludeAssets = runtime to Ai.Orchestrator.Models reference. 

*NOTE: Ai.Orchestrator.Common does not work for this yet*
```
<ItemGroup>
  <Reference Include="Ai.Orchestrator.Common">
    <HintPath>..\..\ai.orchestrator\Ai.Orchestrator\bin\Debug\net7.0\Plugins\Ai.Orchestrator.Common.dll</HintPath>
  </Reference>
  <Reference Include="Ai.Orchestrator.Models">
    <HintPath>..\..\ai.orchestrator\Ai.Orchestrator\bin\Debug\net7.0\Ai.Orchestrator.Models.dll</HintPath>
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
  </Reference>
</ItemGroup>
```

---

## Getting Started

Clone the repository and add the desired plugin(s) to your AI Orchestrator solution:

```bash
git clone https://github.com/smartguy05/ai.orchestrator.plugins.git
cd ai.orchestrator.plugins
```

Each plugin’s folder contains its own README with installation, configuration, and usage instructions.

---

## Contributing

We welcome contributions! For new plugins, improvements, or bug reports, open an issue or pull request. See plugin documentation for guidelines.

---

## License

MIT License. See [LICENSE](./LICENSE).

---

_Note: Some search results are limited. For more plugin details, see the [GitHub repository](https://github.com/smartguy05/ai.orchestrator.plugins)._ 
