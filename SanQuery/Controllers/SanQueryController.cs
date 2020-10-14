using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanQuery.Utils;
using SanQuery.ViewModels;

namespace SanQuery.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SanQuery : ControllerBase
    {


        private readonly ILogger<SanQuery> _logger;

        public SanQuery(ILogger<SanQuery> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Get([FromBody] RequestModel requestModel)
        {
            try
            {
                var result = await QueryUtils.ConsultGenerator(requestModel);
                return await Task.FromResult(StatusCode(200, result));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
