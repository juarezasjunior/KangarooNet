// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.UI.API.Middlewares
{
    using System;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using KangarooNet.Domain.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class KangarooNetExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<KangarooNetExceptionMiddleware> logger;

        public KangarooNetExceptionMiddleware(RequestDelegate next, ILogger<KangarooNetExceptionMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch (KangarooNetSecurityException exception)
            {
                this.logger.LogError(exception.ToString());
                await this.HandleExceptionAsync(context, exception.InternalErrorCode, exception.ErrorCode, exception.AdditionalInfo, HttpStatusCode.Unauthorized);
            }
            catch (KangarooNetException exception)
            {
                this.logger.LogError(exception.ToString());
                await this.HandleExceptionAsync(context, exception.InternalErrorCode, exception.ErrorCode, exception.AdditionalInfo);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception.ToString());
                await this.HandleExceptionAsync(context);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, KangarooNetErrorCode internalErrorCode = KangarooNetErrorCode.Others, int? errorCode = null, string? additionalInfo = null, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)httpStatusCode;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new KangarooNetExceptionInfo()
                {
                    InternalErrorCode = internalErrorCode,
                    ErrorCode = errorCode,
                    AdditionalInfo = additionalInfo,
                }));
        }
    }
}
