﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNet.Identity;

// ReSharper disable once CheckNamespace
namespace Blun.AspNet.Identity.RavenDb.Common
{
    internal static class Helper
    {
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
                return string.Format("IdentityUserLogins{0}{1}", identityPartsSeparator, Helper.ToHex(hashBytes));
            }
        }
    }
}