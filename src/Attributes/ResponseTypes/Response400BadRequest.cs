using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers;

namespace PowerUtils.AspNetCore.WebAPI.Attributes.ResponseTypes
{
    public class Response400BadRequest : ProducesResponseTypeAttribute
    {
        public Response400BadRequest() : base(typeof(ProblemDetailsResponse), StatusCodes.Status400BadRequest) { }
    }
}