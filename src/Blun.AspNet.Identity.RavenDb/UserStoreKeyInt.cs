using System;
using Blun.AspNet.Identity.RavenDb.Store;
using Microsoft.AspNet.Identity;
using Raven.Client;

namespace Blun.AspNet.Identity.RavenDB
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public sealed class UserStoreKeyInt :
                        UserStore<IdentityUser<int>, IdentityRole<int>, int>
    {
        public UserStoreKeyInt(Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null)
            : base(getSession, describer)
        {
        }

        public UserStoreKeyInt(IAsyncDocumentSession session, IdentityErrorDescriber describer = null)
            : base(session, describer)
        {
        }
    }
}