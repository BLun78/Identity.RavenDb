using System;
using Blun.AspNet.Identity.RavenDb.Store;
using Microsoft.AspNet.Identity;
using Microsoft.Framework.Logging;
using Raven.Client;

namespace Blun.AspNet.Identity.RavenDb
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public sealed class RoleStoreKeyInt :
                        RoleStore<IdentityRole<int>, int>
    {
        public RoleStoreKeyInt(ILogger logger, Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null) : base(logger, getSession, describer)
        {
        }

        public RoleStoreKeyInt(ILogger logger, IAsyncDocumentSession session, IdentityErrorDescriber describer = null) : base(logger, session, describer)
        {
        }
    }
}