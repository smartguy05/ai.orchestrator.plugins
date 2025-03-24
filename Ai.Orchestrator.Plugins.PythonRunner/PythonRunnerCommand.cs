using System.Diagnostics;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.PythonRunner.Models;

namespace Ai.Orchestrator.Plugins.PythonRunner;

public class PythonRunnerCommand: CommandBase<ServiceRequest, ServiceConfig>
{
    public override string Name => "PythonRunner";
    public override string Description => "A plugin to run a python script";

    public override async Task<object> DoWork(ServiceRequest serviceRequest, ServiceConfig config, IEnumerable<ToolCall> availableToolCalls)
    {
        var tempFile = Path.GetTempFileName() + ".py";

        try
        {
            // Write the Python script to the temporary file
            await File.WriteAllTextAsync(tempFile, serviceRequest.PythonScript);
            return await RunPythonScript(tempFile);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new { Success = false };
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
    
    private async Task<object> RunPythonScript(string tempFile)
    {
        // Setup the process start info to run the Python interpreter.
        // Use Python's subprocess to validate script syntax
        var start = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{tempFile}\"",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        string output = null;
        try
        {
            using var process = Process.Start(start);

            if (process is null)
            {
                throw new Exception("Could not start python script");
            }
            
            // Read the standard output and error.
            output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
                
            await process.WaitForExitAsync();

            // Output the results.
            Console.WriteLine("Output:");
            Console.WriteLine(output);

            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.WriteLine("Error:");
                Console.WriteLine(error);

                return new { Success = false, Error = error };
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while running the Python script:");
            Console.WriteLine(ex.Message);
            return new { Success = false, Error = ex.Message };
        }
        
        return new { Success = true, Result = output };
    }
}