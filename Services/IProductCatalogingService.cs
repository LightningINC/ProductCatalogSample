using Microsoft.AspNetCore.Http;
using ProductCatalogDemo.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogDemo.API.Services
{
    public interface IProductCatalogingService
    {
        public Task<IEnumerable<FiledProductTemplate>> ProcessCatalogFile(IFormFile formFile);
    }
}
