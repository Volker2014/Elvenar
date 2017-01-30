using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Elvenar
{
    /// <summary>
    /// Interaktionslogik für MacroWindow.xaml
    /// </summary>
    public partial class MacroWindow : Window
    {
        private MacroViewModel _viewModel;

        public MacroWindow(MacroViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
        }

        private void dataGridMacros_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var macro = dataGridMacros.SelectedItem as Macro;
            _viewModel.SelectMacro(macro);
        }

        private void RemoveSymbol(object sender, RoutedEventArgs e)
        {
            var step = dataGridSteps.SelectedItem as Step;
            _viewModel.RemoveSymbol(step);
        }

        private void AddSymbol(object sender, RoutedEventArgs e)
        {
            var selectedSymbol = dataGridSymbols.SelectedItem as Symbol;
            _viewModel.AddSymbol(selectedSymbol);
        }

        private void MoveSymbolUp(object sender, RoutedEventArgs e)
        {
            _viewModel.MoveSymbolUp(dataGridSteps.SelectedIndex, dataGridSteps.SelectedItem as Step);
        }

        private void MoveSymbolDown(object sender, RoutedEventArgs e)
        {
            _viewModel.MoveSymbolDown(dataGridSteps.SelectedIndex + 2, dataGridSteps.SelectedItem as Step);
        }

        internal void CommitChanges(object sender, RoutedEventArgs e)
        {
            _viewModel.CommitChanges();
            Close(_viewModel.IsModified);
        }

        internal void CancelChanges(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void Close(bool result)
        {
            DialogResult = result;
            Close();
        }
    }
}
