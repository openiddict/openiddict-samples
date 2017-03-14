using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenIddict.DeviceCodeFlow
{
    public class DeviceCodeManager<TCode> : DeviceCodeManager<TCode, string>
        where TCode : class
    {
        public DeviceCodeManager(IDeviceCodeStore<TCode, string> deviceCodeStore, IServiceProvider services) : base(deviceCodeStore, services)
        {
        }
    }

    public class DeviceCodeManager<TCode, TKey>
        where TCode: class
    {
        public IDeviceCodeStore<TCode, TKey> DeviceCodeStore { get; set; }
        private readonly HttpContext _context;
        private readonly DeviceCodeOptions _options = new DeviceCodeOptions();
        private readonly ICodeGenerator _generator = new CodeGenerator();

        public DeviceCodeManager(IDeviceCodeStore<TCode, TKey> deviceCodeStore, IServiceProvider services)
        {
            DeviceCodeStore = deviceCodeStore;

            if (services != null)
            {
                _context = services.GetService<IHttpContextAccessor>()?.HttpContext;
                _options = services.GetService<DeviceCodeOptions>() ?? _options;
                _generator = services.GetService<ICodeGenerator>() ?? _generator;
            }
        }

        protected CancellationToken CancellationToken => _context?.RequestAborted ?? CancellationToken.None;

        public Task CreateAsync(TCode code)
        {
            return DeviceCodeStore.CreateAsync(code, CancellationToken);
        }

        public Task<TCode> CreateAsync(TKey application, string scope)
        {
            var userCode = _generator.NewCode(_options.UserCodeLength);
            var deviceCode = _generator.NewCode(_options.DeviceCodeLength);
            var expiresAt = DateTimeOffset.Now + TimeSpan.FromSeconds(_options.DeviceCodeDuration);
            return DeviceCodeStore.CreateAsync(application, scope, userCode, deviceCode, expiresAt, CancellationToken);
        }

        public Task<TCode> FindByIdAsync(TKey identifier)
        {
            return DeviceCodeStore.FindByIdAsync(identifier, CancellationToken);
        }

        public Task<TCode> FindByUserCodeAsync(TKey userCode)
        {
            return DeviceCodeStore.FindByUserCodeAsync(userCode, CancellationToken);
        }

        public Task<TCode> FindByDeviceCodeAsync(TKey deviceCode)
        {
            return DeviceCodeStore.FindByDeviceCodeAsync(deviceCode, CancellationToken);
        }

        public Task Authorize(TCode deviceCode, TKey subject)
        {
            return DeviceCodeStore.Authorize(deviceCode, subject, CancellationToken);
        }

        public Task Consume(TCode code)
        {
            return DeviceCodeStore.Consume(code, CancellationToken);
        }

        public Task Revoke(TCode code)
        {
            return DeviceCodeStore.Revoke(code, CancellationToken);
        }
    }
}
