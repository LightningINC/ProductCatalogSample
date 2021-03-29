using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductCatalogDemo.API.Services;

namespace ProductCatalogDemo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCatalogController : ControllerBase
    {
        private readonly ILogger<ProductCatalogController> logger;
        private readonly IProductCatalogingService catalogService; 

        public ProductCatalogController(ILogger<ProductCatalogController> logger, IProductCatalogingService productCatalogingService)
        {
            this.logger = logger;
            this.catalogService = productCatalogingService;
        }

        [Route("upload")]
        public async Task<IActionResult> Upload(IFormFile formFile)
        {
            if (formFile == null) return BadRequest("Please supply a catalog file");

            var csvExtension = ".csv";
            var fileExt = Path.GetExtension(formFile.FileName);
            if (!fileExt.Equals(csvExtension, StringComparison.OrdinalIgnoreCase)) return BadRequest("File format not supported, please supply a csv file");

            try
            {
                var response = await catalogService.ProcessCatalogFile(formFile);
                return Ok(response);
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.ToString());
            }
        }
    }
}
