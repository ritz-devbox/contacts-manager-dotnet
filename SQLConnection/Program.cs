using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SQLConnection.Data;

namespace SQLConnection
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ContactsRepository>(sp => new ContactsRepository($"URI=file:{System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.db")}", sp.GetRequiredService<ILogger<ContactsRepository>>()));
                    services.AddSingleton<Form1>();
                })
                .Build();

            var form = host.Services.GetRequiredService<Form1>();
            Application.Run(form);
        }
    }
}