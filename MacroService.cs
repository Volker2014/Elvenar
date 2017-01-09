using System;
using System.Collections.Generic;
using System.Linq;

namespace Elvenar
{
    public class MacroService
    {
        private IEnumerable<Symbol> _symbols;

        public MacroService(IEnumerable<Symbol> symbols)
        {
            _symbols = symbols;
        }

        public void Run(IEnumerable<Step> steps, Action<int, int, int> runAction)
        {
            if (steps == null) return;
            foreach (var step in steps)
            {
                var symbol = _symbols.FirstOrDefault(s => s.Name == step.Symbol);
                if (symbol == null) continue;
                runAction(symbol.Position.X, symbol.Position.Y, step.Delay);
            }
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
