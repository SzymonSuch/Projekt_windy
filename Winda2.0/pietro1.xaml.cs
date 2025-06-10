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
    public partial class pietro1 : Window
    {
        //public pietro1()
        //{
        //    InitializeComponent(); // bez tego okno będzie puste (brak treści z XAML)
        //}
        private MainWindow mainWindow;
        public pietro1(MainWindow mw)
        {
            InitializeComponent();
            mainWindow = mw;
        }
        private async void GoUp_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Kliknięto GoUp");
            if (mainWindow != null)
            {
                await mainWindow.GoToFloor(1); // dodajemy 1 do kolejki
                //this.Close(); // <-- ZAMKNIJ okno pietro1
            }
            else
            {
                MessageBox.Show("Błąd: brak odniesienia do głównego okna.");
            }
        }

    }
}
