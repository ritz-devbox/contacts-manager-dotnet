using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SQLConnection.Data;
using SQLConnection.Models;

namespace SQLConnection
{
    public partial class Form1 : Form
    {
        private readonly string path;
        private readonly string cs;

        private ContactsRepository repository;

        // Track selected record Id
        private int? selectedId = null;

        // paging state
        private int currentPage = 1;
        private int pageSize = 10;

        public Form1()
        {
            InitializeComponent();
            path = Path.Combine(Application.StartupPath, "data.db");
            cs = "URI=file:" + path;
            this.Load += new EventHandler(Form1_Load);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            repository = new ContactsRepository(cs);
            repository.EnsureSchema();

            // Ensure columns are created once
            EnsureGridColumns();

            // Use paged loading instead of loading all rows
            currentPage = 1;
            RefreshPage();

            // Initialize UI state
            try
            {
                btn_update.Enabled = false;
                btn_delete.Enabled = false;
            }
            catch { }

            if (TryGetControl<TextBox>("txtSearch", out var searchBox))
            {
                // debounce search using a Timer
                var searchTimer = new System.Windows.Forms.Timer { Interval = 350 };
                searchTimer.Tick += (s, ev) =>
                {
                    searchTimer.Stop();
                    currentPage = 1;
                    RefreshPage();
                };

                searchBox.TextChanged += (s, ev) =>
                {
                    searchTimer.Stop();
                    searchTimer.Start();
                };
            }

            if (TryGetControl<Button>("btnPrev", out var prev))
            {
                prev.Click += (s, ev) => { if (currentPage > 1) { currentPage--; RefreshPage(); } };
            }

            if (TryGetControl<Button>("btnNext", out var next))
            {
                next.Click += (s, ev) => { currentPage++; RefreshPage(); };
            }
        }

        private bool TryGetControl<T>(string name, out T control) where T : Control
        {
            control = null;
            var matches = this.Controls.Find(name, true);
            if (matches != null && matches.Length > 0 && matches[0] is T c)
            {
                control = c;
                return true;
            }
            return false;
        }

        private void RefreshPage()
        {
            try
            {
                // If there is a search box use its trimmed text otherwise null
                string search = null;
                if (TryGetControl<TextBox>("txtSearch", out var sbox))
                {
                    search = string.IsNullOrWhiteSpace(sbox.Text) ? null : sbox.Text.Trim();
                }

                var (items, total) = repository.GetPage(currentPage, pageSize, search);

                // If currentPage is out of range, clamp and reload
                var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
                if (currentPage > totalPages)
                {
                    currentPage = totalPages;
                    (items, total) = repository.GetPage(currentPage, pageSize, search);
                }

                Contactsdatagridview.Rows.Clear();
                foreach (var c in items)
                {
                    Contactsdatagridview.Rows.Add(c.Id, c.Name, c.Email, c.Mobile);
                }

                // Update page info label if present
                if (TryGetControl<Label>("lblPageInfo", out var pageLabel))
                {
                    pageLabel.Text = $"Page {currentPage} of {totalPages}";
                }

                // Update prev/next buttons if present
                if (TryGetControl<Button>("btnPrev", out var prev)) prev.Enabled = currentPage > 1;
                if (TryGetControl<Button>("btnNext", out var next)) next.Enabled = currentPage < totalPages;
            }
            catch (Exception ex)
            {
                ShowStatus($"Error loading page: {ex.Message}", false);
            }
        }

        private void EnsureGridColumns()
        {
            if (Contactsdatagridview.Columns.Count == 0)
            {
                var colId = new DataGridViewTextBoxColumn();
                colId.Name = "Id";
                colId.HeaderText = "Id";
                colId.Visible = false; // hide internal Id
                Contactsdatagridview.Columns.Add(colId);

                Contactsdatagridview.Columns.Add("Name", "Name");
                Contactsdatagridview.Columns.Add("Email", "Email");
                Contactsdatagridview.Columns.Add("Mobile", "Mobile");
            }
        }

        private bool ValidateInputs()
        {
            // Name required
            var name = tb_name.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                ShowStatus("Name is required.", false);
                return false;
            }

            // Email optional but if present must be valid
            var email = tb_email.Text?.Trim();
            if (!string.IsNullOrEmpty(email) && !ValidationHelper.IsValidEmail(email))
            {
                ShowStatus("Email is not in a valid format.", false);
                return false;
            }

            // Mobile optional but if present must be digits and reasonable length
            var mobile = tb_mobile.Text?.Trim();
            if (!string.IsNullOrEmpty(mobile) && !ValidationHelper.IsValidMobile(mobile))
            {
                ShowStatus("Mobile must be digits (7-15 characters).", false);
                return false;
            }

            // All good
            return true;
        }

        private void ShowStatus(string message, bool success)
        {
            labelStatus.Text = message;
            labelStatus.ForeColor = success ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                var name = tb_name.Text.Trim();
                var email = string.IsNullOrWhiteSpace(tb_email.Text) ? null : tb_email.Text.Trim();
                var mobile = string.IsNullOrWhiteSpace(tb_mobile.Text) ? null : tb_mobile.Text.Trim();

                // Check duplicate explicitly
                var existing = repository.GetByNameEmail(name, email);
                if (existing != null)
                {
                    ShowStatus("Contact already exists.", false);
                    return;
                }

                var contact = new Contact
                {
                    Name = name,
                    Email = email,
                    Mobile = mobile
                };

                var id = repository.Insert(contact);
                if (id > 0)
                {
                    ShowStatus("Contact saved successfully.", true);
                }
                else
                {
                    ShowStatus("Contact was not saved.", false);
                }

                // Refresh grid after successful insert
                RefreshPage();
                ClearSelection();
            }
            catch (Exception ex)
            {
                ShowStatus($"Cannot insert: {ex.Message}", false);
            }
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            if (selectedId == null)
            {
                ShowStatus("Please select a contact to update.", false);
                return;
            }

            if (!ValidateInputs()) return;

            try
            {
                var contact = new Contact
                {
                    Id = selectedId.Value,
                    Name = tb_name.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(tb_email.Text) ? null : tb_email.Text.Trim(),
                    Mobile = string.IsNullOrWhiteSpace(tb_mobile.Text) ? null : tb_mobile.Text.Trim()
                };

                repository.Update(contact);

                RefreshPage();
                ClearSelection();
                ShowStatus("Contact updated successfully.", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Cannot update: {ex.Message}", false);
            }
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            if (selectedId == null)
            {
                ShowStatus("Please select a contact to delete.", false);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete the selected contact?", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                repository.Delete(selectedId.Value);

                RefreshPage();
                ClearSelection();
                ShowStatus("Contact deleted.", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Cannot delete row: {ex.Message}", false);
            }
        }

        private void Contactsdatagridview_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // kept for compatibility with earlier event wiring
            Contactsdatagridview_CellClick(sender, e);
        }

        private void Contactsdatagridview_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.RowIndex < Contactsdatagridview.Rows.Count)
                {
                    var row = Contactsdatagridview.Rows[e.RowIndex];
                    var idCell = row.Cells["Id"];
                    if (idCell?.Value != null && int.TryParse(idCell.Value.ToString(), out var id))
                    {
                        selectedId = id;
                        tb_name.Text = row.Cells["Name"].FormattedValue?.ToString() ?? string.Empty;
                        tb_email.Text = row.Cells["Email"].FormattedValue?.ToString() ?? string.Empty;
                        tb_mobile.Text = row.Cells["Mobile"].FormattedValue?.ToString() ?? string.Empty;

                        btn_update.Enabled = true;
                        btn_delete.Enabled = true;

                        ShowStatus($"Selected: {tb_name.Text}", true);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"No content to display: {ex.Message}", false);
            }
        }

        private void ClearSelection()
        {
            selectedId = null;
            tb_name.Clear();
            tb_email.Clear();
            tb_mobile.Clear();

            try
            {
                btn_update.Enabled = false;
                btn_delete.Enabled = false;
            }
            catch { }
        }
    }
}
