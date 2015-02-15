using System;
using Blun.AspNet.Identity.RavenDb.Store;
using Microsoft.AspNet.Identity;
using Raven.Client;

namespace Blun.AspNet.Identity.RavenDb
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public sealed class RoleStoreKeyLong :
                        RoleStore<IdentityRole<long>, long>
    {
        public RoleStoreKeyLong(Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null) : base(getSession, describer)
        {
        }

        public RoleStoreKeyLong(IAsyncDocumentSession session, IdentityErrorDescriber describer = null) : base(session, describer)
        {
        }
    }
}