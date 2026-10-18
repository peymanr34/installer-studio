using System.Linq;
using InstallerStudio.Data;
using InstallerStudio.Data.Models;
using MvvmGen;

namespace InstallerStudio.ViewModels
{
    [ViewModel]
    [Inject(typeof(DatabaseContext), PropertyName = "Context")]
    public partial class EditProjectViewModel
    {
        [Property]
        private string _name;

        [Property]
        private string _version;

        [Property]
        private string _website;

        [Property]
        private string _publisher;

        [Property]
        private string _description;

        [Property]
        private SetupType _setupType;

        [Property]
        private Project _project;

        [Property]
        private bool _isSaved;

        public void Load(int projectId)
        {
            Project = Context.Projects
                .First(x => x.Id == projectId);

            Name = Project.Name;
            Version = Project.Version;
            Website = Project.Website;
            SetupType = Project.SetupType;
            Publisher = Project.Publisher;
            Description = Project.Description;
        }

        [Command(CanExecuteMethod = nameof(CanSave))]
        public void Save(object args)
        {
            var model = Context.Projects
                .First(x => x.Id == Project.Id);

            model.Name = Name;
            model.Version = Version;
            model.Website = Website;
            model.SetupType = SetupType;
            model.Publisher = Publisher;
            model.Description = Description;

            Context.SaveChanges();
            IsSaved = true;
        }

        [CommandInvalidate(nameof(Name))]
        [CommandInvalidate(nameof(Version))]
        public bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                !string.IsNullOrWhiteSpace(Version);
        }
    }
}
