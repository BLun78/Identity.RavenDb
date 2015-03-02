using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Microsoft.Framework.Logging;

namespace Blun.AspNet.Identity.RavenDb.Logging
{
    public static class LoggerStructure
    {
        public static LoggerStructureFormat CtorCreate(params object[] values)
        {
            return new LoggerStructureFormat("Create Ctor: {0}.{1}", values);
        }

        public static LoggerStructureFormat CtorFinish(params object[] values)
        {
            return new LoggerStructureFormat("Finisch Ctor: {0}.{1}", values);
        }
    }
}