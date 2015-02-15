using System;
using System.Collections.Generic;
using Blun.AspNet.Identity.RavenDb.Store;
using Microsoft.AspNet.Identity;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Raven.Client;

namespace Blun.AspNet.Identity.RavenDb
{
    //http://blogs.msdn.com/b/webdev/archive/2014/06/17/dependency-injection-in-asp-net-vnext.aspx
    internal static class IdentityRavenDbServices
    {
        public static IEnumerable<IServiceDescriptor> GetDefaultServices(Type userType = null,
                                                                           Type roleType = null,
                                                                           Type keyType = null,
                                                                           IConfiguration config = null)
        {
            Type userStoreType;
            Type roleStoreType;

            var describe = config == null ?
                                    new ServiceDescriber() :
                                    new ServiceDescriber(config);

            if (userType == null && (keyType == null || keyType == typeof(string)))
            {
                userType = typeof(IdentityUser);
            }
            else if (userType == null && keyType != null || keyType == typeof(int) || keyType == typeof(long))
            {
                userType = typeof(IdentityUser<>).MakeGenericType(keyType);
            }

            if (roleType == null && (keyType == null || keyType == typeof(string)))
            {
                roleType = typeof(IdentityRole);
            }
            else if (roleType == null && keyType != null || keyType == typeof(int) || keyType == typeof(long))
            {
                roleType = typeof(IdentityRole<>).MakeGenericType(keyType);
            }

            if (keyType != null)
            {
                //Generic
                userStoreType = typeof(UserStore<,,>).MakeGenericType(userType, roleType, keyType);
                roleStoreType = typeof(RoleStore<,>).MakeGenericType(roleType, keyType);
            }
            else
            {
                //string default
                userStoreType = typeof(UserStore);
                roleStoreType = typeof(RoleStore);
            }

            yield return describe.Scoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
            yield return describe.Scoped(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
        }

        public static IEnumerable<IServiceDescriptor> GetStringBasedKeyServices(IConfiguration config = null)
        {
            return GetDefaultServices(null, null, null, config);
        }
    }
}