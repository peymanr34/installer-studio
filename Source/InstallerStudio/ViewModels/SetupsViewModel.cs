using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InstallerStudio.Data;
using InstallerStudio.Data.Models;
using InstallerStudio.Providers;
using Microsoft.EntityFrameworkCore;
using MvvmGen;
using Windows.Storage;

namespace InstallerStudio.ViewModels
{
    [ViewModel]
    [Inject(typeof(int), PropertyName = "ProjectId")]
    [Inject(typeof(DatabaseContext), PropertyName = "Context")]
    public partial class SetupsViewModel
    {
        [Property]
        private ObservableCollection<SetupViewModel> _items;

        [Property]
        private SetupViewModel _selectedItem;

        [Property]
        private bool _isExecuting;

        public void Load(string search = null)
        {
            var query = Context.Setups
                .Where(x => x.ProjectId == ProjectId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => EF.Functions.Like(x.Name, $"%{search}%"));
            }

            var items = query
                .Select(x => ToViewModel(x))
                .AsNoTracking()
                .ToList();

            Items ??= [];
            Items.Clear();

            foreach (var item in items)
            {
                Items.Add(item);
            }

            SelectedItem ??= Items.FirstOrDefault();
            OnPropertyChanged(nameof(Items));

            LoadIcons();
        }

        [Command]
        public async Task Create()
        {
            var picker = FileProvider.GetFileOpenPicker(Constants.SetupExtensions);
            var files = await picker.PickMultipleFilesAsync();

            IsExecuting = true;

            foreach (var file in files)
            {
                var storageFile = await StorageFile.GetFileFromPathAsync(file.Path);
                await CreateSetup(storageFile);
            }

            IsExecuting = false;
        }

        public async Task Create(IEnumerable<StorageFile> files)
        {
            IsExecuting = true;

            foreach (var file in files)
            {
                if (FileProvider.IsExtensionSupported(file.FileType))
                {
                    await CreateSetup(file);
                }
            }

            IsExecuting = false;
        }

        private async Task CreateSetup(StorageFile file)
        {
            var existing = Context.Setups
                .FirstOrDefault(x => x.ProjectId == ProjectId && x.FilePath == file.Path);

            if (existing is not null)
            {
                return;
            }

            var setup = new Setup
            {
                Name = file.Name,
                FilePath = file.Path,
                ProjectId = ProjectId,
            };

            setup.IsX86 = SetupProvider.IsX86Setup(file.Name);
            setup.IsX64 = SetupProvider.IsX64Setup(file.Name);
            setup.IsArm64 = SetupProvider.IsArm64Setup(file.Name);

            var setupType = SetupProvider.GetSetupType(file.Path);
            setup.Arguments = SetupProvider.GetSilentSwitch(setupType);

            if (setup.Arguments is null &&
                file.FileType.Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                setup.Arguments = "/S";
            }

            if (setupType != SetupProvider.SetupType.Msi)
            {
                var info = FileVersionInfo.GetVersionInfo(file.Path);

                if (!string.IsNullOrEmpty(info.ProductName))
                {
                    setup.Name = info.ProductName.Trim();
                }

                if (!string.IsNullOrEmpty(info.ProductVersion))
                {
                    setup.Version = info.ProductVersion.Trim();
                }

                if (!string.IsNullOrEmpty(info.FileDescription))
                {
                    setup.Description = info.FileDescription.Trim();
                }
            }

            Context.Setups.Add(setup);
            Context.SaveChanges();

            var item = ToViewModel(setup);
            item.Icon = await CacheProvider.GetCachedIconOrDefaultAsync(item.FilePath);

            Items.Add(item);
            OnPropertyChanged(nameof(Items));
        }

        [Command(CanExecuteMethod = nameof(CanRemove))]
        public void Remove(object args)
        {
            var item = Context.Setups
                .First(x => x.Id == SelectedItem.Id);

            Context.Setups.Remove(item);
            Context.SaveChanges();

            Items.Remove(SelectedItem);
            OnPropertyChanged(nameof(Items));
        }

        [CommandInvalidate(nameof(SelectedItem))]
        public bool CanRemove()
        {
            return SelectedItem is not null;
        }

        [Command(CanExecuteMethod = nameof(CanEdit))]
        public void Edit(object args)
        {
            // ...
        }

        [CommandInvalidate(nameof(SelectedItem))]
        public bool CanEdit()
        {
            return SelectedItem is not null;
        }

        [Command(CanExecuteMethod = nameof(CanOpenFolder))]
        public void OpenFolder(object args)
        {
            if (!File.Exists(SelectedItem.FilePath))
            {
                return;
            }

            FileProvider.OpenDirectorySelectFile(SelectedItem.FilePath);
        }

        [CommandInvalidate(nameof(SelectedItem))]
        public bool CanOpenFolder()
        {
            return SelectedItem is not null;
        }

        private async void LoadIcons()
        {
            foreach (var item in Items)
            {
                item.Icon = await CacheProvider.GetCachedIconOrDefaultAsync(item.FilePath);
            }

            OnPropertyChanged(nameof(Items));
        }

        private static SetupViewModel ToViewModel(Setup model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            IsX86 = model.IsX86,
            IsX64 = model.IsX64,
            IsArm64 = model.IsArm64,
            Version = model.Version,
            FilePath = model.FilePath,
            Arguments = model.Arguments,
            Description = model.Description,
        };
    }
}
