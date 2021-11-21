using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers;
using PowerUtils.Net.Constants;
using PowerUtils.Validations;
using System.Linq;
using Xunit;

namespace PowerUtils.AspNetCore.WebAPI.Tests.ProblemDetails
{
    public class ProblemDetailsResponseTests
    {
        [Theory]
        [InlineData(400, StatusCodeLink.BAD_REQUEST)]
        [InlineData(401, StatusCodeLink.UNAUTHORIZED)]
        [InlineData(402, StatusCodeLink.PAYMENT_REQUIRED)]
        [InlineData(403, StatusCodeLink.FORBIDDEN)]
        [InlineData(404, StatusCodeLink.NOT_FOUND)]
        [InlineData(405, StatusCodeLink.METHOD_NOT_ALLOWED)]
        [InlineData(406, StatusCodeLink.NOT_ACCEPTABLE)]
        [InlineData(407, StatusCodeLink.PROXY_AUTHENTICATION_REQUIRED)]
        [InlineData(408, StatusCodeLink.REQUEST_TIMEOUT)]
        [InlineData(409, StatusCodeLink.CONFLICT)]
        [InlineData(410, StatusCodeLink.GONE)]
        [InlineData(411, StatusCodeLink.LENGTH_REQUIRED)]
        [InlineData(412, StatusCodeLink.PRECONDITION_FAILED)]
        [InlineData(413, StatusCodeLink.REQUEST_ENTITY_TOO_LARGE)]
        [InlineData(414, StatusCodeLink.REQUEST_URI_TOO_LONG)]
        [InlineData(415, StatusCodeLink.UNSUPPORTED_MEDIA_TYPE)]
        [InlineData(416, StatusCodeLink.REQUESTED_RANGE_NOT_SATISFIABLE)]
        [InlineData(417, StatusCodeLink.EXPECTATION_FAILED)]
        [InlineData(426, StatusCodeLink.UPGRADE_REQUIRED)]
        [InlineData(500, StatusCodeLink.INTERNAL_SERVER_ERROR)]
        [InlineData(501, StatusCodeLink.NOT_IMPLEMENTED)]
        [InlineData(502, StatusCodeLink.BAD_GATEWAY)]
        [InlineData(503, StatusCodeLink.SERVICE_UNAVAILABLE)]
        [InlineData(504, StatusCodeLink.GATEWAY_TIMEOUT)]
        [InlineData(505, StatusCodeLink.HTTP_VERSION_NOT_SUPPORTED)]
        public void HttpStatusCodes(int statusCode, string statusCodeLink)
        {
            // Arrange & Act
            var act = ProblemDetailsResponse.Create(statusCode);


            // Assert
            act.Status.Should().Be(statusCode);

            act.Errors.Should().BeEmpty();
            act.TraceID.Should().NotBeNullOrWhiteSpace();

            act.Title.Should().Be(ReasonPhrases.GetReasonPhrase(statusCode));

            act.Type.Should().Be(statusCodeLink);
        }


        [Fact]
        public void CustomMessage()
        {
            // Arrange
            int statusCode = 400;
            string errorMessage = "invalid request with this operation";
            int expectedStatusCode = 400;
            string type = StatusCodeLink.BAD_REQUEST;


            // Act
            ProblemDetailsResponse act = ProblemDetailsResponse.Create(statusCode, errorMessage);


            // Assert
            act.Status.Should().Be(expectedStatusCode);

            act.Errors.Should().BeEmpty();
            act.TraceID.Should().NotBeNullOrWhiteSpace();

            act.Title.Should().Be(errorMessage);

            act.Type.Should().Be(type);
        }


        [Fact]
        public void HttpContext_500()
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.StatusCode = 500;
            string expectedErrorMessage = "Internal Server Error";
            int expectedStatusCode = 500;
            string type = StatusCodeLink.INTERNAL_SERVER_ERROR;


            // Arrange
            ProblemDetailsResponse act = ProblemDetailsResponse.Create(httpContext);


            // Assert
            act.Status.Should().Be(expectedStatusCode);

            act.Errors.Should().BeEmpty();
            act.TraceID.Should().NotBeNullOrWhiteSpace();

            act.Title.Should().Be(expectedErrorMessage);

            act.Type.Should().Be(type);
        }

        [Fact]
        public void ModelState_NULL()
        {
            // Arrange
            HttpContext httpContext = null;
            ModelStateDictionary modelState = null;
            string expectedErrorMessage = "Bad Request";
            int expectedStatusCode = 400;
            string type = StatusCodeLink.BAD_REQUEST;


            // Act
            ProblemDetailsResponse act = ProblemDetailsResponse.Create(httpContext, modelState);


            // Assert
            act.Status.Should().Be(expectedStatusCode);

            act.Errors.Should().BeEmpty();
            act.TraceID.Should().NotBeNullOrWhiteSpace();

            act.Title.Should().Be(expectedErrorMessage);

            act.Type.Should().Be(type);
        }



        [Fact]
        public void ActionContext_InternalError()
        {
            // Arrange
            var actionContext = new ActionContext();
            var expectedErrorMessage = "Internal Server Error";
            int expectedStatusCode = 500;
            var type = StatusCodeLink.INTERNAL_SERVER_ERROR;


            // Act
            ProblemDetailsResponse act = ProblemDetailsResponse.Create(actionContext);


            // Assert
            act.Status.Should().Be(expectedStatusCode);

            act.Errors.Should().BeEmpty();
            act.TraceID.Should().NotBeNullOrWhiteSpace();

            act.Title.Should().Be(expectedErrorMessage);

            act.Type.Should().Be(type);
        }


        [Fact]
        public void ActionContext_WithModelStates()
        {
            // Arrange
            var actionContext = new ActionContext();
            var expectedErrorMessage = "Bad Request";
            int expectedStatusCode = 400;
            actionContext.HttpContext = new DefaultHttpContext();
            actionContext.HttpContext.Response.StatusCode = 400;
            var type = StatusCodeLink.BAD_REQUEST;
            actionContext.ModelState.AddModelError("Field", ErrorCodes.INVALID);


            // Act
            ProblemDetailsResponse act = ProblemDetailsResponse.Create(actionContext);


            // Assert
            act.Status.Should().Be(expectedStatusCode);

            act.Errors.Should().NotBeEmpty();
            act.TraceID.Should().NotBeNullOrWhiteSpace();

            act.Title.Should().Be(expectedErrorMessage);

            act.Type.Should().Be(type);
        }

        [Fact]
        public void ActionContext_Create()
        {
            // Arrange
            int expectedStatusCode = 404;
            string expectedInstance = "GET: users/photos";
            string expectedErrorMessage = "Not Found";
            string type = StatusCodeLink.NOT_FOUND;


            // Act
            ProblemDetailsResponse act = ProblemDetailsResponse.Create("get", "users/photos", 404);


            // Assert
            act.Status.Should().Be(expectedStatusCode);

            act.Errors.Should().BeEmpty();
            act.TraceID.Should().NotBeNullOrWhiteSpace();

            act.Instance.Should().Be(expectedInstance);

            act.Title.Should().Be(expectedErrorMessage);

            act.Type.Should().Be(type);
        }

        [Fact]
        public void From_ValidationNotificationsPipeline()
        {
            // Arrange
            var actionContext = new ActionContext();

            var validationNotificationsPipeline = new ValidationNotificationsPipeline();

            int expectedStatusCode = 400;
            var expectedErrorMessage = "Bad Request";
            var expectedType = StatusCodeLink.BAD_REQUEST;

            int expectedTotalErrors = 1;
            var expectedProperty = "FakeProp";
            var expectedErrorCode = "FakeCode";

            validationNotificationsPipeline.AddBadNotification(
                new ValidationNotification(expectedProperty, expectedErrorCode)
            );


            // Act
            var act = ProblemDetailsResponse.Create(
                actionContext,
                validationNotificationsPipeline
            );


            // Assert
            act.Status.Should().Be(expectedStatusCode);

            act.Errors.Count.Should().Be(expectedTotalErrors);
            act.Errors.First().Key
                .Should().Be(expectedProperty);
            act.Errors.First().Value
                .Should().Be(expectedErrorCode);

            act.TraceID.Should().NotBeNullOrWhiteSpace();

            act.Title.Should().Be(expectedErrorMessage);

            act.Type.Should().Be(expectedType);
        }
    }
}