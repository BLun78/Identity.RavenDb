using System;
using System.Linq;
using Microsoft.AspNet.Identity;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Blun.AspNet.Identity.RavenDB.Index
{
    internal class IdentityUser_GetByUserName<TUser, TKey> : AbstractIndexCreationTask<TUser>
        where TUser : IdentityUser<TKey>
        where TKey : IConvertible, IComparable, IEquatable<TKey>
    {
        public class Result
        {
            public TKey Id { get; set; }
            public string UserName { get; set; }
            public string NormalizedUserName { get; set; }
        }

        public IdentityUser_GetByUserName()
        {
            Index(x => x.UserName, FieldIndexing.Default);
            Index(x => x.NormalizedUserName, FieldIndexing.Default);

            Map = users => users.Select(user => new Result()
            {
                Id = user.Id,
                UserName = user.UserName,
                NormalizedUserName = user.NormalizedUserName,
            });
        }
    }
}