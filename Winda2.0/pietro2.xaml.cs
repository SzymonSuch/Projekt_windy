using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Winda2._0
{
    /// <summary>
    /// Logika interakcji dla klasy pietro1.xaml
    /// </summary>
    public partial class pietro2 : Window
    {
        //public pietro1()
        //{
        //    InitializeComponent(); // bez tego okno będzie puste (brak treści z XAML)
        //}
        private MainWindow mainWindow;
        public pietro2(MainWindow mw)
        {
            InitializeComponent();
            mainWindow = mw;
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            mainWindow.pietro1Window = null;
        }
        private async void GoUp_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow != null && (mainWindow.CurrentDirection == 1 || mainWindow.CurrentDirection == 0 || (mainWindow.CurrentDirection == -1 && mainWindow.CurrentFloor < 2)))
            {
                await mainWindow.GoToFloor(2);

            }
            //else
            //{
            //    MessageBox.Show("Winda nie jedzie w górę!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            //}
        }

        private async void GoDown_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow != null && (mainWindow.CurrentDirection == -1 || mainWindow.CurrentDirection == 0 || (mainWindow.CurrentDirection == 1 && mainWindow.CurrentFloor > 1)))
            {
                await mainWindow.GoToFloor(2);

            }
        }
        public void UpdateFloorDisplay(int floor)
        {
            FloorDisplay.Text = $"Piętro: {floor}";
        }
    }
}
