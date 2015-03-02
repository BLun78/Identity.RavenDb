using System;
using Blun.AspNet.Identity.RavenDb.Store;
using Microsoft.AspNet.Identity;
using Microsoft.Framework.Logging;
using Raven.Client;

namespace Blun.AspNet.Identity.RavenDb
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public sealed class UserStoreKeyInt :
                        UserStore<IdentityUser<int>, IdentityRole<int>, int>
    {
        public UserStoreKeyInt(ILogger logger, Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null)
            : base(logger, getSession, describer)
        {
        }

        public UserStoreKeyInt(ILogger logger, IAsyncDocumentSession session, IdentityErrorDescriber describer = null)
            : base(logger, session, describer)
        {
        }
    }
}