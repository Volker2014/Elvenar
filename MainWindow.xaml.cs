using MouseKeyboardLibrary;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Elvenar
{
    public enum TElvenarViews
    {
        Quest = 0,
        Polieren,
        Other
    };

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<Macro> Macros { get; set; }
        public ViewModelBase CurrentView { get { return _views[(int)_selectedView]; } }

        private TElvenarViews _selectedView;
        private ObservableCollection<ViewModelBase> _views;

        public Brush SaveBorder { get; set; }

        //public string NewQuest
        //{
        //    set
        //    {
        //        if (cbxQuestCurrent.SelectedItem != null)
        //        {
        //            if (string.IsNullOrEmpty(cbxQuestCurrent.SelectedItem as string))
        //                cbxQuestCurrent.Items.RemoveAt(cbxQuestCurrent.SelectedIndex);
        //            return;
        //        }
        //        if (!string.IsNullOrEmpty(value))
        //        {
        //            QuestList.Add(value);
        //            cbxQuestCurrent.SelectedItem = value;
        //        }
        //    }
        //}

        private string _filename = "";
        private bool _isModified = false;
        private MacroService _macroService;
        private ElvenarService _elvenarService;
        private ElvenarEnv _elvenar;
       
        private Task _macroTask;
        private CancellationTokenSource _macroToken;
        private KeyboardHook _keyHook;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _selectedView = TElvenarViews.Quest;
            _elvenarService = new ElvenarService();
            InitServices(new ElvenarEnv());
            Loaded += Load;
            SaveBorder = Brushes.Black;
        }

        private void InitServices(ElvenarEnv elvenar)
        {
            _elvenar = elvenar;
            _macroService = new MacroService(elvenar.Symbols, elvenar.Macros, LeftClick, GetMousePosition, KeyPress);
            _views = new ObservableCollection<ViewModelBase>
            {
                new QuestViewModel(elvenar.Quests, _macroService),
                new PolierenViewModel(elvenar.Polieren, elvenar.MyPosition, _macroService),
                new OtherViewModel(),
            };
            NotifyPropertyChanged("CurrentView");
        }

        private void Quest(object sender, RoutedEventArgs e)
        {
            _selectedView = TElvenarViews.Quest;
            NotifyPropertyChanged("CurrentView");
        }

        private void Polieren(object sender, RoutedEventArgs e)
        {
            _selectedView = TElvenarViews.Polieren;
            NotifyPropertyChanged("CurrentView");
        }

        private void Other(object sender, RoutedEventArgs e)
        {
            _selectedView = TElvenarViews.Other;
            NotifyPropertyChanged("CurrentView");
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Elvenar Macro|*.elm";
            if (!string.IsNullOrEmpty(_filename))
            {
                dlg.FileName = _filename;
            }
            else
            {
                dlg.InitialDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
            }
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            _filename = dlg.FileName;
            Title = "Elvenar Makros - " + _filename;

            ElvenarEnv elvenar = _elvenarService.Load(_filename);
            InitServices(elvenar);
        }

        private void ShowHelp(object sender, RoutedEventArgs e)
        {
            var message = "Elvenar Makro Tool - Version " + Assembly.GetExecutingAssembly().GetName().Version +
                "\n\nLinke Seite:\nListe der Makros. Auswählen und Ausführen\n" +
                "Mitte:\nListe der Symbole mit Warten nach dem Klick. Reihenfolge über Pfeil nach oben/unten. "+
                "Entfernen über Pfeil nach rechts.\n" +
                "rechte Seite:\nListe der Symbole. Bestimme, um neues Symbol mit Position aufzunehmen\n" +
                "Auswählen und zum Makro hinzufügen (Pfeil nach links) oder Position anzeigen.";
            System.Windows.Forms.MessageBox.Show(message, "Hilfe Elvenar Makro", MessageBoxButtons.OK);
        }

        private void OpenSymbolWindow(object sender, RoutedEventArgs e)
        {
            var viewModel = new SymbolViewModel(_elvenar.Symbols);
            var symbolWindow = new SymbolWindow(viewModel, new MouseHook(), new KeyboardHook());

            if (symbolWindow.ShowDialog() != true) return;

            _elvenar.Symbols = viewModel.Symbols.ToArray();
            var service = new MacroService(_elvenar.Symbols, _elvenar.Macros, LeftClick, GetMousePosition, KeyPress);
            foreach (var replace in viewModel.ReplaceSymbolNames)
                service.ReplaceSymbolName(Macros, replace.Key, replace.Value);
            SetModified(true);
        }

        private void OpenMacroWindow(object sender, RoutedEventArgs e)
        {
            var viewModel = new MacroViewModel(_elvenar.Symbols, _elvenar.Macros);
            var macroWindow = new MacroWindow(viewModel);
            if (macroWindow.ShowDialog() != true) return;

            _elvenar.Macros = viewModel.Macros.ToArray();
            Macros = new ObservableCollection<Macro>(_elvenar.Macros ?? new Macro[0]);
            SetModified(true);
        }

        private void RunMacro(object sender, RoutedEventArgs e)
        {
            //var macro = dataGridMacros.SelectedItem as Macro;
            //if (macro == null) return;
            //StartTask(() => _macroService.Run(macro.Steps), () => dataGridMacros.SelectedItem = null);
        }

        private void StartTask(Action startAction, Action endAction)
        {
            _keyHook = new KeyboardHook();
            _keyHook.KeyDown += KeyHook_KeyDown;
            _keyHook.Start();
            startAction();
            endAction();
            _keyHook.Stop();
            _keyHook = null;
            return;

            _macroToken = new CancellationTokenSource();
            var backgroundScheduler = TaskScheduler.Default;
            var context = TaskScheduler.FromCurrentSynchronizationContext();
            _keyHook = new KeyboardHook();
            _keyHook.KeyDown += KeyHook_KeyDown;
            _keyHook.Start();
            _macroTask = Task.Factory.StartNew(() =>
            {
                startAction();
            }, _macroToken.Token, TaskCreationOptions.LongRunning, backgroundScheduler).ContinueWith((tsk) =>
            {
                _keyHook.Stop();
                _keyHook = null;
                endAction();
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, context);
        }

        private void KeyHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (_macroTask == null) return;
            if (e.KeyCode != Keys.Escape) return;
            _keyHook.Stop();
            _keyHook = null;
            _macroToken.Cancel();        
        }

        private void RunPolieren(object sender, RoutedEventArgs e)
        {
            var polieren = CurrentView as PolierenViewModel;
            polieren.Run();
        }

        private void RunKampfPaladin(object sender, RoutedEventArgs e)
        {
            StartTask(() => _macroService.Run("Autom. Kampf Paladin"), () => { });
        }

        private void RunKampf(object sender, RoutedEventArgs e)
        {
            StartTask(() => _macroService.Run("Autom. Kampf"), () => { });
        }

        private void RunProvinz(object sender, RoutedEventArgs e)
        {
            StartTask(() => _macroService.Run("zur Provinz"), () => { });
        }

        private void RunFinish(object sender, RoutedEventArgs e)
        {
            var quest = CurrentView as QuestViewModel;
            quest.RunFinish();
        }

        private void RunCancel(object sender, RoutedEventArgs e)
        {
            var quest = CurrentView as QuestViewModel;
            quest.RunCancel();
        }

        private void RemovePolieren(object sender, RoutedEventArgs e)
        {
        }

        private void AddPolieren(object sender, RoutedEventArgs e)
        {
        }

        private void LeftClick(int x, int y, int delay)
        {
            MouseSimulator.Position = new System.Drawing.Point(x, y);
            MouseSimulator.Click(MouseKeyboardLibrary.MouseButton.Left);
            Thread.Sleep(delay * 1000);
        }

        private Position GetMousePosition()
        {
            return new Position { X = MouseSimulator.Position.X, Y = MouseSimulator.Position.Y };
        }

        private void KeyPress(char key, int delay)
        {
            Thread.Sleep(delay * 1000);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (_filename == null)
            {
                var dlg = new SaveFileDialog();
                dlg.FileName = "elvenar.xml";
                dlg.InitialDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
                _filename = dlg.FileName;
            }

            _elvenarService.Save(_elvenar, ((PolierenViewModel)_views[(int)TElvenarViews.Polieren]).MyPosition, ((PolierenViewModel)_views[(int)TElvenarViews.Polieren]).PolierenList, 
                ((QuestViewModel)_views[(int)TElvenarViews.Quest]).QuestList, _filename);

            SetModified(_elvenarService.IsModified);
        }

        private void SetModified(bool modified)
        {
            _isModified = modified;
            if (modified)
                SaveBorder = Brushes.Red;
            else
                SaveBorder = Brushes.Black;
        }
    }
}
