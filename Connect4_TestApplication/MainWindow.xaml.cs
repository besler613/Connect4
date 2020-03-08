using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Connect4_TestApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            int width = 7;
            int height = 6;
            board_VM.GameBoard = new Connect4.GamePosition(width, height);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            board_VM.GameBoard.PlayMove((int)((FrameworkElement)e.OriginalSource).DataContext);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (board_VM.GameBoard.Moves.Count > 0)
                board_VM.GameBoard.TakebackMoves(1);
        }
    }
    public class IntToIEnumerableConverter : IValueConverter
    {
        #region IValueConverter Implementation
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<int> rVal = new List<int>();
            for (int i = 0; i < (int)value; i++)
                rVal.Add(i);
            return rVal;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is a one-way converter.");
        }
        #endregion
    }
}
