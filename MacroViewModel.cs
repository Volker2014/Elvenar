using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Elvenar
{
    public class MacroViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Macro> Macros { get; set; }
        public ObservableCollection<Symbol> Symbols { get; set; }
        public Macro SelectedMacro { get; set; }
        public ObservableCollection<Step> Steps { get; set; }

        public bool IsModified { get; set; }

        public MacroViewModel(Symbol[] symbols, Macro[] macros)
        {
            Symbols = new ObservableCollection<Symbol>(symbols ?? new Symbol[0]);
            Macros = new ObservableCollection<Macro>(macros ?? new Macro[0]); ;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void CommitChanges()
        {
            if (SelectedMacro == null || Steps == null) return;
            SelectedMacro.Steps = Steps.ToArray();
            IsModified = true;
        }

        public void SelectMacro(Macro macro)
        {
            if (macro == null) return;
            CommitChanges();
            if (macro.Steps == null)
                macro.Steps = new List<Step>().ToArray();
            SelectedMacro = macro;
            Steps = new ObservableCollection<Step>(macro.Steps);
            NotifyPropertyChanged("Steps");
        }

        public void RemoveSymbol(Step step)
        {
            if (step == null || Steps == null) return;
            IsModified = Steps.Remove(step);
            NotifyPropertyChanged("Steps");
        }

        public void AddSymbol(Symbol selectedSymbol)
        {
            if (SelectedMacro == null || selectedSymbol == null || Steps == null) return;
            Steps.Add(new Step { Symbol = selectedSymbol.Name, Delay = selectedSymbol.Delay });
            IsModified = true;
            NotifyPropertyChanged("Steps");
        }

        public void MoveSymbolUp(int selectedIndex, Step step)
        {
            if (selectedIndex <= 0) return;
            Steps.Insert(selectedIndex - 1, step);
            Steps.RemoveAt(selectedIndex);
            IsModified = true;
            NotifyPropertyChanged("Steps");
        }

        public void MoveSymbolDown(int selectedIndex, Step step)
        {
            if (selectedIndex < 0 || selectedIndex + 1 >= Steps.Count) return;
            Steps.Insert(selectedIndex + 2, step);
            Steps.RemoveAt(selectedIndex);
            IsModified = true;
            NotifyPropertyChanged("Steps");
        }
    }
}
