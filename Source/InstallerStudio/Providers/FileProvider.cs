using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace InstallerStudio.Providers
{
    public static class FileProvider
    {
        public static FileOpenPicker GetFileOpenPicker(params string[] filters)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Desktop,
            };

            var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(picker, hWnd);

            foreach (var filter in filters)
            {
                picker.FileTypeFilter.Add(filter);
            }

            return picker;
        }

        public static FileSavePicker GetFileSavePicker(string suggestedFileName)
        {
            var picker = new FileSavePicker
            {
                SuggestedFileName = suggestedFileName,
                SuggestedStartLocation = PickerLocationId.Desktop,
            };

            var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(picker, hWnd);

            picker.FileTypeChoices.Add("Setup", [".exe"]);

            return picker;
        }

        public static FolderPicker GetFolderPicker()
        {
            var picker = new FolderPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Desktop,
            };

            var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(picker, hWnd);

            picker.FileTypeFilter.Add("*");

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
