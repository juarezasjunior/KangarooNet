// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Domain.Entities
{
    using KangarooNet.Domain;
    using MediatR;

    public class GenerateTokenRequest<TApplicationUser> : IRequest<TokenResponse>
        where TApplicationUser : IApplicationUser, new()
    {
        public TApplicationUser ApplicationUser { get; set; }
    }
}
