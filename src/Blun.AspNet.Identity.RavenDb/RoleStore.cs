using System;
using Blun.AspNet.Identity.RavenDb.Store;
using Microsoft.AspNet.Identity;
using Raven.Client;

// ReSharper disable once CheckNamespace
namespace Blun.AspNet.Identity.RavenDb
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public sealed class RoleStore :
                        RoleStore<IdentityRole, string>
    {
        public RoleStore(Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null) : base(getSession, describer)
        {
        }

        public RoleStore(IAsyncDocumentSession session, IdentityErrorDescriber describer = null) : base(session, describer)
        {
        }
    }
}