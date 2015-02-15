using System;
using Microsoft.AspNet.Identity;

namespace Blun.AspNet.Identity.RavenDb.Entity
{
    internal class IdentityUserLogin<TKey> : Microsoft.AspNet.Identity.IdentityUserLogin<TKey>
                    where TKey : IConvertible, IComparable, IEquatable<TKey>
    {
        public TKey Id { get; set; }    
    }
}