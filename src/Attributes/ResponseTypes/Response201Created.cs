using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace PowerUtils.AspNetCore.WebAPI.Attributes.ResponseTypes
{
    public class Response201Created : ProducesResponseTypeAttribute
    {
        public Response201Created(Type type) : base(type, StatusCodes.Status201Created) { }
    }
}