using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Elvenar
{
    public class PolierenViewModel : ViewModelBase
    {
        public ObservableCollection<string> PolierenList { get; set; }
        public int CurrentIndex { get; set; }
        public int MyPosition { get; set; }

        public PolierenViewModel(IEnumerable<string> polieren, int myPosition, MacroService macroService)
            :base(macroService)
        {
            PolierenList = new ObservableCollection<string>(polieren ?? new string[0]);
            MyPosition = myPosition;
        }

        public void Run()
        {
            var macroName = PolierenList[CurrentIndex];
            StartTask(() => _macroService.Run(macroName), () =>
            {
                CurrentIndex = GetNextIndex(CurrentIndex, PolierenList.Count);
                if (CurrentIndex + 1 == MyPosition)
                    CurrentIndex = GetNextIndex(CurrentIndex, PolierenList.Count);
                NotifyPropertyChanged("CurrentIndex");
            });
        }
    }
}