using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.EventStream;
using InstallerStudio.Data;
using InstallerStudio.Data.Models;
using InstallerStudio.Providers;
using InstallerStudio.Providers.InnoSetup;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using MvvmGen;
using Windows.Storage;

namespace InstallerStudio.ViewModels
{
    [ViewModel]
    [Inject(typeof(int), PropertyName = "ProjectId")]
    [Inject(typeof(DatabaseContext), PropertyName = "Context")]
    public partial class PublishViewModel
    {
        private readonly StringBuilder _outputBuilder = new();

        [Property]
        private bool _openDirectoryOnFinished;

        [Property]
        private bool _isExecuting;

        public string Output
            => _outputBuilder.ToString();

        [Command(CanExecuteMethod = nameof(CanExecute))]
        public async Task Publish(object args)
        {
            var xamlRoot = (args as Button).XamlRoot;

            var project = Context.Projects
                .Include(x => x.Setups)
                .ThenInclude(x => x.Additionals)
                .First(x => x.Id == ProjectId);

            // Get the compiler.
            var required = new Version(6, 3, 0);
            var compiler = InnoProvider.GetCompiler();

            if (!File.Exists(compiler?.Path) || compiler.Version < required)
            {
                var dialog = new ContentDialog
                {
                    Title = "Inno Setup 6.3+ is required",
                    Content = "To publish the setup, you need to download and install Inno Setup separately. Would you like to download it now?",
                    DefaultButton = ContentDialogButton.Primary,
                    PrimaryButtonText = "Download",
                    CloseButtonText = "Cancel",
                    XamlRoot = xamlRoot,
                };

                if (compiler?.Version is not null)
                {
                    dialog.Content += $"\n\nInstalled: {compiler.Version}";
                }

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://jrsoftware.org/download.php/is.exe",
                        UseShellExecute = true,
                    });
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
                var success = await PublishAsync(project, file.Path, compiler.Path);

                if (success && OpenDirectoryOnFinished)
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
        public bool CanExecute()
            => !IsExecuting;

        private async Task<bool> PublishAsync(Project project, string filePath, string compiler)
        {
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            var directory = await file.GetParentAsync();

            // Create and save the script to a temporary file.
            var script = InnoCreator.CreateSetupScript(project);
            var scriptLines = InnoProvider.GetSetupScript(script);
            var scriptFile = await FileProvider.WriteLinesTemporaryAsync("script.iss", scriptLines);

#if DEBUG
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = scriptFile.Path,
            });
#endif
            var errorBuilder = new StringBuilder();

            // Compile the script.
            var command = Cli.Wrap(compiler)
                .WithWorkingDirectory(directory.Path)
                .WithValidation(CommandResultValidation.None)
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(errorBuilder))
                .WithArguments([scriptFile.Path, $"/O{directory.Path}", $"/F{file.DisplayName}"]);

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
    }
}
