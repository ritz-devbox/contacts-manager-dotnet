using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using SQLConnection.Data;

namespace SQLConnection.UI
{
    public partial class ImportExportDialog : Form
    {
        private readonly ContactsRepository _repo;
        private readonly ILogger<ImportExportDialog>? _logger;
        private CancellationTokenSource _cts;
        private DateTime _startTime;

        public ImportExportDialog(ContactsRepository repo, ILogger<ImportExportDialog> logger)
        {
            _repo = repo;
            _logger = logger;
            InitializeComponent();
        }

        // Back-compat ctor
        public ImportExportDialog(ContactsRepository repo) : this(repo, null)
        {
        }

        private async void btnImport_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "CSV files|*.csv|All files|*.*" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            progressBar.Value = 0;
            _cts = new CancellationTokenSource();

            string[] lines;
            try
            {
                lines = System.IO.File.ReadAllLines(ofd.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot read file: " + ex.Message);
                _logger?.LogError(ex, "Failed reading import file");
                return;
            }

            int totalLines = lines.Length;
            _startTime = DateTime.UtcNow;

            var progress = new Progress<int>(v =>
            {
                // Update UI with processed lines and ETA
                var elapsed = DateTime.UtcNow - _startTime;
                var rate = v / Math.Max(1, elapsed.TotalSeconds);
                var remaining = Math.Max(0, totalLines - v);
                var eta = TimeSpan.Zero;
                if (rate > 0) eta = TimeSpan.FromSeconds(remaining / rate);
                lblStatus.Text = $"Processed {v}/{totalLines} ({(int)(v * 100.0 / totalLines)}%) - ETA {eta.ToString(@"mm\:ss")}";
                progressBar.Value = Math.Min(progressBar.Maximum, (int)(v * 100.0 / totalLines));
            });

            try
            {
                var inserted = 0;
                await Task.Run(() =>
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (_cts.IsCancellationRequested) _cts.Token.ThrowIfCancellationRequested();
                        var line = lines[i];
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            ((IProgress<int>)progress).Report(i + 1);
                            continue;
                        }
                        var parts = _repo.ParseCsvLine(line);
                        if (parts.Length < 1)
                        {
                            ((IProgress<int>)progress).Report(i + 1);
                            continue;
                        }
                        var name = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                        var email = parts.Length > 1 ? parts[1].Trim() : null;
                        var mobile = parts.Length > 2 ? parts[2].Trim() : null;
                        if (string.IsNullOrEmpty(name))
                        {
                            ((IProgress<int>)progress).Report(i + 1);
                            continue;
                        }
                        var existing = _repo.GetByNameEmail(name, email);
                        if (existing != null)
                        {
                            ((IProgress<int>)progress).Report(i + 1);
                            continue;
                        }
                        _repo.Insert(new Models.Contact { Name = name, Email = string.IsNullOrEmpty(email) ? null : email, Mobile = string.IsNullOrEmpty(mobile) ? null : mobile });
                        inserted++;
                        ((IProgress<int>)progress).Report(i + 1);
                    }
                }, _cts.Token);

                MessageBox.Show($"Imported {inserted} contacts.");
                _logger?.LogInformation($"User imported {inserted} contacts from {ofd.FileName}");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Import cancelled.");
                _logger?.LogWarning("Import cancelled by user.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Import failed: " + ex.Message);
                _logger?.LogError(ex, "Import failed.");
            }
            finally
            {
                progressBar.Value = 0;
                _cts = null;
            }
        }

        private async void btnExport_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Filter = "CSV files|*.csv|All files|*.*", FileName = "contacts_export.csv" };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            progressBar.Value = 0;
            var lines = _repo.GetAll().ToList();
            var total = lines.Count;
            _startTime = DateTime.UtcNow;
            var progress = new Progress<int>(v =>
            {
                var elapsed = DateTime.UtcNow - _startTime;
                var rate = v / Math.Max(1, elapsed.TotalSeconds);
                var remaining = Math.Max(0, total - v);
                var eta = TimeSpan.Zero;
                if (rate > 0) eta = TimeSpan.FromSeconds(remaining / rate);
                lblStatus.Text = $"Wrote {v}/{total} ({(int)(v * 100.0 / total)}%) - ETA {eta.ToString(@"mm\:ss")}";
                progressBar.Value = Math.Min(progressBar.Maximum, (int)(v * 100.0 / total));
            });

            try
            {
                await _repo.ExportToCsvAsync(sfd.FileName, progress);
                MessageBox.Show("Export completed.");
                _logger?.LogInformation($"User exported contacts to {sfd.FileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message);
                _logger?.LogError(ex, "Export failed.");
            }
            finally
            {
                progressBar.Value = 0;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }
    }
}
