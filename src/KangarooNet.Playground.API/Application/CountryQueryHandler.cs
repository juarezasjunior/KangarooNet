// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Playground.Application
{
    using System.Threading.Tasks;
    using KangarooNet.Playground.Entities;
    using KangarooNet.Playground.Infrastructure.DatabaseEntities;
    using KangarooNet.Playground.Infrastructure.DatabaseRepositories;
    using MediatR;

    public partial class CountryQueryHandler : IRequestHandler<CountryQueryRequest, CountryQueryResponse>
    {
        private readonly IApplicationDatabaseRepository applicationDatabaseRepository;

        public CountryQueryHandler(IApplicationDatabaseRepository applicationDatabaseRepository)
        {
            this.applicationDatabaseRepository = applicationDatabaseRepository;
        }

        public async Task<CountryQueryResponse> Handle(CountryQueryRequest request, CancellationToken cancellationToken)
        {
            var result = (await this.applicationDatabaseRepository.GetByConditionAsync<TbCountry, Country>(
                x => x.Where(y => y.CountryId == request.CountryId), cancellationToken))?.FirstOrDefault();

            return new CountryQueryResponse() { Entity = result };
        }
    }
}
