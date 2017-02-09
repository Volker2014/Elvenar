using System;
using System.Collections.Generic;
using System.Linq;

namespace Elvenar
{
    public class MacroService
    {
        private IEnumerable<Symbol> _symbols;
        private Action<int, int, int> _mouseAction;
        private Action<char, int> _keyboardAction;
        private Func<Position> _mousePosition;

        public MacroService(IEnumerable<Symbol> symbols, Action<int, int, int> mouseAction, Func<Position> mousePosition, Action<char, int> keyboardAction)
        {
            _symbols = symbols;
            _mouseAction = mouseAction;
            _mousePosition = mousePosition;
            _keyboardAction = keyboardAction;
        }

        public bool Run(IEnumerable<Step> steps)
        {
            if (steps == null) return false;
            var lastPosition = _mousePosition();
            foreach (var step in steps)
            {
                var currentPosition = _mousePosition();
                if (lastPosition.X != currentPosition.X || lastPosition.Y != currentPosition.Y) return false;
                var symbol = _symbols.FirstOrDefault(s => s.Name == step.Symbol);
                if (symbol == null) continue;
                if (symbol.Position != null)
                {
                    _mouseAction(symbol.Position.X, symbol.Position.Y, step.Delay);
                    lastPosition = _mousePosition();
                }
                else if (symbol.Key != 0)
                    _keyboardAction(symbol.Key, step.Delay);
            }
            return true;
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
