using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerUtils.Net.Constants;
using System;
using System.Net;

namespace PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers
{
    // ERROR with ProblemDetails: https://andrewlock.net/creating-a-custom-error-handler-middleware-function/
    // Official documentation: https://docs.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-5.0#exception-handler
    // Logger eeror: https://stackoverflow.com/questions/39192063/dependency-injection-on-the-fly-in-asp-net-core
    // Costumize error code response: https://weblog.west-wind.com/posts/2016/oct/16/error-handling-and-exceptionfilter-dependency-injection-for-aspnet-core-apis

    public static class ErrorHandlerMiddleware
    { // DONE
        public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder app)
        {
            ILoggerFactory loggerFactory = app.ApplicationServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            ILogger logger = loggerFactory.CreateLogger("ErrorHandler");

            ProblemFactory factory = app.ApplicationServices.GetService(typeof(ProblemFactory)) as ProblemFactory;

            return app.UseExceptionHandler(appError =>
                appError.Run(async httpContext =>
                {
                    // DONE
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    IExceptionHandlerFeature exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
                    var exception = exceptionDetails?.Error;

                    // Create response
                    var problemDetails = ProblemDetailsResponse.Create(httpContext);

                    if (exception == null)
                    {
                        logger.LogError(
                            exception,
                            $"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > Unknown error"
                        );
                    }
                    else
                    {
                        // Inprovement exceptions when only will be one
                        if (exception is AggregateException aggregateException && aggregateException.InnerExceptions.Count == 1)
                        {
                            exception = aggregateException.InnerExceptions[0];
                        }

                        logger.LogError(
                            exception,
                            $"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > {exception}"
                        );
                    }

                    factory.ClearResponse(httpContext, httpContext.Response.StatusCode);
                    httpContext.Response.ContentType = ExtendedMediaTypeNames.ProblemApplication.JSON;

                    // Write error details in body response
                    await httpContext.Response.WriteAsync(problemDetails.ToString());
                })
            );
        }
    }
}