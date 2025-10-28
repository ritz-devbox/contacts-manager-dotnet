using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SQLConnection.Data;
using Xunit;

namespace SQLConnection.Tests
{
 public class ContactsRepositoryAsyncTests
 {
 private string GetInMemoryConnectionString() => "Data Source=file:memdbasync?mode=memory&cache=shared";

 [Fact]
 public async Task ImportFromCsvAsync_Works()
 {
 var cs = GetInMemoryConnectionString();
 var repo = new ContactsRepository(cs, NullLogger<ContactsRepository>.Instance);
 repo.EnsureSchema();

 var temp = Path.GetTempFileName();
 try
 {
 File.WriteAllLines(temp, new[] { "Alice,alice@example.com,1234567", "Bob,bob@example.com,7654321" });
 var progress = new Progress<int>();
 var inserted = await repo.ImportFromCsvAsync(temp, progress);
 Assert.Equal(2, inserted);
 }
 finally
 {
 File.Delete(temp);
 }
 }

 [Fact]
 public async Task ExportToCsvAsync_Works()
 {
 var cs = GetInMemoryConnectionString();
 var repo = new ContactsRepository(cs, NullLogger<ContactsRepository>.Instance);
 repo.EnsureSchema();

 // Insert some data
 repo.Insert(new Models.Contact { Name = "Zed", Email = "zed@example.com", Mobile = "555" });

 var temp = Path.GetTempFileName();
 try
 {
 var progress = new Progress<int>();
 await repo.ExportToCsvAsync(temp, progress);
 var lines = File.ReadAllLines(temp);
 Assert.True(lines.Length >=1);
 }
 finally
 {
 File.Delete(temp);
 }
 }
 }
}
