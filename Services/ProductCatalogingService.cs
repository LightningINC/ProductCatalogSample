using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductCatalogDemo.API.Helper;
using ProductCatalogDemo.API.Models;

namespace ProductCatalogDemo.API.Services
{
    public class ProductCatalogingService : IProductCatalogingService
    {
        private ConcurrentDictionary<long, FiledProductTemplate> filedProductTemplates { get; set; } = new ConcurrentDictionary<long, FiledProductTemplate>();
        private readonly ILogger<ProductCatalogingService> logger;

        public ProductCatalogingService(ILogger<ProductCatalogingService> logger)
        {
            this.logger = logger;
        }

        public async Task<IEnumerable<FiledProductTemplate>> ProcessCatalogFile(IFormFile formFile)
        {
            
            using (var reader = new StreamReader(formFile.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                await csv.ReadAsync();
                csv.ReadHeader();

                var filedProductTemplatePropertyNames = typeof(FiledProductTemplate)
                    .GetProperties()
                    .Select(prop => prop.Name);


                var filedCommerceProductPropertyNames = typeof(FiledCommerceProduct)
                    .GetProperties()
                    .Select(prop => prop.Name);

                var filedProductTemplateMap = new Dictionary<string, int>();
                var commerceProductMap = new Dictionary<string, int>();
                var customCommerceProductMap = new Dictionary<string, int>();

                for (int i = 0; i < csv.HeaderRecord.Length; i++)
                {
                    var header = csv.HeaderRecord[i];
                    if (filedProductTemplatePropertyNames.Contains(header)) filedProductTemplateMap.Add(header, i);

                    if (filedCommerceProductPropertyNames.Contains(header)) {
                        commerceProductMap.Add(header, i);
                    }
                    else { customCommerceProductMap.Add(header, i); }
                }

                List<Task> taskList = new List<Task>();
                int maxConcurrency = 100;
                using (SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrency))
                {
                    while (await csv.ReadAsync())
                    {
                        var row = csv.Parser.Record;
                        if (row == null) continue;
                        semaphore.Wait();
                        var task = Task.Run(() =>
                        {
                            try
                            {
                                ProductCatalogMapper(row, filedProductTemplateMap, commerceProductMap, customCommerceProductMap);
                            }
                            catch
                            {
                                this.logger.LogInformation($"Error processing line {csv.Parser.Row}");
                                throw;
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        });
                        
                        taskList.Add(task);
                    }
                    Task.WaitAll(taskList.ToArray());
                }
            }
            return this.filedProductTemplates.Values.AsEnumerable();
        }

        public void ProductCatalogMapper(
            string[] currentFields, 
            Dictionary<string, int> productTemplateMap, 
            Dictionary<string,int> commerceProductMap,
            Dictionary<string,int> customCommerceProductMap )
            
        {
            var commerceProduct = new FiledCommerceProduct()
            {
                Id = Convert.ToInt64(this.ResolveField(currentFields,commerceProductMap, "Id")),
                Name = this.ResolveField(currentFields, commerceProductMap, "Name"),
                Currency = this.ResolveField(currentFields, commerceProductMap, "Currency"),
                Sku = this.ResolveField(currentFields, commerceProductMap, "Sku"),
                Barcode = this.ResolveField(currentFields, commerceProductMap, "Barcode"),
                InventoryQuantity = Convert.ToInt64(this.ResolveField(currentFields, commerceProductMap, "InventoryQuantity")),
                Price = Convert.ToDecimal(this.ResolveField(currentFields, commerceProductMap, "InventoryQuantity")),
                CompareAtPrice = Convert.ToDecimal(this.ResolveField(currentFields, commerceProductMap, "CompareAtPrice")),
                Url = this.ResolveField(currentFields, commerceProductMap, "Url"),
                ImageUrl = this.ResolveField(currentFields, commerceProductMap, "ImageUrl"),
                Availability = Convert.ToBoolean(this.ResolveField(currentFields, commerceProductMap, "Availability")),
                Tags = this.ResolveField(currentFields, commerceProductMap, "Tags"),
                ShortDescription = this.ResolveField(currentFields, commerceProductMap, "ShortDescription"),
                CustomProperties = this.GetCustomProperties(currentFields, customCommerceProductMap)
            };

            

            var productTemplate = new FiledProductTemplate()
            {
                Id = Convert.ToInt64(currentFields[productTemplateMap["Id"]]),
                Name = currentFields[productTemplateMap["Name"]]
            };
            productTemplate.CommerceProducts.Add(commerceProduct);
            var isAdded = this.filedProductTemplates.TryAdd(commerceProduct.Id, productTemplate);

            if (!isAdded && this.filedProductTemplates.ContainsKey(commerceProduct.Id))
            {
                var retrieved = false;
                var tries = 0;
                while (!retrieved)
                {
                    retrieved = this.filedProductTemplates.TryGetValue(commerceProduct.Id, out FiledProductTemplate retrievedProductTemplate);
                    if (retrieved)
                    {
                        var updatedProductTemplate = retrievedProductTemplate;
                        updatedProductTemplate.CommerceProducts.Add(commerceProduct);
                        this.filedProductTemplates.TryUpdate(commerceProduct.Id, updatedProductTemplate, retrievedProductTemplate);
                    };
                    tries++;
                }
                if (!retrieved) this.logger.LogInformation($"Unable to add product {JsonConvert.SerializeObject(commerceProduct)}");
            }


        }

        private string ResolveField(string[] currentFields, Dictionary<string, int> commerceProductMap, string field)
        {
            if(commerceProductMap.TryGetValue(field, out int value)) return currentFields[value];
            return null;
        }

        private string GetCustomProperties(string[] currentFields, Dictionary<string, int> customPropertyMap)
        {
            var customProperties = new Dictionary<string, string>();
            foreach(var currentProperty in customPropertyMap)
            {
                customProperties.Add(currentProperty.Key, currentFields[currentProperty.Value]);
            }

            return JsonConvert.SerializeObject(customProperties);
        }
    }
}