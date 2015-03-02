using System;

namespace Blun.AspNet.Identity.RavenDb.Common
{
    public class NullDisposable : IDisposable
    {
        public static NullDisposable Instance = new NullDisposable();

        public void Dispose()
        {
            // intentionally does nothing
        }
    }
}