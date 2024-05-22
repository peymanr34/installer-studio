using MvvmGen;

namespace InstallerStudio.ViewModels
{
    [ViewModel]
    public partial class AdditionalViewModel
    {
        [Property]
        private int _id;

        [Property]
        private string _path;

        [Property]
        private bool _isDirectory;
    }
}
