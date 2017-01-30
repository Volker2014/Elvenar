using System.Windows.Forms;

namespace MouseKeyboardLibrary
{
    public interface IMouseHook
    {
        event MouseEventHandler MouseMove;
        event MouseEventHandler MouseDown;

        bool IsStarted { get; }
 
        void Start();
        void Stop();
    }
}