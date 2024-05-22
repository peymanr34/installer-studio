using Microsoft.UI.Xaml.Media.Imaging;
using MvvmGen;

namespace InstallerStudio.ViewModels
{
    [ViewModel]
    public partial class SetupViewModel
    {
        [Property]
        private int _id;

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
        private BitmapImage _icon;

        public bool HasArguments
            => !string.IsNullOrWhiteSpace(Arguments);
    }
}
