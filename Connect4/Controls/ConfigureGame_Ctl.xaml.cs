using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Connect4
{
    /// <summary>
    /// Interaction logic for ConfigureGame_Ctl.xaml
    /// </summary>
    public partial class ConfigureGame_Ctl : Window, System.ComponentModel.INotifyPropertyChanged
    {
        public ConfigureGame_Ctl()
        {
            InitializeComponent();
        }
        
        #region INotifyPropertyChanged Implementation
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        // I like this pattern very much: It greatly improves the maintainability of the class since property name changes will not break things (vs. explicitely providing the property name string).
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
