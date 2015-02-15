using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Blun.AspNet.Identity.RavenDb.Common;
using Microsoft.AspNet.Identity;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using Raven.Client.Linq;

namespace Blun.AspNet.Identity.RavenDb.Store
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public class RoleStore<TRole, TKey> : GenericStore<TKey>,
                                IRoleStore<TRole>,
                                IQueryableRoleStore<TRole>,
                                IRoleClaimStore<TRole>
                                where TRole : IdentityRole<TKey>
                                where TKey : IConvertible, IComparable, IEquatable<TKey>
    {
        #region CTOR

        public RoleStore(Func<IAsyncDocumentSession> getSession, IdentityErrorDescriber describer = null)
            : base(getSession, describer)
        {
        }

        public RoleStore(IAsyncDocumentSession session, IdentityErrorDescriber describer = null)
            : base(session, describer)
        {
        }

        #endregion

        #region Property IRavenQueryable

        internal IRavenQueryable<TRole> IdentityRoles => base.Session.Query<TRole>();

        internal IRavenQueryable<IdentityRoleClaim<TKey>> IdentityRoleClaims => base.Session.Query<IdentityRoleClaim<TKey>>();

        #endregion

        #region IRoleStore

        public async virtual Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(role, nameof(role));
            base.CheckArgumentForNull(role.Name, nameof(role.Name));

            await base.Session.StoreAsync(role, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);

            return IdentityResult.Success;
        }

        public async virtual Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(role, nameof(role));

            try
            {
                role.ConcurrencyStamp = Guid.NewGuid().ToString();
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

        public async virtual Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(role, nameof(role));

            try
            {
                base.Session.Delete(role);
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

        public async virtual Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(role, nameof(role));

            return await Task.FromResult(base.ConvertIdToString(role.Id));
        }
        public async Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(role, nameof(role));

            return await Task.FromResult(role.Name);
        }

        public async Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(role, nameof(role));

            role.Name = roleName;
            await base.VoidTask();
        }

        public async virtual Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(role, nameof(role));

            return await Task.FromResult(role.NormalizedName);
        }

        public async virtual Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(role, nameof(role));

            role.NormalizedName = normalizedName;
            await base.VoidTask();
        }

        public async virtual Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(roleId, nameof(roleId));

            var id = string.Empty;

            if (CheckNumeric())
            {
                id = base.CreateId(roleId, typeof(TRole));
            }
            else if (CheckString())
            {
                id = roleId;
            }
            else
            {
                ThrowTypeAccessException(typeof(TKey));
            }

            return await base.Session.LoadAsync<TRole>(id, cancellationToken);
        }

        public async virtual Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(normalizedName, nameof(normalizedName));

            return await IdentityRoles.FirstOrDefaultAsync(x => x.NormalizedName == normalizedName, cancellationToken);
        }

        #endregion

        #region IRoleClaimStore

        public async virtual Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(role, nameof(role));

            return await IdentityRoleClaims.Where(rc => rc.RoleId.Equals(role.Id))
                                                .Select(c => new Claim(c.ClaimType, c.ClaimValue))
                                                .ToListAsync(cancellationToken);
        }

        public async virtual Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(role, nameof(role));

            var roleClaim = new IdentityRoleClaim<TKey> { RoleId = role.Id, ClaimType = claim.Type, ClaimValue = claim.Value };

            await base.Session.StoreAsync(roleClaim, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);

            await base.VoidTask();
        }

        public async virtual Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            base.CheckArgumentForNull(role, nameof(role));
            base.CheckArgumentForNull(claim, nameof(claim));

            var claims = await IdentityRoleClaims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToListAsync(cancellationToken);
            foreach (var c in claims)
            {
                base.Session.Delete(c);
            }
            await base.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region IQueryableRoleStore

        public virtual IQueryable<TRole> Roles
        {
            get
            {
                base.ThrowIfDisposed();
                return IdentityRoles.AsQueryable();
            }
        }

        #endregion
    }
}