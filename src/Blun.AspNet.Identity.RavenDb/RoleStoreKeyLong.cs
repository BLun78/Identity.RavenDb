using System;
using Blun.AspNet.Identity.RavenDb.Store;
using Microsoft.AspNet.Identity;
using Microsoft.Framework.Logging;
using Raven.Client;

namespace Blun.AspNet.Identity.RavenDb
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public sealed class RoleStoreKeyLong :
                        RoleStore<IdentityRole<long>, long>
    {
        public RoleStoreKeyLong(ILogger logger, Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null) : base(logger, getSession, describer)
        {
        }

        public RoleStoreKeyLong(ILogger logger, IAsyncDocumentSession session, IdentityErrorDescriber describer = null) : base(logger, session, describer)
        {
        }
    }
}