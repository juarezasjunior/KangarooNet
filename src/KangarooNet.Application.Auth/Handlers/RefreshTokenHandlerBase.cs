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
    using KangarooNet.Application.Services;
    using KangarooNet.Domain;
    using KangarooNet.Domain.CacheKeys;
    using KangarooNet.Domain.Entities;
    using KangarooNet.Domain.Entities.Auth;
    using KangarooNet.Domain.Exceptions;
    using KangarooNet.Domain.OptionsSettings;
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    public abstract class RefreshTokenHandlerBase<TApplicationUser, TRefreshTokenRequest, TRefreshTokenResponse>
        : IRequestHandler<TRefreshTokenRequest, TRefreshTokenResponse>
        where TApplicationUser : IdentityUser<Guid>, IApplicationUser, new()
        where TRefreshTokenRequest : IRefreshTokenRequest, IRequest<TRefreshTokenResponse>
        where TRefreshTokenResponse : IRefreshTokenResponse, new()
    {
        private readonly UserManager<TApplicationUser> userManager;
        private readonly IMediator mediator;
        private readonly ICurrentUserService currentUserService;
        private readonly IDistributedCache distributedCache;
        private readonly JwtOptions jwtOptions;

        public RefreshTokenHandlerBase(
            UserManager<TApplicationUser> userManager,
            IMediator mediator,
            ICurrentUserService currentUserService,
            IDistributedCache distributedCache,
            IOptions<JwtOptions> jwtOptions)
        {
            this.userManager = userManager;
            this.mediator = mediator;
            this.currentUserService = currentUserService;
            this.distributedCache = distributedCache;
            this.jwtOptions = jwtOptions.Value;
        }

        public async Task<TRefreshTokenResponse> Handle(TRefreshTokenRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentUserId = this.currentUserService.CurrentUserId;
            var principal = this.GetPrincipalFromToken(request.Token);

            var email = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;
            var applicationUser = await this.userManager.FindByEmailAsync(email);

            if (applicationUser == null
                || applicationUser.Id != currentUserId)
            {
                throw new KangarooNetSecurityException();
            }

            var refreshToken = await this.distributedCache.GetStringAsync(string.Format(JwtCacheKeys.RefreshTokenKey, applicationUser.Email));
            var refreshTokenExpiration = await this.distributedCache.GetStringAsync(string.Format(JwtCacheKeys.RefreshTokenExpirationTimeKey, applicationUser.Email));

            if (string.IsNullOrEmpty(refreshToken)
                || string.IsNullOrEmpty(refreshTokenExpiration)
                || request.RefreshToken != refreshToken)
            {
                throw new KangarooNetSecurityException();
            }

            if (!DateTime.TryParse(refreshTokenExpiration, out var refreshTokenExpirationDateTime))
            {
                throw new KangarooNetSecurityException();
            }

            if (refreshTokenExpirationDateTime < DateTime.Now)
            {
                throw new KangarooNetSecurityException();
            }

            var result = await this.mediator.Send(new GenerateTokenRequest<TApplicationUser>()
            {
                ApplicationUser = applicationUser,
            });

            return new TRefreshTokenResponse()
            {
                Token = result.Token,
                RefreshToken = result.RefreshToken,
            };
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var validIssuer = this.jwtOptions.JwtIssuer;
            var validAudience = this.jwtOptions.JwtAudience;
            var secretKey = this.jwtOptions.JwtSecurityKey;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = false,
                ValidIssuer = validIssuer,
                ValidAudience = validAudience,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken
                || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new KangarooNetSecurityException();
            }

            return principal;
        }
    }
}
