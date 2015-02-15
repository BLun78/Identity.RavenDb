using System;

namespace Blun.AspNet.Identity.RavenDb.Entity
{
    public class IdentityUserRole<TKey> : Microsoft.AspNet.Identity.IdentityUserRole<TKey>
         where TKey : IConvertible, IComparable, IEquatable<TKey>
    {
        public TKey Id { get; set; }
    }
}