using MouseKeyboardLibrary;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Elvenar
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected MacroService _macroService;

        public ViewModelBase(MacroService macroService)
        {
            _macroService = macroService;
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void StartTask(Func<bool> startAction, Action endAction)
        {
            if (startAction())
                endAction();
        }

        private bool keypressed = false;
        private void KeyHook_KeyDown(object sender, KeyEventArgs e)
        {
            keypressed = true;
        }

        protected int GetNextIndex(int index, int count)
        {
            if (index + 1 < count)
                return index + 1;
            return 0;
        }
    }
}