namespace Elvenar
{
    public class OtherViewModel : ViewModelBase
    {
        public OtherViewModel()
            :base(null)
        {

        }

        //private void RemovePolieren(object sender, RoutedEventArgs e)
        //{
        //    var selectedPolieren = cbxPolieren.SelectedItem as string;
        //    if (selectedPolieren == null) return;
        //    PolierenList.Remove(selectedPolieren);
        //    SetModified(true);
        //    NotifyPropertyChanged("PolierenList");
        //}

        //private void AddPolieren(object sender, RoutedEventArgs e)
        //{
        //    var macro = dataGridMacros.SelectedItem as Macro;
        //    if (macro == null) return;
        //    if (PolierenList.Contains(macro.Name)) return;
        //    PolierenList.Add(macro.Name);
        //    SetModified(true);
        //    NotifyPropertyChanged("PolierenList");
        //}
    }
}