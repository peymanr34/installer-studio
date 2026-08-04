using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InstallerStudio.Data;
using InstallerStudio.Data.Models;
using InstallerStudio.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Media.Imaging;
using MvvmGen;
using Windows.Storage;
using Windows.System;

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
                .Select(x => ToViewModel(x, null))
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

        [Command(CanExecuteMethod = nameof(CanCreate))]
        public async Task Create()
        {
            var picker = FileProvider.GetFileOpenPicker(Constants.SetupExtensions);
            var files = await picker.PickMultipleFilesAsync();

            await CreateRange(files.Select(x => x.Path));
        }

        [Command(CanExecuteMethod = nameof(CanCreate))]
        public async Task CreateByFolder()
        {
            var picker = FileProvider.GetFolderPicker();
            var result = await picker.PickSingleFolderAsync();

            if (result is null)
            {
                return;
            }

            var folder = await StorageFolder.GetFolderFromPathAsync(result.Path);
            var files = await folder.GetFilesAsync();

            await CreateRange(files.Select(x => x.Path));
        }

        [Command(CanExecuteMethod = nameof(CanCreate))]
        public async Task CreateWithFolder()
        {
            var picker = FileProvider.GetFileOpenPicker(Constants.SetupExtensions);
            var result = await picker.PickSingleFileAsync();

            if (result is null || IsSetupAlreadyExists(result.Path))
            {
                return;
            }

            var setup = GetSetup(result.Path);

            var file = await StorageFile.GetFileFromPathAsync(setup.FilePath);
            var folder = await file.GetParentAsync();

            var items = await folder.GetItemsAsync();

            setup.Additionals ??= [];

            foreach (var item in items.Where(x => x.Path != setup.FilePath))
            {
                var additional = new SetupAdditional
                {
                    Path = item.Path,
                    IsDirectory = item.IsOfType(StorageItemTypes.Folder),
                };

                setup.Additionals.Add(additional);
            }

            Context.Setups.Add(setup);
            Context.SaveChanges();

            await PreviewSetup(setup);
        }

        [CommandInvalidate(nameof(IsExecuting))]
        public bool CanCreate()
        {
            return !IsExecuting;
        }

        public async Task CreateRange(IEnumerable<string> files)
        {
            IsExecuting = true;

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);

                if (!FileProvider.IsExtensionSupported(extension) || IsSetupAlreadyExists(file))
                {
                    continue;
                }

                var setup = GetSetup(file);

                Context.Setups.Add(setup);
                Context.SaveChanges();

                await PreviewSetup(setup);
            }

            IsExecuting = false;
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
        public async Task OpenFolder(object args)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(SelectedItem.FilePath);
                var folder = await file.GetParentAsync();

                await Launcher.LaunchFolderAsync(folder, new FolderLauncherOptions { ItemsToSelect = { file } });
            }
            catch (FileNotFoundException)
            {
                return;
            }
        }

        [CommandInvalidate(nameof(SelectedItem))]
        public bool CanOpenFolder()
        {
            return SelectedItem is not null;
        }

        private bool IsSetupAlreadyExists(string filePath)
        {
            var existing = Context.Setups
                .FirstOrDefault(x => x.ProjectId == ProjectId && x.FilePath == filePath);

            return existing is not null;
        }

        private Setup GetSetup(string filePath)
        {
            var fileName = Path.GetFileName(filePath);

            var setup = new Setup
            {
                Name = fileName,
                FilePath = filePath,
                ProjectId = ProjectId,
            };

            setup.IsX86 = SetupProvider.IsX86Setup(fileName);
            setup.IsX64 = SetupProvider.IsX64Setup(fileName);
            setup.IsArm64 = SetupProvider.IsArm64Setup(fileName);

            var setupType = SetupProvider.GetSetupType(filePath);
            setup.Arguments = SetupProvider.GetSilentSwitch(setupType);

            var info = SetupProvider.GetSetupInfo(filePath, setupType);

            if (!string.IsNullOrEmpty(info?.Name))
            {
                setup.Name = info.Name.Trim();
            }

            if (!string.IsNullOrEmpty(info?.Version))
            {
                setup.Version = info.Version.Trim();
            }

            if (!string.IsNullOrEmpty(info?.Description))
            {
                setup.Description = info.Description.Trim();
            }

            return setup;
        }

        private async Task PreviewSetup(Setup setup)
        {
            var icon = await CacheProvider.GetCachedIconOrDefaultAsync(setup.FilePath);
            var item = ToViewModel(setup, icon);

            Items.Add(item);
            OnPropertyChanged(nameof(Items));
        }

        private async void LoadIcons()
        {
            foreach (var item in Items)
            {
                item.Icon = await CacheProvider.GetCachedIconOrDefaultAsync(item.FilePath);
            }

            OnPropertyChanged(nameof(Items));
        }

        private static SetupViewModel ToViewModel(Setup model, BitmapImage icon) => new()
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
            Icon = icon,
        };
    }
}
