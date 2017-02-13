using MouseKeyboardLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Elvenar
{
    public class MacroService
    {
        private IEnumerable<Symbol> _symbols;
        private IEnumerable<Macro> _macros;
        private Action<int, int, int> _mouseAction;
        private Action<char, int> _keyboardAction;
        private Func<Position> _mousePosition;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly KeyboardHook keyHook = new KeyboardHook();

        public MacroService(IEnumerable<Symbol> symbols, IEnumerable<Macro> macros, Action<int, int, int> mouseAction, Func<Position> mousePosition, Action<char, int> keyboardAction)
        {
            _symbols = symbols;
            _macros = macros;
            _mouseAction = mouseAction;
            _mousePosition = mousePosition;
            _keyboardAction = keyboardAction;
        }

        public bool Run(string macroName)
        {
            var macro = _macros.FirstOrDefault(m => m.Name == macroName);
            if (macro == null) return false;
            keyHook.KeyDown += KeyHook_KeyDown;
            keyHook.Start();
            var task = new Task<bool>(()=> Run(macro.Steps));
            task.Start();
            task.Wait();
            keyHook.Stop();
            keyHook.KeyDown -= KeyHook_KeyDown;
            return task.Result;
        }

        private bool Run(IEnumerable<Step> steps)
        {
            if (steps == null) return false;
             foreach (var step in steps)
            {
                var currentPosition = _mousePosition();
                var symbol = _symbols.FirstOrDefault(s => s.Name == step.Symbol);
                if (symbol == null) continue;
                if (symbol.Position != null)
                {
                    _mouseAction(symbol.Position.X, symbol.Position.Y, step.Delay);
                }
                else if (symbol.Key != 0)
                    _keyboardAction(symbol.Key, step.Delay);
                if (cts.IsCancellationRequested)
                     return false;
            }
            return true;
        }

        private void KeyHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            cts.Cancel();
        }

        public void ReplaceSymbolName(IEnumerable<Macro> macros, string symbolName, string newName)
        {
            if (macros == null) return;
            foreach (var macro in macros)
            {
                if (macro.Steps == null) continue;
                foreach (var step in macro.Steps)
                {
                    if (string.IsNullOrEmpty(step.Symbol)) continue;
                    if (step.Symbol == symbolName)
                        step.Symbol = newName;
                }
            }
        }
    }
}
