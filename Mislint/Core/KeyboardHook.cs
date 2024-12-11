using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using static Mislint.Core.KeyboardHook;

namespace Mislint.Core
{
    public class KeyPressedEventArgs : EventArgs
    {
        private ModifierKeys _modifier;
        private VirtualKey _key;

        internal KeyPressedEventArgs(ModifierKeys modifier, VirtualKey key)
        {
            _modifier = modifier;
            _key = key;
        }

        public ModifierKeys Modifier
        {
            get { return _modifier; }
        }

        public VirtualKey Key
        {
            get { return _key; }
        }
    }

    public class KeyboardHook : IDisposable
    {
        private int _currentId;
        private IntPtr _handle;
        public KeyboardHook(MainWindow mainWindow, IntPtr handle)
        {
            // register the event of the inner native window.
            mainWindow.KeyPressed += (sender,args) => 
            {
                this.KeyPressed?.Invoke(this, args);
            };
            this._handle = handle;
        }

        public void RegisterHotKey(ModifierKeys modifier, VirtualKey key)
        {
            this._currentId++;
            if (NativeMethods.RegisterHotKey(this._handle, _currentId, modifier, key) != 0)
            {
                
            }
        }

        public void Dispose()
        {
            for (int i = _currentId; i > 0; i--)
            {
                var _ = NativeMethods.UnregisterHotKey(this._handle, i);
            }
        }

        /// <summary>
        /// A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        [Flags]
        public enum ModifierKeys : uint
        {
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }
    }
}
