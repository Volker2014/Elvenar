using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Elvenar
{
    public class QuestViewModel : ViewModelBase
    {
        public ObservableCollection<string> QuestList { get; set; }
        public int CurrentIndex { get; set; }
        public int NextIndex { get; set; }

        public QuestViewModel(IEnumerable<string> quests, MacroService macroService)
            :base(macroService)
        {
            QuestList = new ObservableCollection<string>(quests ?? new string[0]);
            NextIndex = -1;
        }

        public void RunFinish()
        {
            RunQuestLoop("Quest Abschliessen");
        }

        public void RunCancel()
        {
            RunQuestLoop("Quest Ablehnen");
        }

        private void RunQuestLoop(string questMacro)
        {
            if (NextIndex == -1)
                NextIndex = GetNextIndex(CurrentIndex, QuestList.Count);
            StartTask(() =>
            {
                var currentIndex = RunQuest(questMacro, CurrentIndex);

                while (currentIndex != NextIndex)
                {
                    if (currentIndex == -1) return false;
                    currentIndex = RunQuest("Quest Ablehnen", currentIndex);
                }
                return true;
            }, () =>
            {
                CurrentIndex = NextIndex;
                NextIndex = GetNextIndex(CurrentIndex, QuestList.Count);
                NotifyPropertyChanged("CurrentIndex");
                NotifyPropertyChanged("NextIndex");
            });
        }

        private int RunQuest(string questMacro, int index)
        {
            if (!_macroService.Run(questMacro)) return -1;
            return GetNextIndex(index, QuestList.Count);
        }
    }
}