using System;
using System.Collections.ObjectModel;
using System.Linq;
using InstallerStudio.Data;
using InstallerStudio.Data.Models;
using MvvmGen;

namespace InstallerStudio.ViewModels
{
    [ViewModel]
    [Inject(typeof(DatabaseContext), PropertyName = "Context")]
    public partial class StartViewModel
    {
        [Property]
        private ObservableCollection<ProjectViewModel> _items;

        [Property]
        private ProjectViewModel _selectedItem;

        [Property]
        private string _newProjectName;

        public void Load()
        {
            var items = Context.Projects
                .OrderByDescending(x => x.ModifiedDateUtc)
                .Select(x => new ProjectViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Version = x.Version,
                    Website = x.Website,
                    UniqueId = x.UniqueId,
                    SetupType = x.SetupType,
                    Publisher = x.Publisher,
                    Description = x.Description,
                })
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

        [Command(CanExecuteMethod = nameof(CanCreate))]
        public void Create()
        {
            var project = new Project
            {
                Name = NewProjectName,
                Version = "1.0.0",
                UniqueId = Guid.NewGuid(),
                Publisher = Environment.UserName,
            };

            Context.Projects.Add(project);
            Context.SaveChanges();

            NewProjectName = string.Empty;

            var item = new ProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Version = project.Version,
                Website = project.Website,
                UniqueId = project.UniqueId,
                SetupType = project.SetupType,
                Publisher = project.Publisher,
                Description = project.Description,
            };

            Items.Add(item);
            OnPropertyChanged(nameof(Items));

            SelectedItem = item;
        }

        [CommandInvalidate(nameof(NewProjectName))]
        public bool CanCreate()
        {
            return !string.IsNullOrWhiteSpace(NewProjectName);
        }

        [Command(CanExecuteMethod = nameof(CanRemove))]
        public void Remove()
        {
            var item = Context.Projects
                .First(x => x.Id == SelectedItem.Id);

            Context.Projects.Remove(item);
            Context.SaveChanges();

            Items.Remove(SelectedItem);
            OnPropertyChanged(nameof(Items));
        }

        [CommandInvalidate(nameof(SelectedItem))]
        public bool CanRemove()
        {
            return SelectedItem is not null;
        }

        [Command(CanExecuteMethod = nameof(CanOpen))]
        public void Open()
        {
            // ...
        }

        [CommandInvalidate(nameof(SelectedItem))]
        public bool CanOpen()
        {
            return SelectedItem is not null;
        }
    }
}
