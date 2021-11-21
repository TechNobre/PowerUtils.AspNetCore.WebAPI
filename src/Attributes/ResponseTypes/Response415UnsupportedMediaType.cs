using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers;

namespace PowerUtils.AspNetCore.WebAPI.Attributes.ResponseTypes
{
    public class Response415UnsupportedMediaType : ProducesResponseTypeAttribute
    {
        public Response415UnsupportedMediaType() : base(typeof(ProblemDetailsResponse), StatusCodes.Status415UnsupportedMediaType) { }
    }
}