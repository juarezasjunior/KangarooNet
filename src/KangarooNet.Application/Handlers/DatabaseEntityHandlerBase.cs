// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Application.Handlers
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using KangarooNet.Application.Services;
    using KangarooNet.Domain;
    using KangarooNet.Domain.DatabaseEntities;
    using KangarooNet.Domain.Entities;
    using KangarooNet.Infrastructure.DatabaseRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public abstract class DatabaseEntityHandlerBase<TDbContext, TDatabaseEntity, TEntity, TEntityHandlerRequest, TEntityHandlerResponse>
        : IRequestHandler<TEntityHandlerRequest, TEntityHandlerResponse>
        where TDbContext : DbContext
        where TDatabaseEntity : class, IDatabaseEntity
        where TEntity : class, IEntity
        where TEntityHandlerRequest : class, IEntityHandlerRequest<TEntity>, IRequest<TEntityHandlerResponse>
        where TEntityHandlerResponse : class, IEntityHandlerResponse<TEntity>, new()
    {
        private readonly IDatabaseRepository<TDbContext> databaseRepository;
        private readonly IMapper mapper;
        private readonly ICurrentUserService currentUserService;

        public DatabaseEntityHandlerBase(
            IDatabaseRepository<TDbContext> databaseRepository,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            this.databaseRepository = databaseRepository;
            this.mapper = mapper;
            this.currentUserService = currentUserService;
        }

        public async Task<TEntityHandlerResponse> Handle(TEntityHandlerRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (request == null)
            {
                throw new ArgumentNullException();
            }

            if (request is IHasAuditLog auditLog && request is IHasDataState dataState)
            {
                if (dataState.DataState == DataState.Inserted)
                {
                    auditLog.CreatedAt = DateTimeOffset.Now;
                    auditLog.CreatedByUserName = this.currentUserService.GetCurrentUserNameToAudit();
                }
                else if (dataState.DataState == DataState.Updated)
                {
                    auditLog.UpdatedAt = DateTimeOffset.Now;
                    auditLog.UpdatedByUserName = this.currentUserService.GetCurrentUserNameToAudit();
                }
            }

            var databaseEntity = this.databaseRepository.ApplyChanges<TDatabaseEntity, TEntity>(request.Entity);
            await this.databaseRepository.SaveAsync(cancellationToken);

            var entity = this.mapper.Map<TEntity>(databaseEntity);

            request.Entity = entity;

            return new TEntityHandlerResponse()
            {
                Entity = entity,
            };
        }
    }
}
