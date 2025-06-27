using Ai.Orchestrator.Models.Enums;
using Ai.Orchestrator.Models.Interfaces;

namespace Ai.Orchestrator.Logging.FileLogger.Models;

public class LoggingConfig: ILoggingConfig
{
    public string Name { get; set; }
    public string Description { get; set; }
    public LogLevel MinimumLogLevel { get; set; } 
    public string LoggingFolder { get; set; }
}