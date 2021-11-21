using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerUtils.Net.Constants;
using System.Net;

namespace PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers
{
    public static class ModelStateHandlerMiddleware
    {
        public static IServiceCollection AddModelStateResponse(this IServiceCollection services)
        { // DONE
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            ILoggerFactory loggerFactory = serviceProvider.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            ILogger logger = loggerFactory.CreateLogger("ModelStateHandler");

            ProblemFactory factory = serviceProvider.GetService(typeof(ProblemFactory)) as ProblemFactory;

            return services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    actionContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    ProblemDetailsResponse problemDetails = ProblemDetailsResponse.Create(actionContext);

                    logger.LogDebug(
                        $"[REQUEST: {problemDetails.Instance} | STATUS CODE: {problemDetails.Status}] > {problemDetails}"
                    );

                    factory.ClearResponse(actionContext.HttpContext, actionContext.HttpContext.Response.StatusCode);
                    actionContext.HttpContext.Response.ContentType = ExtendedMediaTypeNames.ProblemApplication.JSON;

                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { ExtendedMediaTypeNames.ProblemApplication.JSON }
                    };
                };
            });
        }
    }
}