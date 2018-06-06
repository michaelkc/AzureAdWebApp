using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureAdWebapp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.ClaimActions.Remove("amr");
                    options.RequireHttpsMetadata = true;
                    options.MetadataAddress =
                        "https://login.microsoftonline.com/698134b6-7f35-441a-99c6-6cb8df7a36b1/.well-known/openid-configuration";
                    options.ClientId = "a8dec748-dde0-48d3-8317-30e91fcb2e75";
                    options.SaveTokens = true;
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProvider = context =>
                        {
                            var domainHint = context.Request.Query["domain_hint"];
                            if (!string.IsNullOrWhiteSpace(domainHint))
                            {
                                context.ProtocolMessage.SetParameter("domain_hint", domainHint);
                            }

                            return Task.CompletedTask;
                        },
                        OnRemoteFailure = context => {
                            if (context.Failure.Message.Contains("AADSTS51004", StringComparison.Ordinal))
                            {
                                context.HandleResponse();
                                context.Response.Redirect("/Home/UserNotFound");
                            }
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            // Save the id_token as a claim 1
                            context.HttpContext.Items[".SEGES.id_token"] = context.ProtocolMessage.IdToken;
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context => {
                            // Save the id_token as a claim 2
                            string idToken = context.HttpContext.Items[".SEGES.id_token"]?.ToString() ?? "<empty>";
                            var identity = new ClaimsIdentity(new List<Claim>());
                            identity.AddClaim(new Claim("id_token", idToken));
                            context.Principal.AddIdentity(identity);
                            return Task.CompletedTask;
                        }
                    };
                });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
