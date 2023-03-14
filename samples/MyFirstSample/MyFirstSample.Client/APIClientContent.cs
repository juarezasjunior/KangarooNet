﻿// <auto-generated>
namespace MyFirstSample.Client.APIClient
{
    using System;
    using System.Threading.Tasks;
    using KangarooNet.UI.APIClient;
    using Refit;
    using MyFirstSample.Client.Entities;

    public partial interface ICountryClient : IAPIClient
    {
        [Post("/api/CountryHandler/Post")]
        public Task<CountryHandlerResponse> PostAsync([Body] CountryHandlerRequest request);
    }

    public partial interface ICountryQueryClient : IAPIClient
    {
        [Get("/api/CountryQuery/GetEntity")]
        public Task<CountryQueryResponse> GetEntityAsync([Query] CountryQueryRequest request);
    }

    public partial interface ICountriesQueryClient : IAPIClient
    {
        [Get("/api/CountriesQuery/GetEntities")]
        public Task<CountriesQueryResponse> GetEntitiesAsync([Query] CountriesQueryRequest request);
    }

    public partial interface IAuthClient : IAPIClient
    {
        [Post("/api/Auth/InsertApplicationUser")]
        public Task<ApplicationUserInsertResponse> InsertApplicationUserAsync([Body] ApplicationUserInsertRequest request);

        [Post("/api/Auth/Login")]
        public Task<LoginResponse> LoginAsync([Body] LoginRequest request);

        [Post("/api/Auth/RefreshToken")]
        public Task<RefreshTokenResponse> RefreshTokenAsync([Body] RefreshTokenRequest request);

        [Post("/api/Auth/Logout")]
        public Task<LogoutResponse> LogoutAsync([Body] LogoutRequest request);

        [Post("/api/Auth/ChangePassword")]
        public Task<ChangePasswordResponse> ChangePasswordAsync([Body] ChangePasswordRequest request);
    }

}