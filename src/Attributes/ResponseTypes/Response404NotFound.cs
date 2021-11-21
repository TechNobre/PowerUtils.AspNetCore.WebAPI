using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers;

namespace PowerUtils.AspNetCore.WebAPI.Attributes.ResponseTypes
{
    public class Response404NotFound : ProducesResponseTypeAttribute
    {
        public Response404NotFound() : base(typeof(ProblemDetailsResponse), StatusCodes.Status404NotFound) { }
    }
}