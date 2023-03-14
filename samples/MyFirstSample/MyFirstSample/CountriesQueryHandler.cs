namespace MyFirstSample
{
    using System.Threading.Tasks;
    using KangarooNet.Application;
    using MediatR;
    using MyFirstSample.Entities;
    using MyFirstSample.Infrastructure.DatabaseEntities;
    using MyFirstSample.Infrastructure.DatabaseRepositories;

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
