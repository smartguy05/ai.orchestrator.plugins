# Ai.Orchestrator.Plugins.PythonRunner

## Overview

**Ai.Orchestrator.Plugins.PythonRunner** is a plugin for [AI Orchestrator](https://github.com/smartguy05/ai.orchestrator.plugins) designed to execute Python scripts and return their output. This enables AI Orchestrator to interact with Python code, automate script execution, and integrate Python-based solutions seamlessly.

## Features

- Run any Python script from AI Orchestrator workflows.
- Capture and return the standard output of the executed script.
- Easily configurable via a dedicated JSON configuration file.

## Installation

To use the PythonRunner plugin with AI Orchestrator, follow these steps:

1. **Compile the Plugin DLL**

   Build the project to produce the `Ai.Orchestrator.Plugins.PythonRunner.dll` file.

2. **Place the DLL in the Plugins Directory**

   Move the compiled DLL to the `Plugins` directory of your AI Orchestrator installation:

   ```
   /path/to/Ai.Orchestrator/Plugins/Ai.Orchestrator.Plugins.PythonRunner.dll
   ```

3. **Configuration File Setup**

   Copy the configuration file `Ai.Orchestrator.Plugins.PythonRunner.json` to the `Configs` directory:

   ```
   /path/to/Ai.Orchestrator/Configs/Ai.Orchestrator.Plugins.PythonRunner.json
   ```

   > **Note:** The configuration file typically specifies settings such as the path to the Python interpreter and environment variables. See the example below for details.

## Example Configuration

Below is a sample `Ai.Orchestrator.Plugins.PythonRunner.json`:

```json
{
  "PythonInterpreterPath": "C:\\Python311\\python.exe",
  "DefaultWorkingDirectory": "C:\\Scripts\\Python",
  "TimeoutSeconds": 60
}
```

- `PythonInterpreterPath`: Full path to the Python executable.
- `DefaultWorkingDirectory`: Directory where scripts are run by default.
- `TimeoutSeconds`: Max time allowed for script execution.

## Usage

Once installed and configured, you can use the PythonRunner plugin from within AI Orchestrator to execute Python scripts.

### Basic Usage Steps

1. **Ensure your Python script is accessible** from the working directory specified in the config.
2. **Trigger the plugin** from your AI Orchestrator workflow, specifying the script to run and any arguments.
3. **Receive the output**: The plugin will execute the script and return its standard output, errors, and exit code.

### Example Workflow Integration

Here is a conceptual example of invoking the plugin from an orchestrator workflow:

```json
{
  "Plugin": "Ai.Orchestrator.Plugins.PythonRunner",
  "Script": "my_script.py",
  "Arguments": ["--foo", "bar"]
}
```

## Requirements

- Windows or Linux OS
- .NET runtime compatible with AI Orchestrator
- Python installed and accessible at the specified path

## Troubleshooting

- **No output or errors**: Check that the Python path is correct in the config file.
- **Timeouts**: Increase `TimeoutSeconds` in the configuration if scripts take longer to run.
- **Script not found**: Ensure the script is in the working directory or specify the full path.

## License

This project is licensed under the MIT License.

## Contributing

If you wish to contribute improvements, please fork the repository and submit pull requests.

## Contact

For questions or support, open an issue in the repository.
