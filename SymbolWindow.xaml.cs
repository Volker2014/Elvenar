using MouseKeyboardLibrary;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System;

namespace Elvenar
{
    /// <summary>
    /// Interaktionslogik für SymbolWindow.xaml
    /// </summary>
    public partial class SymbolWindow : Window
    {
        private IMouseHook _mouseHook;
        private IKeyboardHook _keyboardHook;
        private SymbolViewModel _viewModel;

        public SymbolWindow(SymbolViewModel viewModel, IMouseHook mouseHook, IKeyboardHook keyboardHook)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _mouseHook = mouseHook;
            _mouseHook.MouseMove += mouseHook_MouseMove;
            _mouseHook.MouseDown += mouseHook_MouseDown;

            _keyboardHook = keyboardHook;
            _keyboardHook.KeyPress += keyboardHook_KeyPress;
        }

        private void keyboardHook_KeyPress(object sender, KeyPressEventArgs e)
        {
            StopHook();
            var selectedSymbol = dataGridSymbols.SelectedItem as Symbol;
            _viewModel.ChangePositionOrAddSymbol(selectedSymbol, e.KeyChar);
        }

        private void mouseHook_MouseMove(object sender, MouseEventArgs e)
        {
            labelXY.Content = new Position { X = e.X, Y = e.Y }.ToString();
        }

        private void mouseHook_MouseDown(object sender, MouseEventArgs e)
        {
            StopHook();
            var point = btnSymbolSelect.PointFromScreen(new System.Windows.Point(e.X, e.Y));
            var rect = new Rect(new System.Windows.Point(0, 0), new System.Windows.Size(btnSymbolSelect.ActualWidth, btnSymbolSelect.ActualHeight));
            if (rect.Contains(point))
                return;

            var pos = new Position { X = e.X, Y = e.Y };
            var selectedSymbol = dataGridSymbols.SelectedItem as Symbol;
            _viewModel.ChangePositionOrAddSymbol(selectedSymbol, pos);
        }

        private void StopHook()
        {
            btnSymbolSelect.Content = "Bestimme";
            _mouseHook.Stop();
            _keyboardHook.Stop();
        }

        internal void SelectSymbol(object sender, RoutedEventArgs e)
        {
            if (_mouseHook.IsStarted || _keyboardHook.IsStarted) return;
            _mouseHook.Start();
            _keyboardHook.Start();
            btnSymbolSelect.Content = "Abbruch";
        }

        private void ShowSymbol(object sender, RoutedEventArgs e)
        {
            var selectedSymbol = dataGridSymbols.SelectedItem as Symbol;
            if (selectedSymbol == null) return;
            MouseSimulator.Position = new System.Drawing.Point(selectedSymbol.Position.X, selectedSymbol.Position.Y);
            Thread.Sleep(100);
            MouseSimulator.Click(MouseButton.Right);
        }

        private void dataGridSymbols_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            var symbol = e.EditingElement.DataContext as Symbol;
            var textBox = e.EditingElement as System.Windows.Controls.TextBox;
            if (symbol == null || textBox == null || e.Column.DisplayIndex != 0 || symbol.Name == textBox.Text) return;

            _viewModel.ReplaceSymbolName(symbol.Name, textBox.Text);
        }

        internal void CommitChanges(object sender, RoutedEventArgs e)
        {
            Close(_viewModel.IsModified);
        }

        internal void CancelChanges(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void Close(bool result)
        {
            _mouseHook.MouseMove -= mouseHook_MouseMove;
            _mouseHook.MouseDown -= mouseHook_MouseDown;

            DialogResult = result;
            Close();
        }

        private void dataGridSymbols_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count == 0) return;
            _viewModel.IsModified = true;
        }
    }
}
