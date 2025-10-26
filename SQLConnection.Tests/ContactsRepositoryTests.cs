using System;
using Microsoft.Data.Sqlite;
using SQLConnection.Data;
using SQLConnection.Models;
using Xunit;
using System.IO;

namespace SQLConnection.Tests
{
    public class ContactsRepositoryTests
    {
        private string GetInMemoryConnectionString() => "Data Source=file:memdb?mode=memory&cache=shared";

        [Fact]
        public void InsertAndReadUsingRepository_Succeeds()
        {
            var cs = GetInMemoryConnectionString();
            var repo = new ContactsRepository(cs);
            repo.EnsureSchema();

            var contact = new Contact { Name = "RepoAlice", Email = "repoalice@example.com", Mobile = "1234567890" };
            var id = repo.Insert(contact);
            Assert.True(id > 0);

            var all = repo.GetAll();
            Assert.Contains(all, c => c.Name == "RepoAlice" && c.Email == "repoalice@example.com");
        }

        [Fact]
        public void UpdateAndDeleteUsingRepository_Succeeds()
        {
            var cs = GetInMemoryConnectionString();
            var repo = new ContactsRepository(cs);
            repo.EnsureSchema();

            var contact = new Contact { Name = "RepoBob", Email = "repobob@example.com", Mobile = "9876543210" };
            var id = repo.Insert(contact);

            contact.Id = id;
            contact.Email = "repobob.new@example.com";
            repo.Update(contact);

            var all = repo.GetAll();
            Assert.Contains(all, c => c.Id == id && c.Email == "repobob.new@example.com");

            repo.Delete(id);
            all = repo.GetAll();
            Assert.DoesNotContain(all, c => c.Id == id);
        }
    }
}
