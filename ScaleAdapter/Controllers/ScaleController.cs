using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScaleAdapter.Models;
using ScaleAdapter.Services;

namespace ScaleAdapter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScaleController : ControllerBase
    {
        private readonly ILogger<ScaleController> _logger;
        private readonly IScaleService _scaleService;

        public ScaleController(ILogger<ScaleController> logger, IScaleService scaleService)
        {
            _logger = logger;
            _scaleService = scaleService;
        }

        [HttpGet]
        [Route("get-values/{id}")]
        public async Task<ScaleResponse> GetValue(int id)
        {
            return await _scaleService.GetScaleValues(id);
        }
    }
}
