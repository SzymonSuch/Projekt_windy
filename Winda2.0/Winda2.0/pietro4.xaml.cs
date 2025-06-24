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
    /// Logika interakcji dla klasy pietro4.xaml
    /// </summary>
    public partial class pietro4 : Window
    {
        private MainWindow mainWindow;
        public pietro4(MainWindow mw)
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
            if (mainWindow != null && (mainWindow.CurrentDirection == -1 && mainWindow.CurrentFloor < 4 || mainWindow.IsPendingFloorsEmpty))
            {
                await mainWindow.GoToFloor(4); // dodajemy 1 do kolejki
                //this.Close(); // <-- ZAMKNIJ okno pietro1
            }
            if (mainWindow != null && mainWindow.CurrentFloor == 4 && !mainWindow.isMoving)
            {
                if (!mainWindow.AreDoorsOpen)
                {
                    await mainWindow.OpenDoors();
                }
                var result = MessageBox.Show("Czy chcesz wejść do windy?", "Winda", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    mainWindow.SetUserInElevator(true);
                    mainWindow.Show();
                    this.Close();
                }
            }
        }

        private async void GoDown_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow != null && (mainWindow.CurrentDirection == 1 && mainWindow.CurrentFloor > 4 || mainWindow.IsPendingFloorsEmpty))
            {
                await mainWindow.GoToFloor(4); // dodajemy 1 do kolejki
                //this.Close(); // <-- ZAMKNIJ okno pietro1
            }
            if (mainWindow != null && mainWindow.CurrentFloor == 4 && !mainWindow.isMoving)
            {
                if (!mainWindow.AreDoorsOpen)
                {
                    await mainWindow.OpenDoors();
                }
                var result = MessageBox.Show("Czy chcesz wejść do windy?", "Winda", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    mainWindow.SetUserInElevator(true);
                    mainWindow.Show();
                    this.Close();
                }
            }
        }
        public void UpdateFloorDisplay(int floor)
        {
            FloorDisplay.Text = $"Piętro: {floor}";
        }
    }
}
