using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Permissions.Models;

namespace Permissions
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.

            // https://github.com/aspnet/Security/issues/1043#issuecomment-261937401
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddIdentity<User, Role>(opts =>
            {
                opts.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
                opts.ClaimsIdentity.UserNameClaimType = "full_name";
            });
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // JWT
            var signingKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("Test Authentication Security Key"));
            var tvp = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = "Test",
                ValidateAudience = true,
                ValidAudience = "Test",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMilliseconds(500)
            };

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                TokenValidationParameters = tvp,
            });

            app.UseMvc();
        }
    }
}
