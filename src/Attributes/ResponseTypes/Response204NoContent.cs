using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PowerUtils.AspNetCore.WebAPI.Attributes.ResponseTypes
{
    public class Response204NoContent : ProducesResponseTypeAttribute
    {
        public Response204NoContent() : base(StatusCodes.Status204NoContent) { }
    }
}