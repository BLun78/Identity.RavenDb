using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Blun.AspNet.Identity.RavenDb.Common;
using Blun.AspNet.Identity.RavenDB.Index;
using Microsoft.AspNet.Identity;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using Raven.Client.Exceptions;
using Raven.Client.Linq;

namespace Blun.AspNet.Identity.RavenDb.Store
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public class UserStore<TUser, TRole, TKey> : 
                                        GenericStore<TKey>,
                                        IUserStore<TUser>,
                                        IUserLoginStore<TUser>,
                                        IUserRoleStore<TUser>,
                                        IUserClaimStore<TUser>,
                                        IUserPasswordStore<TUser>,
                                        IUserSecurityStampStore<TUser>,
                                        IUserEmailStore<TUser>,
                                        IUserLockoutStore<TUser>,
                                        IUserPhoneNumberStore<TUser>,
                                        IQueryableUserStore<TUser>,
                                        IUserTwoFactorStore<TUser>
                                            where TUser : IdentityUser<TKey>
                                            where TRole : IdentityRole<TKey>
                                            where TKey : IConvertible, IComparable, IEquatable<TKey>
    {
        #region CTOR

        protected UserStore(Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null)
            : base(getSession, describer)
        {
            IndexInstaller().Wait();
        }

        protected UserStore(IAsyncDocumentSession session, IdentityErrorDescriber describer = null)
            : base(session, describer)
        {
            IndexInstaller().Wait();
        }

        private async Task IndexInstaller()
        {
            var store = Session.Advanced.DocumentStore;

            await new IdentityRole_GetByName<TRole, TKey>().ExecuteAsync(store.AsyncDatabaseCommands, store.Conventions);
            await new IdentityUser_GetByEmail<TUser, TKey>().ExecuteAsync(store.AsyncDatabaseCommands, store.Conventions);
            await new IdentityUser_GetByUserName<TUser, TKey>().ExecuteAsync(store.AsyncDatabaseCommands, store.Conventions);
        }

        #endregion

        #region Property IRavenQueryable

        internal IRavenQueryable<TUser> IdentityUsers => base.Session.Query<TUser>();

        internal IRavenQueryable<TRole> IdentityRoles => base.Session.Query<TRole>();

        internal IRavenQueryable<Entity.IdentityUserLogin<TKey>> IdentityUserLogins => base.Session.Query<Entity.IdentityUserLogin<TKey>>();

        internal IRavenQueryable<IdentityUserClaim<TKey>> IdentityUserClaims => base.Session.Query<IdentityUserClaim<TKey>>();

        internal IRavenQueryable<IdentityUserRole<TKey>> IdentityUserRoles => base.Session.Query<IdentityUserRole<TKey>>();

        #endregion

        #region IUserStore

        public async virtual Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await Task.FromResult(base.ConvertIdToString(user.Id));
        }

        public async virtual Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await Task.FromResult(user.UserName);
        }

        public async virtual Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForOnlyNull(userName, nameof(userName));

            user.UserName = userName;
            await base.VoidTask();
        }

        public async virtual Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await Task.FromResult(user.NormalizedUserName);
        }

        public async virtual Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForOnlyNull(normalizedName, nameof(normalizedName));

            user.NormalizedUserName = normalizedName;
            await base.VoidTask();
        }

        public async virtual Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            await this.Session.StoreAsync(user, cancellationToken);

            await base.SaveChangesAsync(cancellationToken);

            return IdentityResult.Success;
        }

        public async virtual Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            try
            {
                await base.SaveChangesAsync(cancellationToken);
            }
            catch (InvalidOperationException ioe)
            {
                if (ioe.Source == Helper.ScourceRavenDbClient)
                {
                    return IdentityResult.Failed(base.ErrorDescriber.ConcurrencyFailure());
                }
                else
                {
                    throw;
                }
            }

            return IdentityResult.Success;
        }

        public async virtual Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            try
            {
                this.Session.Delete(user);
                await base.SaveChangesAsync(cancellationToken);
            }
            catch (InvalidOperationException ioe)
            {
                if (ioe.Source == Helper.ScourceRavenDbClient)
                {
                    return IdentityResult.Failed(base.ErrorDescriber.ConcurrencyFailure());
                }
                else
                {
                    throw;
                }
            }

            return IdentityResult.Success;
        }

        public async virtual Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(userId, nameof(userId));

            var id = string.Empty;

            if (CheckNumeric())
            {
                id = base.CreateId(userId, typeof(TUser));
            }
            else if (CheckString())
            {
                id = userId;
            }
            else
            {
                ThrowNotSupportedException(typeof(TKey));
            }

            return await this.Session.LoadAsync<TUser>(id, cancellationToken);
        }

        public async Task<TUser> FindByNameAsync(string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(userName, nameof(userName));

            return await base.Session.Query<TUser, IdentityUser_GetByUserName<TUser, TKey>>()
                                            .SingleOrDefaultAsync(x => x.UserName == userName, cancellationToken);
        }

        #endregion

        #region IQueryableUserStore

        public virtual IQueryable<TUser> Users
        {
            get
            {
                base.ThrowIfDisposed();
                return IdentityUsers.AsQueryable();
            }
        }

        #endregion

        #region IUserClaimStore

        public async virtual Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await IdentityUserClaims.Where(uc => uc.UserId.Equals(user.Id)).Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToListAsync(cancellationToken);
        }

        public async virtual Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForNull(claims, nameof(claims));

            foreach (var claim in claims)
            {
                var userClaim = new IdentityUserClaim<TKey>
                {
                    UserId = user.Id,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                };
                await base.Session.StoreAsync(userClaim, cancellationToken);

            }
            await base.VoidTask();
        }

        public async virtual Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForNull(claim, nameof(claim));
            base.CheckArgumentForNull(newClaim, nameof(newClaim));

            var id = user.Id;
            var matchedClaims = await IdentityUserClaims.Where(uc => uc.UserId.Equals(id)
                                                            && uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToListAsync(cancellationToken);
            foreach (var matchedClaim in matchedClaims)
            {
                matchedClaim.ClaimValue = newClaim.Value;
                matchedClaim.ClaimType = newClaim.Type;
            }
        }

        public async virtual Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForNull(claims, nameof(claims));

            var id = user.Id;
            foreach (var claim in claims)
            {
                var claim1 = claim;
                var matchedClaims = await IdentityUserClaims.Where(uc => uc.UserId.Equals(id) && uc.ClaimValue == claim1.Value && uc.ClaimType == claim1.Type).ToListAsync(cancellationToken);
                foreach (var c in matchedClaims)
                {
                    base.Session.Delete(c);
                }
            }
        }

        /// <summary>
        ///     Get all users with given claim
        /// </summary>
        /// <param name="claim"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async virtual Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(claim, nameof(claim));

            var userClaims = await this.IdentityUserClaims.Where(x => x.ClaimValue == claim.Value && x.ClaimType == claim.Type)
                                                         .Select(s => s.UserId)
                                                         .ToListAsync(cancellationToken);
            var userIdAsString = new List<string>();

            if (CheckNumeric())
            {
                if (userClaims != null)
                    userIdAsString.AddRange(userClaims.Select(userid => base.CreateId(userid, typeof(TUser))));
            }
            else if (CheckString())
            {
                if (userClaims != null)
                    userIdAsString.AddRange(userClaims.Select(userid => userid as string));
            }
            else
            {
                ThrowNotSupportedException(typeof(TKey));
            }

            return await base.Session.LoadAsync<TUser>(userIdAsString, cancellationToken);
        }

        #endregion

        #region IUserEmailStore

        public async virtual Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForOnlyNull(email, nameof(email));

            user.Email = email;

            await base.VoidTask();
        }

        public async virtual Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await Task.FromResult(user.Email);
        }

        public async virtual Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, "user");

            return await Task.FromResult(user.EmailConfirmed);
        }

        public async virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, "user");
            base.CheckArgumentForOnlyNull(confirmed, "confirmed");

            user.EmailConfirmed = confirmed;

            await base.VoidTask();
        }

        public async virtual Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForOnlyNull(normalizedEmail, nameof(normalizedEmail));

            return await Session.Query<TUser, IdentityUser_GetByEmail<TUser, TKey>>().SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
        }

        public async virtual Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await Task.FromResult(user.NormalizedEmail);
        }

        public async virtual Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForOnlyNull(normalizedEmail, nameof(normalizedEmail));

            user.NormalizedEmail = normalizedEmail;

            await base.VoidTask();
        }

        #endregion

        #region IUserLockoutStore

        public async virtual Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, "user");

            return await Task.FromResult(user.LockoutEnd);
        }

        public async virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, "user");

            user.LockoutEnd = lockoutEnd;

            await base.VoidTask();
        }

        public async virtual Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, "user");

            user.AccessFailedCount++;

            return await Task.FromResult(user.AccessFailedCount);
        }

        public async virtual Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, "user");

            user.AccessFailedCount = 0;

            await base.VoidTask();
        }

        public async virtual Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, "user");

            return await Task.FromResult(user.AccessFailedCount);
        }

        public async virtual Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, "user");

            return await Task.FromResult(user.LockoutEnabled);
        }

        public async virtual Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, "user");
            base.CheckArgumentForOnlyNull(enabled, "enabled");

            user.LockoutEnabled = enabled;

            await base.VoidTask();
        }

        #endregion

        #region IUserLoginStore

        public async virtual Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForNull(login, nameof(login));

            var identityLogin = new Entity.IdentityUserLogin<TKey>()
            {
                UserId = user.Id,
                ProviderKey = login.ProviderKey,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName
            };

            await base.Session.StoreAsync(identityLogin, cancellationToken);
        }

        public async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
                                           CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForOnlyNull(loginProvider, nameof(loginProvider));
            base.CheckArgumentForOnlyNull(providerKey, nameof(providerKey));

            var entry = await IdentityUserLogins.SingleOrDefaultAsync(l => l.UserId.Equals(user.Id)
                                                                    && l.LoginProvider == loginProvider
                                                                    && l.ProviderKey == providerKey
                                                                    , cancellationToken);
            if (entry != null)
            {
                base.Session.Delete(entry);
            }
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await IdentityUserLogins.Where(x => x.UserId.Equals(user.Id))
                                            .Select(s => new UserLoginInfo(s.LoginProvider, s.ProviderKey, s.ProviderDisplayName))
                                            .ToListAsync(cancellationToken);
        }

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(loginProvider, nameof(loginProvider));
            base.CheckArgumentForNull(providerKey, nameof(providerKey));

            var userLogins = await this.IdentityUserLogins.Where(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey)
                                                          .SingleOrDefaultAsync(cancellationToken);

            if (userLogins == null)
            {
                return null;
            }

            var id = string.Empty;
            if (CheckNumeric())
            {
                id = base.CreateId(userLogins.UserId, typeof(TUser));
            }
            else if (CheckString())
            {
                id = userLogins.UserId as string;
            }
            else
            {
                ThrowNotSupportedException(typeof(TKey));
            }
            return await base.Session.LoadAsync<TUser>(id, cancellationToken);
        }

        #endregion

        #region IUserPasswordStore

        public async virtual Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForOnlyNull(passwordHash, nameof(passwordHash));

            user.PasswordHash = passwordHash;

            await base.VoidTask();
        }

        public async virtual Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await Task.FromResult(user.PasswordHash);
        }

        public async virtual Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
        }

        #endregion

        #region IUserPhoneNumberStore

        public async virtual Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForOnlyNull(phoneNumber, nameof(phoneNumber));

            user.PhoneNumber = phoneNumber;

            await base.VoidTask();
        }

        public async virtual Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await Task.FromResult(user.PhoneNumber);
        }

        public async virtual Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await Task.FromResult(user.PhoneNumberConfirmed);
        }

        public async virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForOnlyNull(confirmed, nameof(confirmed));

            user.PhoneNumberConfirmed = confirmed;

            await base.VoidTask();
        }

        #endregion

        #region IUserRoleStore

        public async virtual Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForNull(roleName, nameof(roleName));

            var roleEntity = await Session.Query<IdentityRole<TKey>, IdentityRole_GetByName<TRole, TKey>>()
                                    .SingleOrDefaultAsync(x => String.Equals(x.Name.ToUpper(), roleName.ToUpper(), StringComparison.CurrentCultureIgnoreCase), cancellationToken);
            if (roleEntity == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "RoleNotFound {0}", roleName));
            }

            var ur = new Entity.IdentityUserRole<TKey> { UserId = user.Id, RoleId = roleEntity.Id };
            await base.Session.StoreAsync(ur, cancellationToken);
        }

        public async virtual Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForNull(roleName, nameof(roleName));

            var roleEntity = await Session.Query<IdentityRole<TKey>, IdentityRole_GetByName<TRole, TKey>>()
                                    .SingleOrDefaultAsync(x => String.Equals(x.Name.ToUpper(), roleName.ToUpper(), StringComparison.CurrentCultureIgnoreCase), cancellationToken);
            if (roleEntity != null)
            {
                var userRole = await IdentityUserRoles.FirstOrDefaultAsync(r => r.RoleId.Equals(roleEntity.Id) && r.UserId.Equals(user.Id), cancellationToken);
                if (userRole != null)
                {
                    base.Session.Delete(userRole);
                }
            }
        }

        public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            var userId = user.Id;
            IEnumerable<TKey> roleIds = await IdentityUserRoles.Where(x => x.UserId.Equals(userId)).Select(s => s.RoleId).ToListAsync(cancellationToken);
            var roles = await base.Session.LoadAsync<TRole, TKey>(roleIds, cancellationToken);

            return roles.Select(x => x.Name).ToList();
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForNull(roleName, nameof(roleName));

            var role = await this.IdentityRoles.SingleOrDefaultAsync(x => String.Equals(x.Name.ToUpper(), roleName.ToUpper(), StringComparison.CurrentCultureIgnoreCase), cancellationToken);
            if (role != null)
            {
                var userId = user.Id;
                var roleId = role.Id;
                return await IdentityUserRoles.AnyAsync(ur => ur.RoleId.Equals(roleId) && ur.UserId.Equals(userId), cancellationToken);
            }

            return false;
        }

        public async virtual Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForOnlyNull(roleName, nameof(roleName));

            var role = await this.IdentityRoles.SingleOrDefaultAsync(x => String.Equals(x.Name.ToUpper(), roleName.ToUpper(), StringComparison.CurrentCultureIgnoreCase), cancellationToken);
            if (role != null)
            {
                var roleId = role.Id;
                var iur = await IdentityUserRoles.Where(ur => ur.RoleId.Equals(roleId)).ToListAsync(cancellationToken);

                return await this.Session.LoadAsync<TUser, TKey>(iur.Select(s => s.UserId), cancellationToken);
            }
            return new List<TUser>();
        }

        #endregion

        #region IUserSecurityStampStore

        public async Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForOnlyNull(stamp, nameof(stamp));

            user.SecurityStamp = stamp;

            await base.VoidTask();
        }

        public async Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await Task.FromResult(user.SecurityStamp);
        }

        #endregion

        #region IUserTwoFactorStore

        public async Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));
            base.CheckArgumentForNull(enabled, nameof(enabled));

            user.TwoFactorEnabled = enabled;

            await base.VoidTask();
        }

        public async Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(user, nameof(user));

            return await Task.FromResult(user.TwoFactorEnabled);
        }

        #endregion
    }
}