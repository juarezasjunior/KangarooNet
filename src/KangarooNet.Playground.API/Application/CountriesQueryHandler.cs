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

    public partial class CountriesQueryHandler : IRequestHandler<CountriesQueryRequest, CountriesQueryResponse>
    {
        private readonly IApplicationDatabaseRepository applicationDatabaseRepository;

        public CountriesQueryHandler(IApplicationDatabaseRepository applicationDatabaseRepository)
        {
            this.applicationDatabaseRepository = applicationDatabaseRepository;
        }

        public async Task<CountriesQueryResponse> Handle(CountriesQueryRequest request, CancellationToken cancellationToken)
        {
            var result = await this.applicationDatabaseRepository.GetAllAsync<TbCountry, Country>(cancellationToken);

            return new CountriesQueryResponse() { Entities = result };
        }
    }
}
