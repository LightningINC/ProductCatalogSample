using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogDemo.API.Models
{
	public class FiledProductTemplate
	{
		public long Id { get; set; }
		public string Name { get; set; }

		public virtual List<FiledCommerceProduct> CommerceProducts { get; }

		public FiledProductTemplate()
		{
			this.CommerceProducts = new List<FiledCommerceProduct>();
		}

	}

}
