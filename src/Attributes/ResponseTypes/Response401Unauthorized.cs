using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers;

namespace PowerUtils.AspNetCore.WebAPI.Attributes.ResponseTypes
{
    public class Response401Unauthorized : ProducesResponseTypeAttribute
    {
        public Response401Unauthorized() : base(typeof(ProblemDetailsResponse), StatusCodes.Status401Unauthorized) { }
    }
}