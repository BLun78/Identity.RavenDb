using System;
using Blun.AspNet.Identity.RavenDb.Store;
using Microsoft.AspNet.Identity;
using Microsoft.Framework.Logging;
using Raven.Client;

// ReSharper disable once CheckNamespace
namespace Blun.AspNet.Identity.RavenDb
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public sealed class RoleStore :
                        RoleStore<IdentityRole, string>
    {
        public RoleStore(ILogger logger, Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null) : base(logger, getSession, describer)
        {
        }

        public RoleStore(ILogger logger, IAsyncDocumentSession session, IdentityErrorDescriber describer = null) : base(logger, session, describer)
        {
        }
    }
}