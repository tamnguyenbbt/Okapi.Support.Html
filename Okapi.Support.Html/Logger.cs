using System;
using Okapi.Common;
using Okapi.Logs;
using Serilog;
using SeriLogLogger = Serilog.Core.Logger;

namespace Okapi.Support.Html
{
    public class Logger : IOkapiLogger
    {
        private static SeriLogLogger logger;

        public Logger()
        {
            string logFileName = Session.Instance.LogPath;
            logger = new LoggerConfiguration().WriteTo.File(logFileName).CreateLogger();
        }

        public void Error(string messageTemplate)
        {
            logger.Error(messageTemplate);
        }

        public void Error(string messageTemplate, Exception exception)
        {
            logger.Error(exception, messageTemplate);
        }

        public void Info(string messageTemplate)
        {
            logger.Information(messageTemplate);
        }
    }
}