using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Test.Models;

namespace Test
{
    public class RemoteRolesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RemoteRolesOptions _options;

        public RemoteRolesMiddleware(RequestDelegate next, RemoteRolesOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User != null && context.User.Identity?.IsAuthenticated == true)
            {
                // User was authenticated using JWT, get their roles
                using (var client = new HttpClient())
                {
                    var req = new HttpRequestMessage(HttpMethod.Get, new Uri(_options.GetBaseUri(), "roles/_current"));
                    var header = context.Request.Headers["Authorization"].First(x => x.StartsWith($"{_options.AuthenticationScheme} ", StringComparison.OrdinalIgnoreCase));
                    req.Headers.Authorization = new AuthenticationHeaderValue(_options.AuthenticationScheme, header.Split(' ')[1]);
                    var res = await client.SendAsync(req);
                    if (!res.IsSuccessStatusCode)
                    {
                        context.Response.StatusCode = 403;
                        return;
                    }
                    var assignments = JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<Role>>>(await res.Content.ReadAsStringAsync());
                    IEnumerable<Role> roles;
                    if (!assignments.TryGetValue("Test", out roles) || !roles.Any())
                    {
                        context.Response.StatusCode = 403;
                        return;
                    }
                    var claims = new List<Claim>();
                    foreach (var role in roles)
                        claims.Add(new Claim(ClaimTypes.Role, role.SystemName));
                    var newId = new ClaimsIdentity(claims, "Test", "full_name", ClaimTypes.Role);
                    context.User.AddIdentity(newId);
                }
            }
            await _next(context);
        }
    }

    public class RemoteRolesOptions
    {
        public string PermissionServer { get; set; } = "http://localhost/";
        public string AuthenticationScheme { get; set; } = "Bearer";
        public string RoleClaimType { get; set; } = ClaimTypes.Role;

        internal Uri GetBaseUri(){
            return new Uri(PermissionServer);
        }
    }

    public static class RemoteRolesMiddlewareExtensions
    {
        public static IApplicationBuilder UseRemoteRoles(this IApplicationBuilder builder, RemoteRolesOptions options)
        {
            return builder.UseMiddleware<RemoteRolesMiddleware>(options);
        }
    }
}