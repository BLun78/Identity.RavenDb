using System;
using Microsoft.AspNet.Identity;

namespace Blun.AspNet.Identity.RavenDb
{
    public static class IdentityRavenDbBuilderExtensions
    {
        public static IdentityBuilder AddRavendbStores(this IdentityBuilder builder)
        {
            foreach (var item in IdentityRavenDbServices.GetStringBasedKeyServices(null))
            {
                builder.Services.Add(item);
            }

            return builder;
        }

        public static IdentityBuilder AddRavendbStores<TKey>(this IdentityBuilder builder)
        {
            foreach (var item in IdentityRavenDbServices.GetDefaultServices(builder.UserType, builder.RoleType))
            {
                builder.Services.Add(item);
            }
            
            return builder;
        }
    }
}