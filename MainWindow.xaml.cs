using MouseKeyboardLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace Elvenar
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private MouseHook mouseHook = new MouseHook();

        public ObservableCollection<Symbol> Symbols { get; set; }
        public ObservableCollection<Macro> Macros { get; set; }
        public Macro SelectedMacro { get; set; }
        public ObservableCollection<Step> Steps { get; set; }

        public Brush SaveBorder { get; set; }

        private string _filename = "";
        private bool _isModified = false;
        private MacroService _macroService;
        private ElvenarService _elvenarService;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _elvenarService = new ElvenarService();
            InitServices(new ElvenarEnv());
            mouseHook.MouseMove += mouseHook_MouseMove;
            mouseHook.MouseDown += mouseHook_MouseDown;
            Loaded += Load;
            SaveBorder = Brushes.Black;
        }

        private void InitServices(ElvenarEnv elvenar)
        {
            Symbols = new ObservableCollection<Symbol>(elvenar.Symbols ?? new Symbol[0]);
            Macros = new ObservableCollection<Macro>(elvenar.Macros ?? new Macro[0]);
            _macroService = new MacroService(Symbols);
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.FileName = "elvenar.xml";
            dlg.InitialDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            _filename = dlg.FileName;
            Title = "Elvenar Makros - " + _filename;

            ElvenarEnv elvenar = _elvenarService.Load(_filename);
            InitServices(elvenar);

            NotifyPropertyChanged("Symbols");
            NotifyPropertyChanged("Macros");
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

        private void mouseHook_MouseMove(object sender, MouseEventArgs e)
        {
            labelXY.Content = new Position { X = e.X, Y = e.Y }.ToString();
        }

        private void mouseHook_MouseDown(object sender, MouseEventArgs e)
        {
            btnSymbolSelect.Content = "Bestimme";
            mouseHook.Stop();
            var point = btnSymbolSelect.PointFromScreen(new System.Windows.Point(e.X, e.Y));
            var rect = new Rect(new System.Windows.Point(0, 0), new System.Windows.Size(btnSymbolSelect.ActualWidth, btnSymbolSelect.ActualHeight));
            if (rect.Contains(point))
                return;

            var pos = new Position { X = e.X, Y = e.Y };
            var selectedSymbol = dataGridSymbols.SelectedItem as Symbol;
            if (selectedSymbol != null)
            {
                selectedSymbol.Position = pos;
            }
            else
                Symbols.Add(new Symbol { Name = dataGridSymbols.Items.Count.ToString(), Position = pos, Delay = 1 });
            SetModified(true);
            NotifyPropertyChanged("Symbols");
        }

        private void RunMacro(object sender, RoutedEventArgs e)
        {
            _macroService.Run(Steps, LeftClick);
        }

        private void LeftClick(int x, int y, int delay)
        {
            MouseSimulator.Position = new System.Drawing.Point(x, y);
            MouseSimulator.Click(MouseButton.Left);
            Thread.Sleep(delay * 1000);
        }

        private void SelectSymbol(object sender, RoutedEventArgs e)
        {
            if (mouseHook.IsStarted) return;
            mouseHook.Start();
            btnSymbolSelect.Content = "Abbruch";
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (SelectedMacro != null)
                SelectedMacro.Steps = Steps.ToArray();

            if (_filename == null)
            {
                var dlg = new SaveFileDialog();
                dlg.FileName = "elvenar.xml";
                dlg.InitialDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
                _filename = dlg.FileName;
            }

            _elvenarService.Save(Symbols, Macros, _filename);

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

        private void ShowSymbol(object sender, RoutedEventArgs e)
        {
            var selectedSymbol = dataGridSymbols.SelectedItem as Symbol;
            if (selectedSymbol == null) return;
            MouseSimulator.Position = new System.Drawing.Point(selectedSymbol.Position.X, selectedSymbol.Position.Y);
            Thread.Sleep(100);
            MouseSimulator.Click(MouseButton.Right);
        }

        private void RemoveSymbol(object sender, RoutedEventArgs e)
        {
            var step = dataGridSteps.SelectedItem as Step;
            if (step == null) return;
            Steps.Remove(step);
            SetModified(true);
            NotifyPropertyChanged("Steps");
        }

        private void AddSymbol(object sender, RoutedEventArgs e)
        {
            if (SelectedMacro == null) return;
            var selectedSymbol = dataGridSymbols.SelectedItem as Symbol;
            if (selectedSymbol == null) return;
            Steps.Add(new Step { Symbol = selectedSymbol.Name, Delay = selectedSymbol.Delay });
            SetModified(true);
            NotifyPropertyChanged("Steps");
        }

        private void MoveSymbolUp(object sender, RoutedEventArgs e)
        {
            if (dataGridSteps.SelectedIndex <= 0) return;
            Steps.Insert(dataGridSteps.SelectedIndex - 1, dataGridSteps.SelectedItem as Step);
            Steps.RemoveAt(dataGridSteps.SelectedIndex);
            SetModified(true);
            NotifyPropertyChanged("Steps");
        }

        private void MoveSymbolDown(object sender, RoutedEventArgs e)
        {
            if (dataGridSteps.SelectedIndex < 0 || dataGridSteps.SelectedIndex+1 >= dataGridSteps.Items.Count) return;
            Steps.Insert(dataGridSteps.SelectedIndex + 2, dataGridSteps.SelectedItem as Step);
            Steps.RemoveAt(dataGridSteps.SelectedIndex);
            SetModified(true);
            NotifyPropertyChanged("Steps");
        }

        private void dataGridMacros_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var macro = dataGridMacros.SelectedItem as Macro;
            if (macro == null) return;
            if (SelectedMacro != null)
                SelectedMacro.Steps = Steps.ToArray();
            if (macro.Steps == null)
                macro.Steps = new List<Step>().ToArray();
            SelectedMacro = macro;
            Steps = new ObservableCollection<Step>(macro.Steps);
            NotifyPropertyChanged("Steps");
        }

        private void dataGridSymbols_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            var symbol = e.EditingElement.DataContext as Symbol;
            var textBox = e.EditingElement as System.Windows.Controls.TextBox;
            if (symbol == null || textBox == null || e.Column.DisplayIndex != 0 || symbol.Name == textBox.Text) return;

            new MacroService(Symbols).ReplaceSymbolName(Macros, symbol.Name, textBox.Text);
            dataGridMacros_MouseUp(null, null);
            SetModified(true);
        }
    }
}
