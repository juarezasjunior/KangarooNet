// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IEntitiesQueryResponse<TEntity, TCollectionType> : IEndpointResponse
        where TEntity : class, IEntity
        where TCollectionType : ICollection<TEntity>
    {
        public TCollectionType Entities { get; set; }
    }
}
