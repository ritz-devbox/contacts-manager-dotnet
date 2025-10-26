using System;
using System.Collections.Generic;
using System.Linq;
using SQLConnection.Models;
using SQLConnection.UI;
using Xunit;

namespace SQLConnection.Tests
{
    public class SearchFilterTests
    {
        [Fact]
        public void Filter_FindsByNameOrEmail()
        {
            var items = new List<Contact>
            {
                new Contact { Id = 1, Name = "Alice Wonderland", Email = "alice@example.com" },
                new Contact { Id = 2, Name = "Bob Builder", Email = "bob@builder.com" },
            };

            var res = SearchFilter.Filter(items, "alice").ToList();
            Assert.Single(res);
            Assert.Equal(1, res[0].Id);

            var res2 = SearchFilter.Filter(items, "builder").ToList();
            Assert.Single(res2);
            Assert.Equal(2, res2[0].Id);
        }
    }
}
