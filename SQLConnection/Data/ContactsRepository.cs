using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using SQLConnection.Models;

namespace SQLConnection.Data
{
    public class ContactsRepository
    {
        private readonly string _connectionString;

        public ContactsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void EnsureSchema()
        {
            using var con = new SQLiteConnection(_connectionString);
            con.Open();
            using var cmd = new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS contacts(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Email TEXT,
                Mobile TEXT
            );", con);
            cmd.ExecuteNonQuery();
        }

        public int Insert(Contact contact)
        {
            // Check duplicate by Name+Email
            var existing = GetByNameEmail(contact.Name, contact.Email);
            if (existing != null)
            {
                return existing.Id; // return existing id, do not insert duplicate
            }

            using var con = new SQLiteConnection(_connectionString);
            con.Open();
            using var cmd = new SQLiteCommand("INSERT INTO contacts(Name, Email, Mobile) VALUES(@Name, @Email, @Mobile); SELECT last_insert_rowid();", con);
            cmd.Parameters.AddWithValue("@Name", contact.Name);
            cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(contact.Email) ? (object)DBNull.Value : contact.Email);
            cmd.Parameters.AddWithValue("@Mobile", string.IsNullOrEmpty(contact.Mobile) ? (object)DBNull.Value : contact.Mobile);
            var id = Convert.ToInt32(cmd.ExecuteScalar());
            return id;
        }

        public Contact GetByNameEmail(string name, string email)
        {
            using var con = new SQLiteConnection(_connectionString);
            con.Open();
            using var cmd = new SQLiteCommand("SELECT Id, Name, Email, Mobile FROM contacts WHERE Name = @Name AND (Email = @Email OR (@Email IS NULL AND Email IS NULL)) LIMIT 1", con);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return new Contact
                {
                    Id = dr.GetInt32(0),
                    Name = dr[1]?.ToString(),
                    Email = dr[2]?.ToString(),
                    Mobile = dr[3]?.ToString()
                };
            }
            return null;
        }

        public IEnumerable<Contact> GetAll()
        {
            var list = new List<Contact>();
            using var con = new SQLiteConnection(_connectionString);
            con.Open();
            using var cmd = new SQLiteCommand("SELECT Id, Name, Email, Mobile FROM contacts ORDER BY Id DESC", con);
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new Contact
                {
                    Id = dr.GetInt32(0),
                    Name = dr[1]?.ToString(),
                    Email = dr[2]?.ToString(),
                    Mobile = dr[3]?.ToString()
                });
            }
            return list;
        }

        public (IEnumerable<Contact> Items, int Total) GetPage(int pageNumber, int pageSize, string search = null)
        {
            // pageNumber is 1-based
            int offset = (pageNumber - 1) * pageSize;
            var items = new List<Contact>();
            using var con = new SQLiteConnection(_connectionString);
            con.Open();

            string where = string.Empty;
            if (!string.IsNullOrWhiteSpace(search))
            {
                where = "WHERE Name LIKE @q OR Email LIKE @q";
            }

            using (var countCmd = new SQLiteCommand($"SELECT COUNT(1) FROM contacts {where}", con))
            {
                if (!string.IsNullOrWhiteSpace(search)) countCmd.Parameters.AddWithValue("@q", "%" + search + "%");
                var total = Convert.ToInt32(countCmd.ExecuteScalar());

                using var cmd = new SQLiteCommand($"SELECT Id, Name, Email, Mobile FROM contacts {where} ORDER BY Id DESC LIMIT @limit OFFSET @offset", con);
                if (!string.IsNullOrWhiteSpace(search)) cmd.Parameters.AddWithValue("@q", "%" + search + "%");
                cmd.Parameters.AddWithValue("@limit", pageSize);
                cmd.Parameters.AddWithValue("@offset", offset);

                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    items.Add(new Contact
                    {
                        Id = dr.GetInt32(0),
                        Name = dr[1]?.ToString(),
                        Email = dr[2]?.ToString(),
                        Mobile = dr[3]?.ToString()
                    });
                }

                return (items, total);
            }
        }

        public void Update(Contact contact)
        {
            using var con = new SQLiteConnection(_connectionString);
            con.Open();
            using var cmd = new SQLiteCommand("UPDATE contacts SET Name=@Name, Email=@Email, Mobile=@Mobile WHERE Id=@Id", con);
            cmd.Parameters.AddWithValue("@Name", contact.Name);
            cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(contact.Email) ? (object)DBNull.Value : contact.Email);
            cmd.Parameters.AddWithValue("@Mobile", string.IsNullOrEmpty(contact.Mobile) ? (object)DBNull.Value : contact.Mobile);
            cmd.Parameters.AddWithValue("@Id", contact.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var con = new SQLiteConnection(_connectionString);
            con.Open();
            using var cmd = new SQLiteCommand("DELETE FROM contacts WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public int ImportFromCsv(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("CSV file not found", filePath);
            int inserted = 0;
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                // Expect CSV: Name,Email,Mobile
                var parts = SplitCsvLine(line);
                if (parts.Length < 1) continue;
                var name = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                var email = parts.Length > 1 ? parts[1].Trim() : null;
                var mobile = parts.Length > 2 ? parts[2].Trim() : null;
                if (string.IsNullOrEmpty(name)) continue;
                var existing = GetByNameEmail(name, email);
                if (existing != null) continue;
                Insert(new Contact { Name = name, Email = string.IsNullOrEmpty(email) ? null : email, Mobile = string.IsNullOrEmpty(mobile) ? null : mobile });
                inserted++;
            }
            return inserted;
        }

        public void ExportToCsv(string filePath)
        {
            var all = GetAll();
            using var sw = new StreamWriter(filePath, false, Encoding.UTF8);
            foreach (var c in all)
            {
                sw.WriteLine(EscapeCsv(c.Name) + "," + EscapeCsv(c.Email) + "," + EscapeCsv(c.Mobile));
            }
        }

        private static string[] SplitCsvLine(string line)
        {
            // Very simple CSV splitter handling quoted values
            var parts = new List<string>();
            bool inQuotes = false;
            var sb = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }
                if (ch == ',' && !inQuotes)
                {
                    parts.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }
                sb.Append(ch);
            }
            parts.Add(sb.ToString());
            return parts.ToArray();
        }

        private static string EscapeCsv(string value)
        {
            if (value == null) return string.Empty;
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}
