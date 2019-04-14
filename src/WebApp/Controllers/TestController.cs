using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TestController : Controller
    {
        private readonly IRequestData requestData;

        public TestController(IRequestData requestData)
        {
            this.requestData = requestData;
        }

        [HttpGet]
        public ActionResult Get()
        {
            var model = new ModelClass();
            return Ok(new
            {
                requestData.Something,
                Model = model.RequestData.Something
            });
        }
    }
}