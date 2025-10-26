using System;
using System.IO;
using SQLConnection.Data;
using SQLConnection.Models;
using Xunit;

namespace SQLConnection.Tests
{
    public class ContactsRepositoryCsvTests
    {
        private string GetInMemoryConnectionString() => "Data Source=file:memdbcsv?mode=memory&cache=shared";

        [Fact]
        public void ImportExportCsv_Succeeds()
        {
            var cs = GetInMemoryConnectionString();
            var repo = new ContactsRepository(cs);
            repo.EnsureSchema();

            var temp = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(temp, new[] { "Alice,alice@example.com,1234567", "Bob,bob@example.com,7654321" });
                var inserted = repo.ImportFromCsv(temp);
                Assert.Equal(2, inserted);

                var exportFile = Path.GetTempFileName();
                repo.ExportToCsv(exportFile);
                var lines = File.ReadAllLines(exportFile);
                Assert.True(lines.Length >= 2);
            }
            finally
            {
                File.Delete(temp);
            }
        }
    }
}
