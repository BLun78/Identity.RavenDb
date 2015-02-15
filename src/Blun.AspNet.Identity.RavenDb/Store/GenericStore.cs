using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Blun.AspNet.Identity.RavenDb.Common;
using Microsoft.AspNet.Identity;
using Raven.Client;
using Raven.Client.Document.Async;

namespace Blun.AspNet.Identity.RavenDb.Store
{
    /// <summary>
    /// /the base for the <see cref="UserStore"/> and <see cref="Rolestore"/>
    /// </summary>
    /// <typeparam name="TKey">only <see cref="string"/> or <see cref="int"/></typeparam>
    public abstract class GenericStore<TKey> : GenericBase<TKey>
        where TKey : IConvertible, IComparable, IEquatable<TKey>
    {
        #region Fields/Properties

        private readonly Func<IAsyncDocumentSession> _getSessionFunc;
        private IAsyncDocumentSession _session;
        private Func<string> _identityPartsSeparatorActor;

        /// <summary>
        /// Used to generate public API error messages 
        /// </summary>
        public IdentityErrorDescriber ErrorDescriber { get; set; }

        /// <summary>
        /// If it is 'true' it would automated save the RavenDb, otherwise the developer had to manuel to do
        /// </summary>
        public bool AutoSaveChanges { get; set; }

        #endregion

        #region CTOR

        /// <summary>
        /// Better nev
        /// </summary>
        /// <param name="describer">use to set an own <see cref="IdentityErrorDescriber"/></param>
        private GenericStore(IdentityErrorDescriber describer)
            : base()
        {
            //IDisposable
            HandleDisposable = Disposeable;
            //autosave
            AutoSaveChanges = true;
            //IdentityErrorDescriber
            ErrorDescriber = describer ?? IdentityErrorDescriber.Default;

            //_session.Advanced.DocumentStore.Conventions.RegisterIdConvention<TUser>((dbname, commands, user) => user.KeyPrefix + this.GetIdentityPartsSeparator() + user.Id);
        }


        /// <summary>
        /// Use it for Lazy use for <see cref="AsyncDocumentSession"/>
        /// </summary>
        /// <param name="getSession">delegate for <see cref="AsyncDocumentSession"/></param>
        /// <param name="describer">use to set an own <see cref="IdentityErrorDescriber"/></param>
        protected GenericStore(Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null)
            : this(describer)
        {
            this.CheckArgumentForNull(getSession, "getSession");

            this._getSessionFunc = getSession;
        }

        /// <summary>
        /// Use it if the <see cref="AsyncDocumentSession"/> is now used
        /// </summary>
        /// <param name="session">set a <see cref="AsyncDocumentSession"/></param>
        /// <param name="describer">use to set an own <see cref="IdentityErrorDescriber"/></param>
        protected GenericStore(IAsyncDocumentSession session, IdentityErrorDescriber describer = null)
            : this(describer)
        {
            this.CheckArgumentForNull(session, "session");
            this._session = session;
        }

        #endregion

        #region IDisposable
        
        protected virtual void Disposeable()
        {
            _session.Dispose();
        }

        #endregion

        #region Methods/Functions

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
            return CreateId(ConvertIdFromString(id), type);
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
            base.ThrowIfDisposed();
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
    }
}