// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Infrastructure.DatabaseRepositories
{
    using KangarooNet.Domain.DatabaseEntities;
    using KangarooNet.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public interface IDatabaseRepository<TDbContext>
        where TDbContext : DbContext
    {
        /// <summary>
        /// Apply the changes in the memory by the data state field.
        /// </summary>
        /// <typeparam name="TDatabaseEntity">Database entity.</typeparam>
        /// <typeparam name="TEntity">Entity (DTO).</typeparam>
        /// <param name="entity">Entity.</param>
        /// <returns>The changed database entity.</returns>
        public TDatabaseEntity ApplyChanges<TDatabaseEntity, TEntity>(TEntity entity)
            where TDatabaseEntity : class, IDatabaseEntity
            where TEntity : class, IEntity;

        /// <summary>
        /// Save the changes applied in database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        public Task SaveAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <typeparam name="TDatabaseEntity">Database entity.</typeparam>
        /// <typeparam name="TDestination">Entity DTO, Summary DTO or other destination object.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All entities found.</returns>
        public Task<IList<TDestination>> GetAllAsync<TDatabaseEntity, TDestination>(CancellationToken cancellationToken = default)
            where TDatabaseEntity : class, IDatabaseEntity
            where TDestination : class;

        /// <summary>
        /// Get entities by condition.
        /// </summary>
        /// <typeparam name="TDatabaseEntity">Database entity.</typeparam>
        /// <typeparam name="TDestination">Entity DTO, Summary DTO or other destination object.</typeparam>
        /// <param name="queryable">Query expression.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All entities found.</returns>
        public Task<IList<TDestination>> GetByConditionAsync<TDatabaseEntity, TDestination>(Func<IQueryable<TDatabaseEntity>, IQueryable<TDatabaseEntity>> queryable, CancellationToken cancellationToken = default)
            where TDatabaseEntity : class, IDatabaseEntity
            where TDestination : class;
    }
}