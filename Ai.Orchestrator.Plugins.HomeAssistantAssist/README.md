# Ai.Orchestrator.Plugins.HomeAssistantAssist

## Overview

**Ai.Orchestrator.Plugins.HomeAssistantAssist** is a plugin for [AI Orchestrator](https://github.com/smartguy05/ai.orchestrator.plugins) that enables seamless integration with the [Home Assistant Voice API](https://www.home-assistant.io/docs/voice_control/). This plugin allows AI Orchestrator to communicate with your Home Assistant server to control smart home devices using voice commands.

With this plugin, you can automate and manage your smart home environment directly through AI Orchestrator, leveraging the power of Home Assistant's extensive device ecosystem.

## Features

- Send voice commands to Home Assistant via its Voice API.
- Control lights, switches, thermostats, and other smart devices.
- Receive feedback and status updates from Home Assistant.
- Simple configuration and deployment within the AI Orchestrator framework.

## Installation

### Prerequisites

- [AI Orchestrator](https://github.com/smartguy05/ai.orchestrator.plugins) installed and set up.
- A running instance of [Home Assistant](https://www.home-assistant.io/).

### Steps

1. **Compile the Plugin**

   - Build the `Ai.Orchestrator.Plugins.HomeAssistantAssist` project to produce the DLL file (e.g., `Ai.Orchestrator.Plugins.HomeAssistantAssist.dll`).

2. **Copy the DLL**

   - Place the compiled plugin DLL into the `Plugins` directory of your AI Orchestrator installation.

3. **Configure the Plugin**

   - Create or obtain the configuration file named `Ai.Orchestrator.Plugins.HomeAssistantAssist.json`.
   - Place this configuration file in the `Configs` directory of your AI Orchestrator installation.

   Example directory structure:
   ```
   Ai.Orchestrator/
   ├── Plugins/
   │   └── Ai.Orchestrator.Plugins.HomeAssistantAssist.dll
   ├── Configs/
   │   └── Ai.Orchestrator.Plugins.HomeAssistantAssist.json
   └── ...
   ```

## Configuration

The configuration file (`Ai.Orchestrator.Plugins.HomeAssistantAssist.json`) must contain the necessary settings to connect to your Home Assistant instance, such as the API URL and authentication token.

Example configuration:
```json
{
  "HomeAssistantUrl": "http://localhost:8123/api/voice_assist",
  "AccessToken": "YOUR_LONG_LIVED_ACCESS_TOKEN"
}
```

- `HomeAssistantUrl`: The URL to the Home Assistant Voice API endpoint.
- `AccessToken`: Your Home Assistant long-lived access token.

> **Note:** Never share your access token publicly.

## Usage

Once installed and configured, the plugin will automatically load when you start AI Orchestrator. You can begin issuing voice commands through AI Orchestrator, which will be relayed to your Home Assistant instance for execution.

**Example commands:**
- "Turn on the living room lights."
- "Set the thermostat to 72 degrees."
- "Lock the front door."

The plugin handles sending these commands to Home Assistant and relaying responses back to AI Orchestrator.

## Troubleshooting

- **Plugin not loading:** Ensure the DLL is in the correct `Plugins` directory and named properly.
- **Configuration errors:** Double-check the JSON configuration file for correct syntax and valid credentials.
- **Connection issues:** Ensure your Home Assistant instance is reachable from the machine running AI Orchestrator.

## Contributing

Pull requests and suggestions are welcome! Please see the [main project repo](https://github.com/smartguy05/ai.orchestrator.plugins) for guidelines.

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Credits

- [Home Assistant](https://www.home-assistant.io/)
- [AI Orchestrator](https://github.com/smartguy05/ai.orchestrator.plugins)
