using System;
using Microsoft.AspNet.Identity;

namespace Blun.AspNet.Identity.RavenDb
{
    public static class IdentityRavenDbBuilderExtensions
    {
        public static IdentityBuilder AddRavendbStores(this IdentityBuilder builder)
        {
            builder.Services.Add(IdentityRavenDbServices.GetDefaultServices(builder.UserType, builder.RoleType, typeof(TContext)));
            return builder;
        }

        public static IdentityBuilder AddRavendbStores<TKey>(this IdentityBuilder builder)
        {
            builder.Services.Add(IdentityRavenDbServices.GetDefaultServices(builder.UserType, builder.RoleType, typeof(TContext)));
            return builder;
        }
    }
}