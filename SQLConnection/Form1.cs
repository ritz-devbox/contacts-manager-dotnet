using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SQLConnection.Data;
using SQLConnection.Models;

namespace SQLConnection
{
    public partial class Form1 : Form
    {
        string path = Application.StartupPath + "\\data.db";
        string cs = @"URI=file:" + Application.StartupPath + "\\data.db";

        private ContactsRepository repository;

        // Track selected record Id
        private int? selectedId = null;

        // paging state
        private int currentPage = 1;
        private int pageSize = 10;

        public Form1()
        {
            InitializeComponent();
            this.Load += new EventHandler(Form1_Load);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            repository = new ContactsRepository(cs);
            repository.EnsureSchema();

            LoadContactsDataGridView();

            // Initialize UI state
            btn_update.Enabled = false;
            btn_delete.Enabled = false;
            labelStatus.Text = string.Empty;

            // wire search/paging
            txtSearch.TextChanged += (s, ev) => { currentPage = 1; RefreshPage(); };
            btnPrev.Click += (s, ev) => { if (currentPage > 1) { currentPage--; RefreshPage(); } };
            btnNext.Click += (s, ev) => { currentPage++; RefreshPage(); };
        }

        private void RefreshPage()
        {
            var (items, total) = repository.GetPage(currentPage, pageSize, txtSearch.Text);
            Contactsdatagridview.Rows.Clear();
            foreach (var c in items)
            {
                Contactsdatagridview.Rows.Add(c.Id, c.Name, c.Email, c.Mobile);
            }

            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            lblPageInfo.Text = $"Page {currentPage} of {Math.Max(1, totalPages)}";
        }

        private void LoadContactsDataGridView()
        {
            try
            {
                Contactsdatagridview.Rows.Clear();

                // Ensure columns are set up
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

                foreach (var c in repository.GetAll())
                {
                    Contactsdatagridview.Rows.Add(c.Id, c.Name, c.Email, c.Mobile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading data: {ex.Message}");
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
                var contact = new Contact
                {
                    Name = tb_name.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(tb_email.Text) ? null : tb_email.Text.Trim(),
                    Mobile = string.IsNullOrWhiteSpace(tb_mobile.Text) ? null : tb_mobile.Text.Trim()
                };

                var id = repository.Insert(contact);
                if (id > 0)
                {
                    ShowStatus("Contact saved successfully.", true);
                }
                else
                {
                    ShowStatus("Contact already exists.", false);
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
                if (e.RowIndex >= 0)
                {
                    var row = Contactsdatagridview.Rows[e.RowIndex];
                    if (row.Cells["Id"].Value != null)
                    {
                        selectedId = Convert.ToInt32(row.Cells["Id"].Value);
                        tb_name.Text = row.Cells["Name"].FormattedValue.ToString();
                        tb_email.Text = row.Cells["Email"].FormattedValue.ToString();
                        tb_mobile.Text = row.Cells["Mobile"].FormattedValue.ToString();

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

            btn_update.Enabled = false;
            btn_delete.Enabled = false;
        }
    }
}
