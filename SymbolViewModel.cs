using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Elvenar
{
    public class SymbolViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Symbol> Symbols { get; set; }

        public IDictionary<string, string> ReplaceSymbolNames { get; private set; }

        public bool IsModified { get; set; }

        public SymbolViewModel(Symbol[] symbols)
        {
            Symbols = new ObservableCollection<Symbol>(symbols ?? new Symbol[0]);
            ReplaceSymbolNames = new Dictionary<string, string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ChangePositionOrAddSymbol(Symbol selected, Position pos)
        {
            if (selected != null)
                selected.Position = pos;
            else
                Symbols.Add(new Symbol { Name = Symbols.Count.ToString(), Position = pos, Delay = 1 });
            IsModified = true;
            NotifyPropertyChanged("Symbols");
        }

        public void ChangePositionOrAddSymbol(Symbol selected, char key)
        {
            if (selected != null)
                selected.Key = key;
            else
                Symbols.Add(new Symbol { Name = Symbols.Count.ToString(), Key = key, Delay = 1 });
            IsModified = true;
            NotifyPropertyChanged("Symbols");

        }

        public void ReplaceSymbolName(string oldName, string newName)
        {
            if (oldName == null) return;
            ReplaceSymbolNames.Add(oldName, newName);
            IsModified = true;
        }
    }
}
