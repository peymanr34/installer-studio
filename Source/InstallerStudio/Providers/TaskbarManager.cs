using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using WinRT.Interop;

namespace InstallerStudio.Providers
{
    internal partial class TaskbarManager : IDisposable
    {
        private readonly nint _windowHandle;

        private ITaskbarList4 _taskbar;

        private bool _disposed;

        private TaskbarManager(nint windowHandle, ITaskbarList4 taskbar)
        {
            _windowHandle = windowHandle;
            _taskbar = taskbar;
        }

        public void SetProgressState(TBPFLAG state)
        {
            ThrowIfDisposed();
            _taskbar.SetProgressState(new HWND(_windowHandle), state);
        }

        public void SetProgressValue(int value, int total)
        {
            ThrowIfDisposed();
            _taskbar.SetProgressValue(new HWND(_windowHandle), Convert.ToUInt64(value), Convert.ToUInt64(total));
        }

        public static TaskbarManager Create(Window window)
        {
            ArgumentNullException.ThrowIfNull(window);

            var result = PInvoke.CoCreateInstance(new Guid("56FDF344-FD6D-11d0-958A-006097C9A090"), null,
                CLSCTX.CLSCTX_INPROC_SERVER, out ITaskbarList4 taskbar);

            if (result.Failed)
            {
                return null;
            }

            taskbar.HrInit();

            var handle = WindowNative.GetWindowHandle(window);
            return new TaskbarManager(handle, taskbar);
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        ~TaskbarManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing && _taskbar is not null)
            {
                Marshal.ReleaseComObject(_taskbar);
                _taskbar = null;
            }

            _disposed = true;
        }
    }
}

