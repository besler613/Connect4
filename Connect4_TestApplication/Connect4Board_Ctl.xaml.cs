using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Connect4_TestApplication
{
    /// <summary>
    /// Interaction logic for Connect4Board_Ctl.xaml
    /// </summary>
    public partial class Connect4Board_Ctl : UserControl
    {
        public Connect4Board_Ctl()
        {
            InitializeComponent();
        }
    }
    public class Connect4Board_CtlViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        #region Public Fields
        private int _BoardWidth;
        private int _BoardHeight;
        private Connect4.GamePosition.CheckerStateEnum _CurrentWinner = Connect4.GamePosition.CheckerStateEnum.Empty;
        private Connect4.GamePosition _GameBoard;
        public Connect4.GamePosition GameBoard
        {
            get { return _GameBoard; }
            set
            {
                _GameBoard = value;
                BoardWidth = _GameBoard.BoardWidth;
                BoardHeight = _GameBoard.BoardHeight;
                for (int i = _GameBoard.BoardHeight - 1; i >= 0; i--)   //We have to swap the indexing here since index 0 is at the bottom of a Connect 4 Board
                {
                    Connect4Board_RowViewModel nextRow = new Connect4Board_RowViewModel();
                    for (int j = 0; j < _GameBoard.BoardWidth; j++)
                        nextRow.RowData.Add(new Connect4Board_CheckerModel(_GameBoard.GetPositionState(i, j)));
                    BoardData.Add(nextRow);
                }
                _GameBoard.MoveMade += (int row, int column, Connect4.GamePosition.CheckerStateEnum checker) =>
                {
                    BoardData[_GameBoard.BoardHeight - row - 1].RowData[column].CheckerState = checker;
                    CurrentWinner = _GameBoard.GameWinner;
                };
                _GameBoard.MoveTakeBack += (int row, int column) =>
                {
                    BoardData[_GameBoard.BoardHeight - row - 1].RowData[column].CheckerState = Connect4.GamePosition.CheckerStateEnum.Empty;
                    CurrentWinner = _GameBoard.GameWinner;
                };
                OnPropertyChanged();
            }
        }
        public int BoardWidth
        {
            get { return _BoardWidth; }
            private set
            {
                _BoardWidth = value;
                OnPropertyChanged();
            }
        }
        public int BoardHeight
        {
            get { return _BoardHeight; }
            private set
            {
                _BoardHeight = value;
                OnPropertyChanged();
            }
        }
        public Connect4.GamePosition.CheckerStateEnum CurrentWinner
        {
            get { return _CurrentWinner; }
            private set
            {
                _CurrentWinner = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructors
        public Connect4Board_CtlViewModel()
        {
            
        }
        #endregion

        #region ViewModel
        private ObservableCollection<Connect4Board_RowViewModel> _BoardData = new ObservableCollection<Connect4Board_RowViewModel>();
        public ObservableCollection<Connect4Board_RowViewModel> BoardData { get { return _BoardData; } }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        // I like this pattern very much: It greatly improves the maintainability of the class since property name changes will not break things (vs. explicitely providing the property name string).
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
    public class Connect4Board_RowViewModel
    {
        #region ViewModel
        private ObservableCollection<Connect4Board_CheckerModel> _RowData = new ObservableCollection<Connect4Board_CheckerModel>();
        public ObservableCollection<Connect4Board_CheckerModel> RowData { get { return _RowData; } }
        #endregion
    }
    public class Connect4Board_CheckerModel : System.ComponentModel.INotifyPropertyChanged
    {
        #region Public Fields
        private Connect4.GamePosition.CheckerStateEnum _Checker = Connect4.GamePosition.CheckerStateEnum.Empty;
        public Connect4.GamePosition.CheckerStateEnum CheckerState
        {
            get { return _Checker; }
            set
            {
                _Checker = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructors
        public Connect4Board_CheckerModel(Connect4.GamePosition.CheckerStateEnum initialCheckerState)
        {
            CheckerState = initialCheckerState;
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        // I like this pattern very much: It greatly improves the maintainability of the class since property name changes will not break things (vs. explicitely providing the property name string).
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
    public class CheckerEnumToColorConverter : IValueConverter
    {
        #region IValueConverter Implementation
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Connect4.GamePosition.CheckerStateEnum)value)
            {
                case Connect4.GamePosition.CheckerStateEnum.Yellow:
                    return System.Windows.Media.Brushes.Yellow;
                case Connect4.GamePosition.CheckerStateEnum.Red:
                    return System.Windows.Media.Brushes.Red;
                default:
                    return System.Windows.Media.Brushes.FloralWhite;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is a one-way converter.");
        }
        #endregion
    }
    public class GameStateToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Implementation
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Connect4.GamePosition.CheckerStateEnum)value)
            {
                case Connect4.GamePosition.CheckerStateEnum.Empty:
                    return System.Windows.Visibility.Collapsed;
                default:
                    return System.Windows.Visibility.Visible;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is a one-way converter.");
        }
        #endregion
    }
    public class GameStateToStringConverter : IValueConverter
    {
        #region IValueConverter Implementation
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Connect4.GamePosition.CheckerStateEnum)value)
            {
                case Connect4.GamePosition.CheckerStateEnum.Red:
                    return "RED WINS!!";
                case Connect4.GamePosition.CheckerStateEnum.Yellow:
                    return "YELLOW WINS!!";
                default:
                    return String.Empty;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This is a one-way converter.");
        }
        #endregion
    }
}
