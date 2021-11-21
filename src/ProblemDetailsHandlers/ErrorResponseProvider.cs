using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerUtils.Net.Constants;
using System.Net;

namespace PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers
{
    public class ErrorResponseProvider : IErrorResponseProvider
    {
        private readonly ILogger<ErrorResponseProvider> _logger;

        public ErrorResponseProvider(
            IServiceCollection services
        )
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var loggerFactory = serviceProvider.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                _logger = loggerFactory.CreateLogger<ErrorResponseProvider>();
            }
        }

        public IActionResult CreateResponse(ErrorResponseContext context)
        { // DONE
            var problemDetails = ProblemDetailsResponse.Create(
                context.Request.Method,
                context.Request.Path,
                (int)HttpStatusCode.NotFound
            );

            _logger.LogDebug(
                $"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > Unknown error"
            );

            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status,
                ContentTypes = new MediaTypeCollection()
                {
                    ExtendedMediaTypeNames.ProblemApplication.JSON
                },
            };
        }
    }
}