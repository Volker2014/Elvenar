using MouseKeyboardLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Elvenar
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MouseHook mouseHook = new MouseHook();

        public ObservableCollection<Symbol> Symbols { get; set; }
        public ObservableCollection<Macro> Macros { get; set; }
        public Macro SelectedMacro { get; set; }
        public ObservableCollection<Step> Steps { get; set; }

        private string Filename { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Symbols = new ObservableCollection<Symbol>();
            Macros = new ObservableCollection<Macro>();
            Load(null, null);
            mouseHook.MouseMove += mouseHook_MouseMove;
            mouseHook.MouseDown += mouseHook_MouseDown;
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.FileName = "elvenar.xml";
            dlg.InitialDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            Filename = dlg.FileName;
            Title = "Elvenar Makros - " + Filename;

            var serializer = new XmlSerializer(typeof(ElvenarEnv));
            var writer = new StreamReader(Filename);
            var elvenar = serializer.Deserialize(writer) as ElvenarEnv;
            Symbols = new ObservableCollection<Symbol>(elvenar.Symbols ?? new Symbol[0]);
            dataGridSymbols.ItemsSource = Symbols;
            Macros = new ObservableCollection<Macro>(elvenar.Macros ?? new Macro[0]);
            dataGridMacros.ItemsSource = Macros;
        }

        private void ShowHelp(object sender, RoutedEventArgs e)
        {
            var message = "Elvenar Makro Tool - Version " + Assembly.GetExecutingAssembly().GetName().Version +
                "\n\nLinke Seite:\nListe der Makros. Auswählen und Ausführen\n" +
                "Mitte:\nListe der Symbole mit Verzögerung vor Klick. Reihenfolge über Pfeil nach oben/unten. "+
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
            var point = btnSymbolSelect.PointFromScreen(new Point(e.X, e.Y));
            var rect = new Rect(new Point(0, 0), new Size(btnSymbolSelect.ActualWidth, btnSymbolSelect.ActualHeight));
            if (rect.Contains(point))
                return;

            var pos = new Position { X = e.X, Y = e.Y };
            var selectedSymbol = dataGridSymbols.SelectedItem as Symbol;
            if (selectedSymbol != null)
            {
                selectedSymbol.Position = pos;
            }
            else
                Symbols.Add(new Symbol { Name = dataGridSymbols.Items.Count.ToString(), Position = pos });
            dataGridSymbols.ItemsSource = null;
            dataGridSymbols.ItemsSource = Symbols;
        }

        private void RunMacro(object sender, RoutedEventArgs e)
        {
            if (Steps == null) return;
            foreach (var step in Steps)
            {
                var symbol = Symbols.FirstOrDefault(s => s.Name == step.Symbol);
                if (symbol == null) continue;
                LeftClick(symbol.Position.X, symbol.Position.Y, step.Delay);
            }
        }

        private void LeftClick(int x, int y, int wait)
        {
            Thread.Sleep(wait * 1000);
            MouseSimulator.Position = new System.Drawing.Point(x, y);
            MouseSimulator.Click(MouseButton.Left);
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

            if (Filename == null)
            {
                var dlg = new SaveFileDialog();
                dlg.FileName = "elvenar.xml";
                dlg.InitialDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
                Filename = dlg.FileName;
            }

            var elvenar = new ElvenarEnv { Symbols = Symbols.ToArray(), Macros = Macros.ToArray() };
            var serializer = new XmlSerializer(elvenar.GetType());
            var writer = new StreamWriter(Filename);
            serializer.Serialize(writer, elvenar);
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
            dataGridSteps.ItemsSource = Steps;
        }

        private void AddSymbol(object sender, RoutedEventArgs e)
        {
            if (SelectedMacro == null) return;
            var selectedSymbol = dataGridSymbols.SelectedItem as Symbol;
            if (selectedSymbol == null) return;
            Steps.Add(new Step { Symbol = selectedSymbol.Name, Delay = 1 });
            dataGridSteps.ItemsSource = Steps;
        }

        private void MoveSymbolUp(object sender, RoutedEventArgs e)
        {
            if (dataGridSteps.SelectedIndex <= 0) return;
            Steps.Insert(dataGridSteps.SelectedIndex - 1, dataGridSteps.SelectedItem as Step);
            Steps.RemoveAt(dataGridSteps.SelectedIndex);
            dataGridSteps.ItemsSource = Steps;
        }

        private void MoveSymbolDown(object sender, RoutedEventArgs e)
        {
            if (dataGridSteps.SelectedIndex < 0 || dataGridSteps.SelectedIndex+1 >= dataGridSteps.Items.Count) return;
            Steps.Insert(dataGridSteps.SelectedIndex + 2, dataGridSteps.SelectedItem as Step);
            Steps.RemoveAt(dataGridSteps.SelectedIndex);
            dataGridSteps.ItemsSource = Steps;
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
            dataGridSteps.ItemsSource = Steps;
        }
    }
}
