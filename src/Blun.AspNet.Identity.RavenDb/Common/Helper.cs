using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blun.AspNet.Identity.RavenDb.Store;
using Microsoft.AspNet.Identity;
using Raven.Client;

// ReSharper disable once CheckNamespace
namespace Blun.AspNet.Identity.RavenDb.Common
{
    internal static class Helper
    {
        internal const string ScourceRavenDbClient = @"Raven.Client.Lightweight";

        internal static string ToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                sb.Append(bytes[i].ToString("x2"));
            return sb.ToString();
        }

        internal static byte[] FromHex(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must be an even number of characters to convert to bytes.");

            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0, b = 0; i < hex.Length; i += 2, b++)
                bytes[b] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }

        internal static IList<T> ToIList<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ToList();
        }

        internal static string GetLoginId(UserLoginInfo login, string identityPartsSeparator = "/")
        {
            using (var sha = SHA512.Create())
            {
                byte[] clearBytes = Encoding.UTF8.GetBytes(login.LoginProvider + "|" + login.ProviderKey);
                byte[] hashBytes = sha.ComputeHash(clearBytes);
                return string.Format("IdentityUserLogins{0}{1}", identityPartsSeparator, ToHex(hashBytes));
            }
        }

        internal async static Task<TResult[]> LoadAsync<TResult, TKey>(this IAsyncDocumentSession session, IEnumerable<TKey> ids, CancellationToken token = default(CancellationToken))
            where TKey : IConvertible, IComparable, IEquatable<TKey>
        {
            token.ThrowIfCancellationRequested();
            if (ids == null) throw new ArgumentNullException("ids");
            
            if (typeof(TKey) == typeof(int) || typeof(TKey) == typeof(Int32))
            {
                var keys = new List<int>();
                keys.AddRange(ids.Select(s => Convert.ToInt32(s)));
                return await session.LoadAsync<TResult>(keys.Select(s => s as ValueType), token);
            }
            else if (typeof(TKey) == typeof(long) || typeof(TKey) == typeof(Int64))
            {
                var keys = new List<long>();
                keys.AddRange(ids.Select(s => Convert.ToInt64(s)));
                return await session.LoadAsync<TResult>(keys.Select(s => s as ValueType), token);
            }
            else if (typeof(TKey) == typeof(string) || typeof(TKey) == typeof(String))
            {
                var keys = new List<string>();
                keys.AddRange(ids.Select(s => s as string));
                return await session.LoadAsync<TResult>(keys, token);
            }
            else
            {
                throw new NotSupportedException("Only 'int','long' and 'string' are valid for RavenDB Key!",
                                           new TypeAccessException(typeof(TKey).FullName));
            }
        }
    }
}