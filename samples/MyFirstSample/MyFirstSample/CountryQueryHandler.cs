namespace MyFirstSample
{
    using System.Threading.Tasks;
    using KangarooNet.Application;
    using MediatR;
    using MyFirstSample.Entities;
    using MyFirstSample.Infrastructure.DatabaseEntities;
    using MyFirstSample.Infrastructure.DatabaseRepositories;

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