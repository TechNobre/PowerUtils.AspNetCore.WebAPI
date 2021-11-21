using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace PowerUtils.AspNetCore.WebAPI.Attributes.ResponseTypes
{
    public class Response200OK : ProducesResponseTypeAttribute
    {
        public Response200OK(Type type) : base(type, StatusCodes.Status200OK) { }
    }
}