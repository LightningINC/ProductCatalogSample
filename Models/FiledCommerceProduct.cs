using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogDemo.API.Models
{
    public class FiledCommerceProduct
    {
		public long Id { get; set; }

		public string Currency { get; set; }


		public string Name { get; set; }

		public string Sku { get; set; }

		public string Barcode { get; set; }

		public long InventoryQuantity { get; set; }

		public decimal Price { get; set; }

		public decimal CompareAtPrice { get; set; }

		public string Url { get; set; }

		public string ImageUrl { get; set; }

		public bool Availability { get; set; }

		public string CustomProperties { get; set; }

		public string Tags { get; set; }

		public string ShortDescription { get; set; }

	}
    public enum Currencies
	{
		USD = 0, CAD = 1, POUNDS =2
	}
}
