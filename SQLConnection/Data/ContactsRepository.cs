using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SQLConnection.Models;

namespace SQLConnection.Data
{
    public class ContactsRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ContactsRepository>? _logger;

        public ContactsRepository(string connectionString, ILogger<ContactsRepository>? logger = null)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public void EnsureSchema()
        {
            using var con = new SQLiteConnection(_connectionString);
            con.Open();

            using var tx = con.BeginTransaction();
            try
            {
                // Check if table exists
                using var checkCmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='contacts';", con, tx);
                var exists = checkCmd.ExecuteScalar() != null;

                if (!exists)
                {
                    using var cmd = new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS contacts(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Email TEXT,
                        Mobile TEXT
                    );", con, tx);
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                    _logger?.LogInformation("Created contacts table.");
                    return;
                }

                // Table exists - ensure it has Id column. If not, migrate data into new table with Id.
                var columns = new List<string>();
                using (var colCmd = new SQLiteCommand("PRAGMA table_info(contacts);", con, tx))
                using (var dr = colCmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        columns.Add(dr["name"]?.ToString());
                    }
                }

                if (!columns.Any(c => string.Equals(c, "Id", StringComparison.OrdinalIgnoreCase)))
                {
                    // Migrate: create new table, copy data, drop old, rename
                    using var createCmd = new SQLiteCommand(@"CREATE TABLE contacts_new(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Email TEXT,
                        Mobile TEXT
                    );", con, tx);
                    createCmd.ExecuteNonQuery();

                    // Copy data from old table. Attempt to map Name, Email, Mobile if they exist.
                    var copyCols = new List<string>();
                    if (columns.Any(c => string.Equals(c, "Name", StringComparison.OrdinalIgnoreCase))) copyCols.Add("Name");
                    if (columns.Any(c => string.Equals(c, "Email", StringComparison.OrdinalIgnoreCase))) copyCols.Add("Email");
                    if (columns.Any(c => string.Equals(c, "Mobile", StringComparison.OrdinalIgnoreCase))) copyCols.Add("Mobile");

                    var selectCols = copyCols.Count > 0 ? string.Join(",", copyCols) : string.Empty;
                    var insertCols = selectCols; // same order

                    if (!string.IsNullOrEmpty(selectCols))
                    {
                        using var copyCmd = new SQLiteCommand($"INSERT INTO contacts_new({insertCols}) SELECT {selectCols} FROM contacts;", con, tx);
                        copyCmd.ExecuteNonQuery();
                    }

                    using var dropCmd = new SQLiteCommand("DROP TABLE contacts;", con, tx);
                    dropCmd.ExecuteNonQuery();

                    using var renameCmd = new SQLiteCommand("ALTER TABLE contacts_new RENAME TO contacts;", con, tx);
                    renameCmd.ExecuteNonQuery();

                    _logger?.LogInformation("Migrated legacy contacts table to include Id column.");
                }

                tx.Commit();
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                _logger?.LogError(ex, "EnsureSchema failed");
                throw;
            }
        }

        public int Insert(Contact contact)
        {
            // Check duplicate by Name+Email
            var existing = GetByNameEmail(contact.Name, contact.Email);
            if (existing != null)
            {
                _logger?.LogInformation($"Insert skipped - duplicate found for {contact.Name} / {contact.Email}");
                return existing.Id; // return existing id, do not insert duplicate
            }

            using var con = new SQLiteConnection(_connectionString);
            con.Open();
            using var cmd = new SQLiteCommand("INSERT INTO contacts(Name, Email, Mobile) VALUES(@Name, @Email, @Mobile); SELECT last_insert_rowid();", con);
            cmd.Parameters.AddWithValue("@Name", contact.Name);
            cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(contact.Email) ? (object)DBNull.Value : contact.Email);
            cmd.Parameters.AddWithValue("@Mobile", string.IsNullOrEmpty(contact.Mobile) ? (object)DBNull.Value : contact.Mobile);
            var id = Convert.ToInt32(cmd.ExecuteScalar());
            _logger?.LogInformation($"Inserted contact {contact.Name} with Id {id}");
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
            _logger?.LogInformation($"Updated contact Id={contact.Id}");
        }

        public void Delete(int id)
        {
            using var con = new SQLiteConnection(_connectionString);
            con.Open();
            using var cmd = new SQLiteCommand("DELETE FROM contacts WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            _logger?.LogInformation($"Deleted contact Id={id}");
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
            _logger?.LogInformation($"Imported {inserted} contacts from {filePath}");
            return inserted;
        }

        public async Task<int> ImportFromCsvAsync(string filePath, IProgress<int> progress = null)
        {
            return await Task.Run(() =>
            {
                int inserted = 0;
                if (!File.Exists(filePath)) throw new FileNotFoundException("CSV file not found", filePath);
                var lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;
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
                    progress?.Report(i + 1);
                }
                _logger?.LogInformation($"Imported {inserted} contacts from {filePath} (async)");
                return inserted;
            });
        }

        public void ExportToCsv(string filePath)
        {
            var all = GetAll();
            using var sw = new StreamWriter(filePath, false, Encoding.UTF8);
            foreach (var c in all)
            {
                sw.WriteLine(EscapeCsv(c.Name) + "," + EscapeCsv(c.Email) + "," + EscapeCsv(c.Mobile));
            }
            _logger?.LogInformation($"Exported {all.Count()} contacts to {filePath}");
        }

        public async Task ExportToCsvAsync(string filePath, IProgress<int> progress = null)
        {
            await Task.Run(() =>
            {
                var all = GetAll().ToList();
                using var sw = new StreamWriter(filePath, false, Encoding.UTF8);
                for (int i = 0; i < all.Count; i++)
                {
                    var c = all[i];
                    sw.WriteLine(EscapeCsv(c.Name) + "," + EscapeCsv(c.Email) + "," + EscapeCsv(c.Mobile));
                    progress?.Report(i + 1);
                }
                _logger?.LogInformation($"Exported {all.Count} contacts to {filePath} (async)");
            });
        }

        // Provide reusable CSV parsing for UI
        public string[] ParseCsvLine(string line) => SplitCsvLine(line);

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
