using System;
using System.Runtime.CompilerServices;

namespace Blun.AspNet.Identity.RavenDb.Common
{
    public abstract class GenericBase<TKey> : IDisposable
    where TKey : IConvertible, IComparable, IEquatable<TKey>
    {

        protected GenericBase()
        {
            //check für Valid Key
            if (!(CheckNumeric() || CheckString()))
            {
                ThrowTypeAccessException(typeof(TKey));
            }
        }

        protected static void ThrowTypeAccessException(Type type)
        {
            throw new NotSupportedException("Only 'int','long' and 'string' are valid for RavenDB Key!",
                                            new TypeAccessException(type.FullName));
        }

        //TODO: Refactor Name
        protected static bool CheckNumeric()
        {
            return CheckInt() || CheckLong();
        }

        protected static bool CheckInt()
        {
            return typeof (TKey) == typeof (int) || typeof (TKey) == typeof (Int32);
        }

        protected static bool CheckLong()
        {
            return typeof(TKey) == typeof(long) || typeof(TKey) == typeof(Int64);
        }

        protected static bool CheckString()
        {
            return typeof(TKey) == typeof(string) || typeof(TKey) == typeof(String);
        }

        protected void CheckArgumentForOnlyNull(object input, string argumentName, [CallerMemberName]string sourceMemberName = "")
        {
            this.ThrowIfDisposed();
            if (input == null)
                throw new ArgumentNullException(argumentName, sourceMemberName);
        }

        protected void CheckArgumentForNull(object input, string argumentName, [CallerMemberName]string sourceMemberName = "")
        {
            this.ThrowIfDisposed();
            if (input is string)
            {
                if (string.IsNullOrWhiteSpace(input as string))
                    throw new ArgumentException("ValueCannotBeNullOrEmptyOrWhiteSpace - " + argumentName, sourceMemberName);
            }
            else if (input is int)
            {
                if (Convert.ToInt32(input) == default(int))
                    throw new ArgumentNullException(argumentName, sourceMemberName);
            }
            else if (input is long)
            {
                if (Convert.ToInt64(input) == default(long))
                    throw new ArgumentNullException(argumentName, sourceMemberName);
            }
            else
            {
                if (input == null)
                    throw new ArgumentNullException(argumentName, sourceMemberName);
            }
        }

        #region IDisposable

        protected Action HandleDisposable;
        protected bool Disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    try
                    {
                        HandleDisposable?.Invoke();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~GenericBase()
        {
            Dispose(false);
        }

        protected void ThrowIfDisposed()
        {
            if (this.Disposed)
                throw new ObjectDisposedException(this.GetType().Name);
        }

        #endregion
    }
}