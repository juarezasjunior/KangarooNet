// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using KangarooNet.Domain;
    using KangarooNet.Domain.Entities.Auth;
    using MediatR;

    public class GenerateApplicationUserToInsertRequest<TApplicationUser, TApplicationUserInsertRequest> : IRequest<GenerateApplicationUserToInsertResponse<TApplicationUser>>
        where TApplicationUser : IApplicationUser, new()
        where TApplicationUserInsertRequest : IApplicationUserInsertRequest
    {
        public TApplicationUserInsertRequest ApplicationUserInsertRequest { get; set; }
    }
}
