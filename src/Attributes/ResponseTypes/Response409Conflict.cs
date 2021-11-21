using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers;

namespace PowerUtils.AspNetCore.WebAPI.Attributes.ResponseTypes
{
    public class Response409Conflict : ProducesResponseTypeAttribute
    {
        public Response409Conflict() : base(typeof(ProblemDetailsResponse), StatusCodes.Status409Conflict) { }
    }
}