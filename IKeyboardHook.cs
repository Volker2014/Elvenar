using System.Windows.Forms;

namespace MouseKeyboardLibrary
{
    public interface IKeyboardHook
    {
        event KeyPressEventHandler KeyPress;

        bool IsStarted { get; }

        void Start();
        void Stop();
    }
}