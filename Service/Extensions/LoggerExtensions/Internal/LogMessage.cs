using System;

namespace Service.Extensions.LoggerExtensions.Internal
{
    public class LogMessage
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }
    }
}
