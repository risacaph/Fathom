using Fathom.API.Services;

namespace Fathom.Server.Logging;

public class LoggingService: ILoggingService
{
    public void SwitchLogLevel(string level)
    {
        LogLevelOptions.SwitchLogLevel(level);
    }
}
