using System;
using Blun.AspNet.Identity.RavenDb.Store;
using Microsoft.AspNet.Identity;
using Raven.Client;

namespace Blun.AspNet.Identity.RavenDb
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public sealed class UserStoreKeyLong :
                        UserStore<IdentityUser<long>, IdentityRole<long>, long>
    {
        public UserStoreKeyLong(Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null)
            : base(getSession, describer)
        {
        }

        public UserStoreKeyLong(IAsyncDocumentSession session, IdentityErrorDescriber describer = null)
            : base(session, describer)
        {
        }
    }
}