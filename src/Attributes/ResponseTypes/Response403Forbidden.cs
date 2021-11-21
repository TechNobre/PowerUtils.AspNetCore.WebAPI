using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers;

namespace PowerUtils.AspNetCore.WebAPI.Attributes.ResponseTypes
{
    public class Response403Forbidden : ProducesResponseTypeAttribute
    {
        public Response403Forbidden() : base(typeof(ProblemDetailsResponse), StatusCodes.Status403Forbidden) { }
    }
}