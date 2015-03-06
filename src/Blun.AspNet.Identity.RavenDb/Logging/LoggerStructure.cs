using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Internal;

namespace Blun.AspNet.Identity.RavenDb.Logging
{
    public static class LoggerStructure
    {
        public static ILogValues CtorCreate(params object[] values)
        {
            return new FormattedLogValues("Create Ctor: {0}.{1}", values);
        }

        public static ILogValues CtorFinish(params object[] values)
        {
            return new FormattedLogValues("Finisch Ctor: {0}.{1}", values);
        }
    }
}