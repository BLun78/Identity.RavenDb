using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Blun.AspNet.Identity.RavenDb.Logging;
using Microsoft.AspNet.Identity;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Internal;
using Microsoft.Framework.DependencyInjection;
using Raven.Client;
using Raven.Client.Document.Async;
using JetBrains.Annotations;

namespace Blun.AspNet.Identity.RavenDb.Store
{
    /// <summary>
    /// /the base for the <see cref="UserStore"/> and <see cref="Rolestore"/>
    /// </summary>
    /// <typeparam name="TKey">only <see cref="string"/> or <see cref="int"/></typeparam>
    [DebuggerDisplay("Session = {Session}")]
    public abstract class GenericStore<TKey> : IDisposable
        where TKey : IConvertible, IComparable, IEquatable<TKey>
    {
        #region Fields/Properties

        private readonly Func<IAsyncDocumentSession> _getSessionFunc;
        private IAsyncDocumentSession _session;
        private Func<string> _identityPartsSeparatorActor;

        public ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = NullLogger.Instance;
                }
                return _logger;
            }
            protected set
            {
                _logger = value;
            }
        }
        private ILogger _logger;

        /// <summary>
        /// Used to generate public API error messages 
        /// </summary>
        public IdentityErrorDescriber ErrorDescriber { get; set; }

        /// <summary>
        /// If it is 'true' it would automated save the RavenDb, otherwise the developer had to manuel to do
        /// </summary>
        public bool AutoSaveChanges
        {
            get
            {
                return _autoSaveChanges;
            }
            set
            {
                Logger.LogVerbose(LoggerId.GenericStoreAutoSaveChanges, "{0}.{1} is set to {2}", this.GetType().FullName, @"AutoSaveChanges", value);
                _autoSaveChanges = value;
            }
        }

        #endregion

        #region CTOR

        /// <summary>
        /// Use it if you have an open session
        /// </summary>
        /// <param name="describer">use to set an own <see cref="IdentityErrorDescriber"/></param>
        /// <param name="logger"></param>
        private GenericStore(ILogger logger, IdentityErrorDescriber describer)
        {
            Logger = logger;
            Logger.LogVerbose(0, LoggerStructure.CtorCreate(this.GetType().FullName));
            
            using (Logger.BeginScope("CTOR"))
            {

                //check für Valid Key
                if (!(CheckNumeric() || CheckString()))
                {
                    ThrowNotSupportedException(typeof(TKey));
                }
                //IDisposable
                HandleDisposable = Disposeable;
                //autosave
                AutoSaveChanges = true;
                //IdentityErrorDescriber
                ErrorDescriber = describer ?? IdentityErrorDescriber.Default;

                Logger.LogVerbose(2222, "");
            }
        }


        /// <summary>
        /// Use it for Lazy use for <see cref="AsyncDocumentSession"/>
        /// </summary>
        /// <param name="getSession">delegate for <see cref="AsyncDocumentSession"/></param>
        /// <param name="describer">use to set an own <see cref="IdentityErrorDescriber"/></param>
        protected GenericStore(ILogger logger, [NotNull] Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null)
            : this(logger, describer)
        {
            this.CheckArgumentForNull(getSession, "getSession");

            this._getSessionFunc = getSession;
        }

        /// <summary>
        /// Use it if the <see cref="AsyncDocumentSession"/> is now used
        /// </summary>
        /// <param name="session">set a <see cref="AsyncDocumentSession"/></param>
        /// <param name="describer">use to set an own <see cref="IdentityErrorDescriber"/></param>
        protected GenericStore(ILogger logger, [NotNull] IAsyncDocumentSession session, IdentityErrorDescriber describer = null)
            : this(logger, describer)
        {
            this.CheckArgumentForNull(session, "session");
            this._session = session;
        }

        #endregion

        #region Methods/Functions

        /// <summary>
        /// throws an NotSupportedException 
        /// </summary>
        /// <param name="type"></param>
        protected static void ThrowNotSupportedException(Type type)
        {
            throw new NotSupportedException("Only 'int','long' and 'string' are valid for RavenDB Key!",
                                            new TypeAccessException(type.FullName));
        }

        /// <summary>
        /// return true if 'TKey' is 'int' or 'long'
        /// </summary>
        protected static bool CheckNumeric()
        {
            return CheckInt() || CheckLong();
        }

        /// <summary>
        /// return true if 'TKey' is 'int'
        /// </summary>
        protected static bool CheckInt()
        {
            return typeof(TKey) == typeof(int) || typeof(TKey) == typeof(Int32);
        }

        /// <summary>
        /// return true if 'TKey' is 'long'
        /// </summary>
        protected static bool CheckLong()
        {
            return typeof(TKey) == typeof(long) || typeof(TKey) == typeof(Int64);
        }

        /// <summary>
        /// return true if 'TKey' is 'string'
        /// </summary>
        protected static bool CheckString()
        {
            return typeof(TKey) == typeof(string) || typeof(TKey) == typeof(String);
        }

        /// <summary>
        /// Check the argument only for null
        /// </summary>
        /// <param name="input"></param>
        /// <param name="argumentName"></param>
        /// <param name="sourceMemberName"></param>
        protected void CheckArgumentForOnlyNull(object input, string argumentName, [CallerMemberName]string sourceMemberName = "")
        {
            this.ThrowIfDisposed();
            if (input == null)
                throw new ArgumentNullException(argumentName, sourceMemberName);
        }

        /// <summary>
        /// Check the argument for null, empty, whitespace
        /// </summary>
        /// <param name="input"></param>
        /// <param name="argumentName"></param>
        /// <param name="sourceMemberName"></param>
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


        /// <summary>
        /// Default  void Task
        /// </summary>
        /// <returns></returns>
        protected Task<bool> VoidTask()
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Create the RavenDbId from Int to String
        /// </summary>
        /// <param name="id">value that represent the Id</param>
        /// <param name="type">TRavendb collection Type</param>
        /// <returns></returns>
        protected string CreateId(string id, Type type)
        {
            return this.CreateId(ConvertIdFromString(id), type);
        }

        /// <summary>
        /// Create the RavenDbId from Int to String
        /// </summary>
        /// <param name="id">value that represent the Id</param>
        /// <param name="type">TRavendb collection Type</param>
        /// <returns></returns>
        protected string CreateId(TKey id, Type type)
        {
            if (!CheckNumeric())
            {
                throw new NotSupportedException("Only 'int/long' are valid for GenericStore<TKey>.CreateId()", new TypeAccessException(typeof(TKey).FullName));
            }
            return Session.Advanced.DocumentStore.Conventions.FindFullDocumentKeyFromNonStringIdentifier(id, type, false);
        }

        /// <summary>
        /// Saves changes if <see cref="AutoSaveChanges"/> is 'true'
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        protected async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();
            if (AutoSaveChanges)
            {
                await Session.SaveChangesAsync(cancellationToken);
            }
            else
            {
                await Task.FromResult(0);
            }
        }

        /// <summary>
        /// Return the RavenDB IdentityPartsSeparator from client session
        /// </summary>
        protected Func<string> GetIdentityPartsSeparator
        {
            get
            {
                Func<string> standard = () => @"/";
                if (_identityPartsSeparatorActor == null)
                {
                    return standard;
                }
                return _identityPartsSeparatorActor;
            }
        }

        /// <summary>
        /// The RavenDb session
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        protected IAsyncDocumentSession Session
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_session.Advanced.DocumentStore.Conventions.IdentityPartsSeparator))
                {
                    this._identityPartsSeparatorActor = () => _session.Advanced.DocumentStore.Conventions.IdentityPartsSeparator;
                }
                if (_session == null && this._getSessionFunc != null)
                {
                    this._session = this._getSessionFunc();
                }
                else if (_session == null)
                {
                    throw new NullReferenceException("_session");
                }
                return _session;
            }
        }

        public virtual TKey ConvertIdFromString(string id)
        {
            if (id == null)
            {
                return default(TKey);
            }
            return (TKey)Convert.ChangeType(id, typeof(TKey));
        }

        public virtual string ConvertIdToString(TKey id)
        {
            if (id.Equals(default(TKey)))
            {
                return null;
            }
            if (CheckString())
            {
                return id as string;
            }
            return id.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        #region IDisposable

        protected virtual void Disposeable()
        {
            _session.Dispose();
        }

        protected Action HandleDisposable;
        protected bool Disposed = false;
        private bool _autoSaveChanges;

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

        ~GenericStore()
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