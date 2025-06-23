using System.Windows;
using System.Windows.Controls;

namespace Winda2._0
{
    public partial class LoginWindow : Window
    {
        public bool IsAuthenticated { get; private set; } = false;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;

            if (login == "admin" && password == "admin")
            {
                MessageBox.Show("Dostęp przyznany", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                IsAuthenticated = true;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Brak dostępu", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordBox.Clear();
                LoginBox.Focus();
            }
        }
    }
}
