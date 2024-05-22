using System;
using InstallerStudio.Data.Models;
using MvvmGen;

namespace InstallerStudio.ViewModels
{
    [ViewModel]
    public partial class ProjectViewModel
    {
        [Property]
        private int _id;

        [Property]
        private string _name;

        [Property]
        private string _description;

        [Property]
        private string _publisher;

        [Property]
        private string _website;

        [Property]
        private string _version;

        [Property]
        private Guid _uniqueId;

        [Property]
        private SetupType _setupType;
    }
}
