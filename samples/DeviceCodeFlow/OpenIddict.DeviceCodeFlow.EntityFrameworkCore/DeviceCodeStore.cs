using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.DeviceCodeFlow;
using OpenIddict.Models;

namespace OpenIddict.DeviceCodeFlow.EntityFrameworkCore
{
    /// <summary>
    /// Represents a new instance of a persistence store for device codes, using the default implementation
    /// with a string as a primary key.
    /// </summary>
    public class DeviceCodeStore : DeviceCodeStore<
        OpenIddictDeviceCode<string>,
        DbContext>
    {
        /// <summary>
        /// Constructs a new instance of <see cref="DeviceCodeStore"/>.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public DeviceCodeStore(DbContext context, IdentityErrorDescriber describer = null) : base(context, describer)
        {
        }
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified device code type.
    /// </summary>
    /// <typeparam name="TDeviceCode">The type representing a device code.</typeparam>
    /// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
    public class DeviceCodeStore<TDeviceCode, TContext> 
        : DeviceCodeStore<TDeviceCode, TContext, string>
        where TDeviceCode : OpenIddictDeviceCode<string>, new()
        where TContext : DbContext
    {
        /// <summary>
        /// Constructs a new instance of <see cref="DeviceCodeStore{TDeviceCode, TContext}"/>.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public DeviceCodeStore(TContext context, IdentityErrorDescriber describer = null) : base(context, describer)
        {
        }
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified device code type.
    /// </summary>
    /// <typeparam name="TDeviceCode">The type representing a device code.</typeparam>
    /// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
    public class DeviceCodeStore<TDeviceCode, TContext, TKey> :
        IDeviceCodeStore<TDeviceCode, TKey>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
        where TDeviceCode: OpenIddictDeviceCode<TKey>, new()
    {
        /// <summary>
        /// Creates a new instance of <see cref="DeviceCodeStore"/>.
        /// </summary>
        /// <param name="context">The context used to access the store.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
        public DeviceCodeStore(TContext context, IdentityErrorDescriber describer = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            Context = context;
            ErrorDescriber = describer ?? new IdentityErrorDescriber();
        }

        private bool _disposed;

        /// <summary>
        /// Gets the database context for this store.
        /// </summary>
        public TContext Context { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IdentityErrorDescriber"/> for any error that occurred with the current operation.
        /// </summary>
        public IdentityErrorDescriber ErrorDescriber { get; set; }

        private DbSet<TDeviceCode> DeviceCodesSet => Context.Set<TDeviceCode>();

        /// <summary>
        /// Gets or sets a flag indicating if changes should be persisted after CreateAsync, UpdateAsync and DeleteAsync are called.
        /// </summary>
        /// <value>
        /// True if changes should be automatically persisted, otherwise false.
        /// </value>
        public bool AutoSaveChanges { get; set; } = true;

        /// <summary>Saves the current store.</summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected Task SaveChanges(CancellationToken cancellationToken)
        {
            return AutoSaveChanges ? Context.SaveChangesAsync(cancellationToken) : Task.FromResult(0);
        }

        public virtual async Task CreateAsync(TDeviceCode code, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }
            Context.Add(code);
            await SaveChanges(cancellationToken);
        }

        public virtual async Task<TDeviceCode> CreateAsync(TKey application, string scope,
            string userCode, string deviceCode, DateTimeOffset expiresAt,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var code = new TDeviceCode
            {
                Application = application,
                Scope = scope,
                UserCode = userCode,
                DeviceCode = deviceCode,
                ExpiresAt = expiresAt
            };
            await CreateAsync(code, cancellationToken);
            return code;
        }

        public virtual Task<TDeviceCode> FindByIdAsync(TKey identifier, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }
            return DeviceCodesSet.FirstOrDefaultAsync(d => identifier.Equals(d.Id), cancellationToken);
        }

        public virtual Task<TDeviceCode> FindByUserCodeAsync(TKey userCode, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (userCode == null)
            {
                throw new ArgumentNullException(nameof(userCode));
            }
            return DeviceCodesSet.FirstOrDefaultAsync(d => userCode.Equals(d.UserCode), cancellationToken);
        }

        public virtual Task<TDeviceCode> FindByDeviceCodeAsync(TKey deviceCode, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (deviceCode == null)
            {
                throw new ArgumentNullException(nameof(deviceCode));
            }
            return DeviceCodesSet.FirstOrDefaultAsync(d => deviceCode.Equals(d.DeviceCode), cancellationToken);
        }

        public virtual Task<TKey> GetIdAsync(TDeviceCode code, CancellationToken cancellationToken)
        {
            return Task.FromResult(code.Id);
        }

        public virtual Task<TKey> GetApplicationIdAsync(TDeviceCode code, CancellationToken cancellationToken)
        {
            return Task.FromResult(code.Application);
        }

        public virtual Task<TKey> GetSubjectAsync(TDeviceCode code, CancellationToken cancellationToken)
        {
            return Task.FromResult(code.Subject);
        }

        public virtual Task<string> GetUserCodeAsync(TDeviceCode code, CancellationToken cancellationToken)
        {
            return Task.FromResult(code.UserCode);
        }

        public virtual Task<string> GetDeviceCodeAsync(TDeviceCode code, CancellationToken cancellationToken)
        {
            return Task.FromResult(code.DeviceCode);
        }

        public virtual Task<string> GetScopeAsync(TDeviceCode code, CancellationToken cancellationToken)
        {
            return Task.FromResult(code.Scope);
        }

        public virtual async Task Authorize(TDeviceCode code, TKey subject, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }
            DeviceCodesSet.Attach(code);
            code.AuthorizedOn = DateTimeOffset.Now;
            code.Subject = subject;
            await SaveChanges(cancellationToken);
        }

        public virtual async Task Consume(TDeviceCode code, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }
            DeviceCodesSet.Remove(code);
            await SaveChanges(cancellationToken);
        }

        public virtual async Task Revoke(TDeviceCode code, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }
            DeviceCodesSet.Remove(code);
            await SaveChanges(cancellationToken);
        }

        /// <summary>
        /// Throws if this class has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <summary>
        /// Dispose the store
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
        }
    }
}
