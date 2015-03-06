using System;
using Blun.AspNet.Identity.RavenDb.Common;
using Microsoft.Framework.Logging;

namespace Blun.AspNet.Identity.RavenDb.Logging
{
    public class NullLogger : ILogger
    {
        public static NullLogger Instance = new NullLogger();

        public IDisposable BeginScope(object state)
        {
            return NullDisposable.Instance;
        }

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }
    }
}