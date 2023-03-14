// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Application.Auth.Handlers
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using KangarooNet.Domain;
    using KangarooNet.Domain.CacheKeys;
    using KangarooNet.Domain.Claims;
    using KangarooNet.Domain.Entities;
    using KangarooNet.Domain.OptionsSettings;
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    public abstract class GenerateTokenHandlerBase<TApplicationUser> : IRequestHandler<GenerateTokenRequest<TApplicationUser>, TokenResponse>
        where TApplicationUser : IdentityUser<Guid>, IApplicationUser, new()
    {
        private readonly UserManager<TApplicationUser> userManager;
        private readonly IDistributedCache distributedCache;
        private readonly JwtOptions jwtOptions;

        public GenerateTokenHandlerBase(
            UserManager<TApplicationUser> userManager,
            IDistributedCache distributedCache,
            IOptions<JwtOptions> jwtOptions)
        {
            this.userManager = userManager;
            this.distributedCache = distributedCache;
            this.jwtOptions = jwtOptions.Value;
        }

        public async Task<TokenResponse> Handle(GenerateTokenRequest<TApplicationUser> request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var validIssuer = this.jwtOptions.JwtIssuer;
            var validAudience = this.jwtOptions.JwtAudience;
            var tokenExpirationInMinutes = this.jwtOptions.JwtExpiryInMinutes;
            var refreshTokenExpirationInMinutes = this.jwtOptions.JwtRefreshTokenExpiryInMinutes;
            var secretKey = this.jwtOptions.JwtSecurityKey;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenExpiration = DateTime.Now.AddMinutes(tokenExpirationInMinutes);
            var refreshTokenExpiration = DateTime.Now.AddMinutes(refreshTokenExpirationInMinutes);

            var claims = new List<Claim>()
            {
                new Claim(KangarooNetClaims.UserId, request.ApplicationUser.Id.ToString()),
                new Claim(KangarooNetClaims.FullName, request.ApplicationUser.FullName),
                new Claim(ClaimTypes.Name, request.ApplicationUser.UserName),
                new Claim(ClaimTypes.Email, request.ApplicationUser.Email),
            };

            var roles = await this.userManager.GetRolesAsync(request.ApplicationUser);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtSecurityToken = new JwtSecurityToken(
                validIssuer,
                validAudience,
                claims,
                expires: tokenExpiration,
                signingCredentials: creds);

            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            var refreshToken = this.GenerateRandomToken();

            var refreshTokenDistributedCacheEntryOptions = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = refreshTokenExpiration,
            };

            await this.distributedCache.SetStringAsync(string.Format(JwtCacheKeys.RefreshTokenKey, request.ApplicationUser.Email), refreshToken, refreshTokenDistributedCacheEntryOptions);
            await this.distributedCache.SetStringAsync(
                string.Format(JwtCacheKeys.RefreshTokenExpirationTimeKey, request.ApplicationUser.Email),
                DateTime.Now.AddMinutes(this.jwtOptions.JwtRefreshTokenExpiryInMinutes).ToString(),
                refreshTokenDistributedCacheEntryOptions);
            await this.distributedCache.RemoveAsync(string.Format(JwtCacheKeys.UserHasLogoutKey, request.ApplicationUser.Email));

            return new TokenResponse()
            {
                Token = token,
                RefreshToken = refreshToken,
            };
        }

        private string GenerateRandomToken() => Guid.NewGuid().ToString() + DateTime.Now.ToString("ddMMyyyyHHmmss") + Guid.NewGuid().ToString();
    }
}
