﻿using System;
using System.Linq;
using System.Threading.Tasks;
using InstallerStudio.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace InstallerStudio
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        public static MainWindow MainWindow { get; private set; }

        public static DatabaseContext Context { get; private set; }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Context = await GetDatabaseContextAsync();

            var pending = await Context.Database.GetPendingMigrationsAsync();

            if (pending.Any())
            {
                await Context.Database.MigrateAsync();
            }

            MainWindow = new MainWindow();
            MainWindow.Activate();
        }

        private static async Task<DatabaseContext> GetDatabaseContextAsync()
        {
            var file = await ApplicationData.Current.LocalFolder
                .CreateFileAsync("data.db", CreationCollisionOption.OpenIfExists);

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlite($"Data Source={file.Path}");

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
