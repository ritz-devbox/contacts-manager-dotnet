using System;
using System.Collections.Generic;
using System.Linq;
using SQLConnection.Models;

namespace SQLConnection.UI
{
    public static class SearchFilter
    {
        public static IEnumerable<Contact> Filter(IEnumerable<Contact> source, string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return source;
            query = query.Trim().ToLowerInvariant();
            return source.Where(c => (c.Name ?? string.Empty).ToLowerInvariant().Contains(query)
                || (c.Email ?? string.Empty).ToLowerInvariant().Contains(query));
        }
    }
}
