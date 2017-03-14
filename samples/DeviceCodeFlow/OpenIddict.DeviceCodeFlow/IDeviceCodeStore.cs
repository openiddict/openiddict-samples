using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace OpenIddict.DeviceCodeFlow
{
    public interface IDeviceCodeStore<TCode> : IDeviceCodeStore<TCode, string>
        where TCode : class
    {
    }

    public interface IDeviceCodeStore<TCode, TKey>
        where TCode : class
    {
        Task CreateAsync(TCode code, CancellationToken cancellationToken);

        Task<TCode> CreateAsync(TKey application, string scope, string userCode, string deviceCode, DateTimeOffset expiresAt, CancellationToken cancellationToken);

        Task<TCode> FindByIdAsync(TKey identifier, CancellationToken cancellationToken);

        Task<TCode> FindByUserCodeAsync(TKey userCode, CancellationToken cancellationToken);

        Task<TCode> FindByDeviceCodeAsync(TKey deviceCode, CancellationToken cancellationToken);

        Task<TKey> GetIdAsync(TCode code, CancellationToken cancellationToken);

        Task<TKey> GetApplicationIdAsync(TCode code, CancellationToken cancellationToken);

        Task<string> GetUserCodeAsync(TCode code, CancellationToken cancellationToken);

        Task<string> GetDeviceCodeAsync(TCode code, CancellationToken cancellationToken);

        Task<string> GetScopeAsync(TCode code, CancellationToken cancellationToken);

        Task<TKey> GetSubjectAsync(TCode code, CancellationToken cancellationToken);

        Task Authorize(TCode code, TKey subject, CancellationToken cancellationToken);

        Task Consume(TCode code, CancellationToken cancellationToken);

        Task Revoke(TCode code, CancellationToken cancellationToken);
    }
}
