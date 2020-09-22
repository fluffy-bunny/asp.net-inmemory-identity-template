﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using InMemoryIdentityApp.Constants;
using InMemoryIdentityApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace InMemoryIdentityApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly List<OpenIdConnectSchemeRecord> _oidcConfigRecords;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            List<OpenIdConnectSchemeRecord> oidcConfigRecords,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _oidcConfigRecords = oidcConfigRecords;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            // lets see if we are currently logged in.
            // if so, then compare this user to the one comming in
            // if the same, do nothing.
            // if not, logout the current user and signin this new one.
            string currentNameIdClaimValue = null;
            string currentLoginProviderValue = null;

            if (User.Identity.IsAuthenticated)
            {
                // we will only create a new user if the user here is actually new.
                var qName = from claim in User.Claims
                            where claim.Type == ".externalNamedIdentitier"
                            select claim;
                var claimExternalNamedIdentifier = qName.FirstOrDefault();
                currentNameIdClaimValue = claimExternalNamedIdentifier?.Value;

                var qLoginProvider = from claim in User.Claims
                                     where claim.Type == ".loginProvider"
                                     select claim;
                var claimLoginProvider = qLoginProvider.FirstOrDefault();
                currentLoginProviderValue = claimLoginProvider?.Value;
            }

            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            // house keeping, lets make sure that this external provider is allowed
            var oidcConfig = (from item in _oidcConfigRecords
                              where item.Scheme == info.LoginProvider
                              select item).FirstOrDefault();
            if (oidcConfig == null)
            {
                // Something fishy going on here.
                throw new Exception($"Error getting oidcConfig for loginProvider:{info.LoginProvider}");
            }

            // Lets see how this user is, if its the same as the authenticated one we do nothing.
            var queryNameId = from claim in info.Principal.Claims
                              where claim.Type == ClaimTypes.NameIdentifier
                              select claim;
            var nameIdClaim = queryNameId.FirstOrDefault();
            if (User.Identity.IsAuthenticated
                && currentNameIdClaimValue == nameIdClaim.Value
                && currentLoginProviderValue == info.LoginProvider)
            {
                // this is a re login from the same user, so don't do anything;
                return LocalRedirect(returnUrl);
            }
            await _signInManager.SignOutAsync();

            var displayName = nameIdClaim.Value;

            var externalPrincipalClaims = info.Principal.Claims.ToList();

            var email = GetValueFromClaim(externalPrincipalClaims, ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email))
            {
                displayName = email;
            }
            var name = GetValueFromClaim(externalPrincipalClaims, "name");
            if (!string.IsNullOrWhiteSpace(name))
            {
                displayName = name;
            }

            var user = new IdentityUser
            {
                UserName = nameIdClaim.Value,
                Email = email
            };
            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                var newUser = await _userManager.FindByIdAsync(user.Id);
                var eClaims = new List<Claim>
                {
                    new Claim(".displayName", displayName),
                    new Claim(".externalNamedIdentitier",nameIdClaim.Value),
                    new Claim(".loginProvider",info.LoginProvider)
                };

                // normalized id.
                await _userManager.AddClaimsAsync(newUser, eClaims);

                await _signInManager.SignInAsync(user, isPersistent: false);
                await _userManager.DeleteAsync(user); // just using this inMemory userstore as a scratch holding pad
                _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                // READ the OIDC Tokens and store them in the Session.
                var oidc = await HarvestOidcDataAsync();
                HttpContext.Session.Set(Wellknown.OIDCSessionKey, new OpenIdConnectSessionDetails
                {
                    LoginProider = info.LoginProvider,
                    OIDC = oidc
                });

                return LocalRedirect(returnUrl);
            }
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }
        private string GetValueFromClaim(List<Claim> claims, string type)
        {
            var query = from claim in claims
                        where claim.Type == type
                        select claim;
            var theClaim = query.FirstOrDefault();
            var value = theClaim?.Value;
            return value;
        }
        private async Task<Dictionary<string, string>> HarvestOidcDataAsync()
        {
            var at = await HttpContext.GetTokenAsync(IdentityConstants.ExternalScheme, "access_token");
            var idt = await HttpContext.GetTokenAsync(IdentityConstants.ExternalScheme, "id_token");
            var rt = await HttpContext.GetTokenAsync(IdentityConstants.ExternalScheme, "refresh_token");
            var tt = await HttpContext.GetTokenAsync(IdentityConstants.ExternalScheme, "token_type");
            var ea = await HttpContext.GetTokenAsync(IdentityConstants.ExternalScheme, "expires_at");

            var oidc = new Dictionary<string, string>
            {
                {"access_token", at},
                {"id_token", idt},
                {"refresh_token", rt},
                {"token_type", tt},
                {"expires_at", ea}
            };
            return oidc;
        }
    }
}
