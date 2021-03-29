using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogDemo.API.Helper
{
    public class FiledCsvHelper : IDisposable
    {
        public StreamReader fileStream;
        public string[] Headers;
        public List<string[]> BodyList;
        public char separator;
        public FiledCsvHelper(StreamReader stream, char separator)
        {
            this.fileStream = stream;
            this.separator = separator;
            Task.Run(async () =>  await this.ReadHeaders() ).Wait();
        }

        public async Task ReadHeaders()
        {
            CsvParser parser = new CsvParser(fileStream, CultureInfo.InvariantCulture);
            await parser.ReadAsync();
            this.Headers = parser.Record;

            while (await parser.ReadAsync())
            {
                var currentRow = parser.Record;
                BodyList.Add(currentRow);
            }
            
        }


        public void Dispose()
        {
            fileStream.Dispose();
        }
    }
    
}
