using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerUtils.Validations.Exceptions;
using System;
using System.Threading.Tasks;

namespace PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers
{
    public class ProblemDetailsMiddleware
    {
        #region PRIVATE PROPERTIES
        private readonly RequestDelegate _next;
        private readonly ProblemFactory _factory;
        private readonly ILogger<ProblemDetailsMiddleware> _logger;
        #endregion


        #region CONSTRUCTOR
        public ProblemDetailsMiddleware(
            RequestDelegate next,
            ProblemFactory factory,
            ILogger<ProblemDetailsMiddleware> logger
        )
        {
            this._next = next ?? throw new ArgumentNullException($"{typeof(ProblemDetailsMiddleware).Namespace} > {nameof(RequestDelegate)}");
            this._factory = factory ?? throw new ArgumentNullException($"{typeof(ProblemDetailsMiddleware).Namespace} > {nameof(ProblemFactory)}");
            this._logger = logger ?? throw new ArgumentNullException($"{typeof(ProblemDetailsMiddleware).Namespace} > {nameof(ILogger<ProblemDetailsMiddleware>)}");
        }
        #endregion


        #region PUBLIC METHODS
        public async Task Invoke(HttpContext httpContext)
        { // DONE
            try
            {
                await _next(httpContext);

                if (httpContext.Response.StatusCode >= 400 && httpContext.Response.StatusCode <= 599)
                { // Only can override BAD RESPNSES 4XX and 5XX
                    await _handleProblem(httpContext);
                }
            }
            catch (BaseValidationException exception)
            {
                await _handleRestException(httpContext, exception);
            }
            catch (AggregateException exception) when (exception.InnerExceptions.Count == 1)
            {
                if (exception.InnerExceptions[0] is not BaseValidationException restException)
                { // Generic exception
                    await _handleException(httpContext, exception.InnerExceptions[0]);
                }
                else
                { // BaseValidationException
                    await _handleRestException(httpContext, restException);
                }
            }
            catch (Exception exception)
            {
                await _handleException(httpContext, exception);
            }
        }
        #endregion


        #region PRIVATE METHODS
        private Task _handleProblem(HttpContext httpContext)
        { // DONE
            var problemDetails = ProblemDetailsResponse.Create(httpContext);

            // Only override response was not started. https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0#create-a-middleware-pipeline-with-iapplicationbuilder
            if (httpContext.Response.HasStarted)
            { // TODO: it is not very correct when the response was setted by 'ValidationNotificationsFilter'. The 'problemDetails' does not have the error list
                this._logger.LogDebug($"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > Response already started > {problemDetails}");

                return Task.CompletedTask;
            }
            else if (httpContext.Response.ContentLength.HasValue)
            { // Only override response without "content length"

                // TODO: it is not very correct when the response was setted by 'ValidationNotificationsFilter. The 'problemDetails' does not have the error list
                this._logger.LogDebug($"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > Response already has content > {problemDetails}");

                return Task.CompletedTask;
            }
            else
            {
                this._logger.LogDebug($"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > {problemDetails}");

                this._factory.ClearResponse(httpContext, httpContext.Response.StatusCode);

                return _factory.WriteProblemDetails(httpContext, problemDetails);
            }
        }

        private Task _handleException(HttpContext httpContext, Exception exception)
        { // DONE
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var problemDetails = ProblemDetailsResponse.Create(httpContext);

            // Only override response was not started. https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0#create-a-middleware-pipeline-with-iapplicationbuilder
            if (httpContext.Response.HasStarted)
            {
                this._logger.LogError($"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > Response already started > {exception}");

                return Task.CompletedTask;
            }
            else if (httpContext.Response.ContentLength.HasValue)
            { // Only override response without "content length"
                this._logger.LogError($"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > Response already has content > {exception}");

                return Task.CompletedTask;
            }
            else
            {

                this._logger.LogError($"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > {exception}");

                this._factory.ClearResponse(httpContext, httpContext.Response.StatusCode);

                return _factory.WriteProblemDetails(httpContext, problemDetails);
            }

        }

        private Task _handleRestException(HttpContext httpContext, BaseValidationException exception)
        { // DONE
            httpContext.Response.StatusCode = (int)exception.StatusCode;

            var problemDetails = ProblemDetailsResponse.Create(httpContext, exception.StatusCode, exception.Notifications);

            // Only override response was not started. https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0#create-a-middleware-pipeline-with-iapplicationbuilder
            if (httpContext.Response.HasStarted)
            {
                this._logger.LogDebug($"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > Response already started > {exception}");

                return Task.CompletedTask;
            }
            else if (httpContext.Response.ContentLength.HasValue)
            { // Only override response without "content length"
                this._logger.LogDebug($"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > Response already has content > {exception}");

                return Task.CompletedTask;
            }
            else
            {
                this._logger.LogDebug($"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > {exception}");

                _factory.ClearResponse(httpContext, httpContext.Response.StatusCode);

                return _factory.WriteProblemDetails(httpContext, problemDetails);
            }

        }
        #endregion
    }

    public static class ProblemDetailsExtensions
    { // DONE
        /// <summary>
        /// Adds the required services for <see cref="UseProblemDetails"/> to work correctly,
        /// using the default options.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        public static IServiceCollection AddProblemDetails(this IServiceCollection services)
        { // DONE
            services.AddSingleton<ProblemFactory>();

            // Add ModelState hendler to format response. It needs to after services.AddSingleton<ProblemFactory>(); because the dependency in ProblemFactory
            services.AddModelStateResponse();

            return services;
        }

        /// <summary>
        /// Adds the <see cref="ProblemDetailsMiddleware"/> to the application pipeline.
        /// </summary>
        /// <param name="app">The application builder to add the middleware to.</param>
        public static IApplicationBuilder UseProblemDetails(this IApplicationBuilder app)
        { // DONE
            // Add error hendler to format error response and log error. Only fallback when the ProblemDetailsMiddleware cannot catch the error
            // UseErrorHandler (UseExceptionHandler) is the first middleware component added to the pipeline. Therefore, the Exception Handler Middleware catches any exceptions that occur in later calls.
            app.UseErrorHandler();

            return app.UseMiddleware<ProblemDetailsMiddleware>();
        }
    }
}