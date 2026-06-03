using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.EventStream;
using InstallerStudio.Data;
using InstallerStudio.Data.Models;
using InstallerStudio.Models;
using InstallerStudio.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using MvvmGen;
using Windows.Storage;
using Windows.System;

namespace InstallerStudio.ViewModels
{
    [ViewModel]
    [Inject(typeof(int), PropertyName = "ProjectId")]
    [Inject(typeof(DatabaseContext), PropertyName = "Context")]
    public partial class PublishViewModel
    {
        private readonly StringBuilder _outputBuilder = new();

        [Property]
        private ObservableCollection<CompilerInfo> _compilers;

        [Property]
        private CompilerInfo _selectedCompiler;

        [Property]
        [PropertyCallMethod(nameof(SaveSettings))]
        private bool _openFolderOnPublished;

        [Property]
        private bool _isExecuting;

        public string Output
            => _outputBuilder.ToString();

        public void Load()
        {
            _compilers = new(CompilerProvider.GetSupportedCompilers());
            _selectedCompiler = _compilers.First();

            LoadSettings();
        }

        [Command(CanExecuteMethod = nameof(CanExecute))]
        public async Task Publish(object args)
        {
            var xamlRoot = (args as Button).XamlRoot;

            var project = Context.Projects
                .Include(x => x.Setups)
                .ThenInclude(x => x.Additionals)
                .First(x => x.Id == ProjectId);

            // Get the compiler.
            var compilers = CompilerProvider.GetInstalledCompilers(SelectedCompiler.CompilerType);
            var compiler = compilers.FirstOrDefault();

            if (!File.Exists(compiler?.Path) || compiler.Version < SelectedCompiler.SupportedVersion)
            {
                var dialog = new ContentDialog
                {
                    Title = $"{SelectedCompiler.Name} {SelectedCompiler.SupportedVersion.ToString(2)} or later is required",
                    Content = $"To publish the setup, you need to download and install {SelectedCompiler.Name} separately. Would you like to download it now?",
                    DefaultButton = ContentDialogButton.Primary,
                    PrimaryButtonText = "Download",
                    CloseButtonText = "Cancel",
                    XamlRoot = xamlRoot,
                };

                if (compiler?.Version is not null)
                {
                    dialog.Content += $"\n\nInstalled: {compiler.Version}";
                }

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    await Launcher.LaunchUriAsync(new Uri(SelectedCompiler.DownloadUrl));
                }

                return;
            }

            _outputBuilder.Clear();

            // Check for missing files and folders.
            var errors = GetValidationErrors(project);

            if (errors.Count != 0)
            {
                foreach (var error in errors)
                {
                    UpdateOutput($"- {error}");
                }

                UpdateOutput($"\nFailed.");
                return;
            }

            // Get the destination file.
            var picker = FileProvider.GetFileSavePicker($"Setup-v{project.Version}");
            var file = await picker.PickSaveFileAsync();

            if (file is null)
            {
                return;
            }

            IsExecuting = true;

            try
            {
                var storageFile = await StorageFile.GetFileFromPathAsync(file.Path);
                var storageFolder = await storageFile.GetParentAsync();

                var success = await PublishAsync(project, storageFile, storageFolder, SelectedCompiler, compiler.Path);

                if (success && OpenFolderOnPublished)
                {
                    FileProvider.OpenDirectorySelectFile(file.Path);
                }

                UpdateOutput($"\n{(success ? "Finished." : "Failed.")}");
            }
            catch (Exception ex)
            {
                UpdateOutput("Publish failed:");
                UpdateOutput(ex.Message);
            }
            finally
            {
                IsExecuting = false;
            }
        }

        [CommandInvalidate(nameof(IsExecuting))]
        [CommandInvalidate(nameof(SelectedCompiler))]
        public bool CanExecute()
            => !IsExecuting && SelectedCompiler is not null;

        private async Task<bool> PublishAsync(Project project, StorageFile file, StorageFolder directory, CompilerInfo compilerInfo, string compilerPath)
        {
            // Create and save the script to a temporary file.
            var script = SetupCreator.CreateScript(project, compilerInfo.CompilerType);
            var scriptFile = await ApplicationData.Current.TemporaryFolder
                .CreateFileAsync($"script{compilerInfo.ScriptFileExtension}", CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(scriptFile, script);

#if DEBUG
            await Launcher.LaunchFileAsync(scriptFile);
#endif
            var errorBuilder = new StringBuilder();

            // Compile the script.
            var arguments = CompilerProvider.GetArgumentsForScript(scriptFile, file, directory, compilerInfo.CompilerType);

            var command = Cli.Wrap(compilerPath)
                .WithWorkingDirectory(directory.Path)
                .WithValidation(CommandResultValidation.None)
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(errorBuilder))
                .WithArguments(arguments);

            await foreach (var commandEvent in command.ListenAsync())
            {
                if (commandEvent is StandardOutputCommandEvent standardOut)
                {
                    UpdateOutput(standardOut.Text);
                }
                else if (commandEvent is StandardErrorCommandEvent standardError)
                {
                    UpdateOutput($"\n{standardError.Text}");
                }
            }

            if (project.SetupType == SetupType.External)
            {
                await CopyExternalFilesAsync(project, directory);
            }

            return errorBuilder.Length == 0;
        }

        private async Task CopyExternalFilesAsync(Project project, StorageFolder directory)
        {
            var external = await directory.CreateFolderAsync("Files", CreationCollisionOption.OpenIfExists);

            UpdateOutput($"\nCopying external files to: {external.Path}");

            foreach (var item in project.Setups)
            {
                UpdateOutput($"- Copying {item.FilePath}");

                var setupFolder = await external.CreateFolderAsync(item.GetIdentifier());
                var setupFile = await StorageFile.GetFileFromPathAsync(item.FilePath);

                await FileProvider.CopyFileAsync(setupFile, setupFolder);

                foreach (var additional in item.Additionals)
                {
                    UpdateOutput($"  - Copying {additional.Path}");

                    if (additional.IsDirectory)
                    {
                        var src = await StorageFolder.GetFolderFromPathAsync(additional.Path);
                        var dest = await setupFolder.CreateFolderAsync(src.Name);

                        await FileProvider.CopyFolderAsync(src, dest);
                    }
                    else
                    {
                        var src = await StorageFile.GetFileFromPathAsync(additional.Path);
                        await FileProvider.CopyFileAsync(src, setupFolder);
                    }
                }
            }
        }

        private static List<string> GetValidationErrors(Project project)
        {
            var errors = new List<string>();

            foreach (var setup in project.Setups)
            {
                if (!File.Exists(setup.FilePath))
                {
                    errors.Add($"Setup file for '{setup.Name}' does not exists.");
                }

                foreach (var additional in setup.Additionals)
                {
                    if (additional.IsDirectory)
                    {
                        if (!Directory.Exists(additional.Path))
                        {
                            errors.Add($"Additional folder for '{setup.Name}' does not exists.");
                        }
                    }
                    else
                    {
                        if (!File.Exists(additional.Path))
                        {
                            errors.Add($"Additional file for '{setup.Name}' does not exists.");
                        }
                    }
                }
            }

            return errors;
        }

        private void UpdateOutput(string value)
        {
            _outputBuilder.AppendLine(value);
            OnPropertyChanged(nameof(Output));
        }

        private void LoadSettings()
        {
            // Set via the fields to avoid invoking the save method.
            _openFolderOnPublished = App.Settings.GetValue(SettingsKeys.OpenFolderOnPublished, false);

            OnPropertyChanged(nameof(OpenFolderOnPublished));
        }

        private void SaveSettings()
        {
            App.Settings.SetValue(SettingsKeys.OpenFolderOnPublished, OpenFolderOnPublished);
        }
    }
}
