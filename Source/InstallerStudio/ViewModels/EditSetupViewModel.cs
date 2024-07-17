using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using InstallerStudio.Data;
using InstallerStudio.Data.Models;
using InstallerStudio.Providers;
using Microsoft.EntityFrameworkCore;
using MvvmGen;

namespace InstallerStudio.ViewModels
{
    [ViewModel]
    [Inject(typeof(int), PropertyName = "SetupId")]
    [Inject(typeof(DatabaseContext), PropertyName = "Context")]
    public partial class EditSetupViewModel
    {
        [Property]
        private string _name;

        [Property]
        private string _version;

        [Property]
        private string _description;

        [Property]
        private string _filePath;

        [Property]
        private string _arguments;

        [Property]
        private bool _isX86;

        [Property]
        private bool _isX64;

        [Property]
        private bool _isArm64;

        [Property]
        private ObservableCollection<AdditionalViewModel> _items;

        [Property]
        private AdditionalViewModel _selectedItem;

        public void Load()
        {
            var model = Context.Setups
                .Include(x => x.Additionals)
                .First(x => x.Id == SetupId);

            Name = model.Name;
            Version = model.Version;
            Description = model.Description;
            FilePath = model.FilePath;
            Arguments = model.Arguments;
            IsX86 = model.IsX86;
            IsX64 = model.IsX64;
            IsArm64 = model.IsArm64;

            var items = model.Additionals
                .Select(x => ToViewModel(x))
                .OrderByDescending(x => x.IsDirectory)
                .ThenByDescending(x => x.Path)
                .ToList();

            Items ??= [];
            Items.Clear();

            foreach (var item in items)
            {
                Items.Add(item);
            }

            SelectedItem ??= Items.FirstOrDefault();
            OnPropertyChanged(nameof(Items));
        }

        [Command(CanExecuteMethod = nameof(CanSave))]
        public void Save(object args)
        {
            var model = Context.Setups
                .First(x => x.Id == SetupId);

            model.Name = Name;
            model.IsX86 = IsX86;
            model.IsX64 = IsX64;
            model.IsArm64 = IsArm64;
            model.Version = Version;
            model.FilePath = FilePath;
            model.Arguments = Arguments;
            model.Description = Description;

            Context.SaveChanges();
        }

        [CommandInvalidate(nameof(Name))]
        [CommandInvalidate(nameof(FilePath))]
        public bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                !string.IsNullOrWhiteSpace(FilePath);
        }

        [Command]
        public async Task Browse()
        {
            var picker = FileProvider.GetFileOpenPicker(Constants.SetupExtensions);
            var file = await picker.PickSingleFileAsync();

            if (file is not null)
            {
                FilePath = file.Path;
            }
        }

        [Command]
        public async void AddFile()
        {
            var picker = FileProvider.GetFileOpenPicker("*");
            var files = await picker.PickMultipleFilesAsync();

            foreach (var item in files)
            {
                AddAdditional(item.Path, false);
            }
        }

        [Command]
        public async void AddFolder()
        {
            var picker = FileProvider.GetFolderPicker();
            var folder = await picker.PickSingleFolderAsync();

            if (folder is not null)
            {
                AddAdditional(folder.Path, true);
            }
        }

        public void AddAdditional(string path, bool directory)
        {
            // Prevent adding the setup file by accident via additional files.
            if (FilePath.Equals(path, StringComparison.Ordinal))
            {
                return;
            }

            // Prevent adding duplicates.
            if (Items.Any(x => x.Path.Equals(path, StringComparison.Ordinal)))
            {
                return;
            }

            var model = new SetupAdditional
            {
                Path = path,
                SetupId = SetupId,
                IsDirectory = directory,
            };

            Context.Additionals.Add(model);
            Context.SaveChanges();

            Items.Add(ToViewModel(model));
        }

        [Command(CanExecuteMethod = nameof(CanRemoveAdditional))]
        public void RemoveAdditional(object args)
        {
            var item = Context.Additionals
                .First(x => x.Id == SelectedItem.Id);

            Context.Additionals.Remove(item);
            Context.SaveChanges();

            Items.Remove(SelectedItem);
        }

        [CommandInvalidate(nameof(SelectedItem))]
        public bool CanRemoveAdditional()
        {
            return SelectedItem is not null;
        }

        private static AdditionalViewModel ToViewModel(SetupAdditional model) => new()
        {
            Id = model.Id,
            Path = model.Path,
            IsDirectory = model.IsDirectory,
        };
    }
}
