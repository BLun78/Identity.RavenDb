using System;
using Blun.AspNet.Identity.RavenDb.Store;
using Microsoft.AspNet.Identity;
using Raven.Client;

namespace Blun.AspNet.Identity.RavenDB
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public sealed class UserStore :
                        UserStore<IdentityUser, IdentityRole, string>
    {
        public UserStore(Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null)
            : base(getSession, describer)
        {
        }

        public UserStore(IAsyncDocumentSession session, IdentityErrorDescriber describer = null)
            : base(session, describer)
        {
        }
    }
}
