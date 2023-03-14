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

    public class GenerateApplicationUserToInsertResponse<TApplicationUser>
        where TApplicationUser : IApplicationUser, new()
    {
        public TApplicationUser ApplicationUser { get; set; }
    }
}
