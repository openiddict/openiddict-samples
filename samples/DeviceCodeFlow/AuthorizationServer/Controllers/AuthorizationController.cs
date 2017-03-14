/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using AuthorizationServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Core;
using OpenIddict.DeviceCodeFlow;
using Microsoft.AspNetCore.Authorization;
using AuthorizationServer.ViewModels.Shared;
using AuthorizationServer.Helpers;
using AuthorizationServer.ViewModels.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AuthorizationServer.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OpenIddictApplicationManager<ApplicationApplication> _applicationManager;
        private readonly DeviceCodeManager<ApplicationDeviceCode> _deviceCodeManager;
        private readonly DeviceCodeOptions _deviceCodeOptions;

        public AuthorizationController(
            IOptions<IdentityOptions> identityOptions,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            OpenIddictApplicationManager<ApplicationApplication> applicationManager,
            DeviceCodeManager<ApplicationDeviceCode> deviceCodeManager,
            DeviceCodeOptions deviceCodeOptions)
        {
            _identityOptions = identityOptions;
            _signInManager = signInManager;
            _userManager = userManager;
            _applicationManager = applicationManager;
            _deviceCodeManager = deviceCodeManager;
            _deviceCodeOptions = deviceCodeOptions;
        }

        [Authorize, HttpGet("~/connect/authorize_device")]
        public IActionResult ConnectDeviceCode()
        {
            return View();
        }

        [Authorize]
        [HttpPost("~/connect/authorize_device"), ValidateAntiForgeryToken]
        public async Task<IActionResult> ConnectDeviceCodeConfirmation(AuthorizeDeviceCodeRequest model)
        {
            var deviceCode = await _deviceCodeManager.FindByUserCodeAsync(model.UserCode);
            if (deviceCode == null)
            {
                ModelState.AddModelError(string.Empty, "Unrecognised or expired code.");
                return View("ConnectDeviceCode", model);
            }
            
            if (deviceCode.Application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            var application = await _applicationManager.FindByIdAsync(deviceCode.Application, Request.HttpContext.RequestAborted);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            return View(new AuthorizeDeviceCodeViewModel
            {
                UserCode = deviceCode.UserCode,
                Scope = deviceCode.Scope,
                ApplicationName = application.DisplayName
            });
        }

        [Authorize, FormValueRequired("submit.Accept")]
        [HttpPost("~/connect/device_code_authorization"), ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptDevice(AuthorizeDeviceCodeRequest request)
        {
            var deviceCode = await _deviceCodeManager.FindByUserCodeAsync(request.UserCode);
            if (deviceCode == null)
            {
                ModelState.AddModelError(string.Empty, "Unrecognised or expired code.");
                return View("ConnectDeviceCode", request);
            }

            if (deviceCode.Application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            var application = await _applicationManager.FindByIdAsync(deviceCode.Application, Request.HttpContext.RequestAborted);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            // Create a new authentication ticket.
            var ticket = await CreateTicketAsync(new OpenIdConnectRequest
            {
                ClientId = deviceCode.Application,
                Scope = deviceCode.Scope,
            }, user);

            await _deviceCodeManager.Authorize(deviceCode, user.Id);


            return View("AuthorizeDeviceCodeResult", new AuthorizeDeviceCodeResultViewModel
            {
                ApplicationName = application.DisplayName,
                Scope = deviceCode.Scope,
                Authorized = true
            });
        }

        [Authorize, FormValueRequired("submit.Deny")]
        [HttpPost("~/connect/device_code_authorization"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyDevice(AuthorizeDeviceCodeViewModel request)
        {
            var deviceCode = await _deviceCodeManager.FindByUserCodeAsync(request.UserCode);
            if (deviceCode == null)
            {
                ModelState.AddModelError(string.Empty, "Unrecognised or expired code.");
                return View("ConnectDeviceCode", request);
            }

            if (deviceCode.Application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            var application = await _applicationManager.FindByIdAsync(deviceCode.Application, Request.HttpContext.RequestAborted);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            await _deviceCodeManager.Revoke(deviceCode);

            // Notify OpenIddict that the authorization grant has been denied by the resource owner
            // to redirect the user agent to the client application using the appropriate response_mode.
            return View("AuthorizeDeviceCodeResult", new AuthorizeDeviceCodeResultViewModel
            {
                ApplicationName = application.DisplayName,
                Scope = deviceCode.Scope,
                Authorized = false
            });
        }

        [HttpPost("~/connect/device_token"), Produces("application/json")]
        public async Task<IActionResult> MintDeviceCode(string response_type, string client_id, string client_secret, string scope)
        {
            if (response_type != DeviceCodeFlowConstants.ResponseTypes.DeviceCode)
            {
                return BadRequest(new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidRequest,
                    ErrorDescription = "Invalid response_type"
                });
            }

            if (client_id == null)
            {
                return BadRequest(new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidRequest,
                    ErrorDescription = "Missing client_id"
                });
            }

            if (scope == null)
            {
                return BadRequest(new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidRequest,
                    ErrorDescription = "Missing scope"
                });
            }


            var application = await _applicationManager.FindByClientIdAsync(client_id, HttpContext.RequestAborted);
            if (application == null)
            {
                return BadRequest(new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            if (await _applicationManager.IsConfidentialAsync(application, Request.HttpContext.RequestAborted))
            {
                if (string.IsNullOrEmpty(client_secret) || !await _applicationManager.ValidateClientSecretAsync(application, client_secret, Request.HttpContext.RequestAborted))
                {
                    return BadRequest(new ErrorViewModel
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidClient,
                        ErrorDescription = "Confidential clients must supply the client_secret"
                    });
                }
            }
            else if (!string.IsNullOrEmpty(client_secret))
            {
                return BadRequest(new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Public clients must not submit a client_secret"
                });
            }

            var deviceCode = await _deviceCodeManager.CreateAsync(application.Id, scope);

            // issue user and device codes
            return Json(new DeviceCodeFlowViewModel
            {
                VerificationUri = Url.Action("ConnectDeviceCode", null, null, Request.Scheme),
                UserCode = deviceCode.UserCode,
                DeviceCode = deviceCode.DeviceCode,
                Interval = _deviceCodeOptions.Interval,
                ExpiresIn = (int)(deviceCode.ExpiresAt - DateTimeOffset.Now).TotalSeconds
            });
        }

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange(OpenIdConnectRequest request, string device_code)
        {
            Debug.Assert(request.IsTokenRequest(),
                "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
                "Make sure services.AddOpenIddict().AddMvcBinders() is correctly called.");

            if (request.IsDeviceCodeGrantType())
            {
                if (request.ClientId == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidClient,
                        ErrorDescription = "Missing required parameter client_id."
                    });
                }

                // exchange device code for tokens
                var application = await _applicationManager.FindByClientIdAsync(request.ClientId, HttpContext.RequestAborted);
                if (application == null)
                {
                    return BadRequest(new ErrorViewModel
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidClient,
                        ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                    });
                }

                var code = await _deviceCodeManager.FindByDeviceCodeAsync(device_code);

                if (code == null)
                {
                    return BadRequest(new ErrorViewModel
                    {
                        Error = OpenIdConnectConstants.Errors.AccessDenied,
                        ErrorDescription = "Access denied" // todo: use canonical descriptions message for these errors
                    });
                }

                if (code.AuthorizedOn == default(DateTimeOffset))
                {
                    return BadRequest(new ErrorViewModel
                    {
                        Error = DeviceCodeFlowConstants.Errors.DeviceCodeAuthorizationPending,
                        ErrorDescription = "Device code authorization pending"
                    });
                }

                var user = await _userManager.FindByIdAsync(code.Subject);

                if (user == null)
                {
                    return BadRequest(new ErrorViewModel
                    {
                        Error = OpenIdConnectConstants.Errors.ServerError,
                        ErrorDescription = "An internal error has occurred"
                    });
                }

                await _deviceCodeManager.Consume(code);

                // Ensure the user is still allowed to sign in.
                if (!await _signInManager.CanSignInAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The user is no longer allowed to sign in."
                    });
                }

                var ticket = await CreateTicketAsync(new OpenIdConnectRequest
                {
                    Scope = code.Scope,
                    ClientId = request.ClientId,
                    ClientSecret = request.ClientSecret,
                    GrantType = request.GrantType
                }, user);

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                ErrorDescription = "The specified grant type is not supported."
            });
        }

        private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest request, ApplicationUser user)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(principal,
                new AuthenticationProperties(),
                OpenIdConnectServerDefaults.AuthenticationScheme);

            // Set the list of scopes granted to the client application.
            ticket.SetScopes(new[]
            {
                OpenIdConnectConstants.Scopes.OpenId,
                OpenIdConnectConstants.Scopes.Email,
                OpenIdConnectConstants.Scopes.Profile,
                OpenIddictConstants.Scopes.Roles
            }.Intersect(request.GetScopes()));

            ticket.SetResources("resource-server");

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            foreach (var claim in ticket.Principal.Claims)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
                {
                    continue;
                }

                var destinations = new List<string>
                {
                    OpenIdConnectConstants.Destinations.AccessToken
                };

                // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
                // The other claims will only be added to the access_token, which is encrypted when using the default format.
                if ((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
                {
                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(OpenIdConnectConstants.Destinations.AccessToken);
            }

            return ticket;
        }
    }
}