using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Windows.Storage.Pickers;
using Windows.Storage;

namespace InstallerStudio.Providers
{
    public static class FileProvider
    {
        public static bool IsExtensionSupported(string extension)
        {
            return Constants.SetupExtensions
                .Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

        public static FileOpenPicker GetFileOpenPicker(params string[] filters)
        {
            var picker = new FileOpenPicker(App.MainWindow.AppWindow.Id)
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Desktop,
            };

            foreach (var filter in filters)
            {
                picker.FileTypeFilter.Add(filter);
            }

            return picker;
        }

        public static FileSavePicker GetFileSavePicker(string suggestedFileName)
        {
            var picker = new FileSavePicker(App.MainWindow.AppWindow.Id)
            {
                SuggestedFileName = suggestedFileName,
                SuggestedStartLocation = PickerLocationId.Desktop,
            };

            picker.FileTypeChoices.Add("Setup", [".exe"]);

            return picker;
        }

        public static FolderPicker GetFolderPicker()
        {
            var picker = new FolderPicker(App.MainWindow.AppWindow.Id)
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Desktop,
            };

            return picker;
        }

        public static async Task<StorageFile> WriteLinesTemporaryAsync(string fileName, IEnumerable<string> lines)
        {
            var temporaryFolder = ApplicationData.Current.TemporaryFolder;
            var scriptFile = await temporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteLinesAsync(scriptFile, lines);

            return scriptFile;
        }

        public static async Task CopyFileAsync(StorageFile file, StorageFolder destination)
        {
            await file.CopyAsync(destination, file.Name, NameCollisionOption.ReplaceExisting);
        }

        public static async Task CopyFolderAsync(StorageFolder source, StorageFolder destination)
        {
            var files = await source.GetFilesAsync();

            foreach (var file in files)
            {
                await file.CopyAsync(destination);
            }

            var folders = await source.GetFoldersAsync();

            foreach (var folder in folders)
            {
                var newDestination = await destination.CreateFolderAsync(folder.Name);
                await CopyFolderAsync(folder, newDestination);
            }
        }

        public static void OpenDirectorySelectFile(string filePath)
        {
            var info = new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = $"/select,\"{filePath}\"",
            };

            Process.Start(info);
        }
    }
}
