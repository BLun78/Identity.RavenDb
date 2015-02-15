# Identity.RavenDb
A vNext Microsoft.AspNet.Identity provider that use RavenDB as datastore

## Alpha Version ##
Beware it is the first apha realese. I need more time for mor UnitTest.

Greets
Blun78

## Requirments ##
* Microsoft .net Framework 4.6 (4.5.3) RC 
* c# 6.0
* Microsoft.AspNet.Identity in min Version 3.0
* RavenDb 3.0

## Features ##
* Drop-in replacement ASP.NET Identity with RavenDB as the backing store.
* Requires 6 document types and 3 Index
* based on the default Entities from Microsoft.AspNet.Identity 3.0
* Supports in RavenDB 'string', 'int' and 'long' for Document IDs 
* Supports additional profile properties on your application's user model, base on the 
 
## Supports ##
* Provides UserStore<TUser, TRole, TKey>
    * IUserStore<TUser>
    * IUserLoginStore<TUser>
    * IUserLockoutStore<TUser>
    * IUserRoleStore<TUser>
    * IUserClaimStore<TUser>
    * IUserPasswordStore<TUser>
    * IUserSecurityStampStore<TUser>
    * IUserTwoFactorStore<TUser>
    * IUserEmailStore<TUser>
    * IUserPhoneNumberStore<TUser>
    * IQueryableUserStore<TUser>
* Provide RoleStore<TRole, TKey>
    * IRoleStore<TRole>
    * IQueryableRoleStore<TRole>
    * IRoleClaimStore<TRole>
