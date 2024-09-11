using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Targets;

namespace Mislint.Core
{
    public partial class Logger
    {
        public static Logger Instance { get; } = new Logger();
        public readonly ILoggerFactory loggerFactory;
        public Logger()
        {
            this.loggerFactory =
                LoggerFactory.Create(builder =>
                {
                    builder.AddDebug();
                    var config = new NLog.Config.LoggingConfiguration();
                    var logFile = new FileTarget("logfile")
                    {
                        FileName = "mislint.log",
                        Layout = "${longdate}|${level:uppercase=true}|${logger}|${threadid}|${message}|${exception:format=tostring}\n"
                    };
                    config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logFile);
                    builder.AddNLog();
                }
            );
        }
    }
}
