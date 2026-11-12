using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace KursoviyProjectHirich
{
    public partial class User_control : Window
    {
        public User_control()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = LoginEmailTxt.Text.Trim();
            string pass = LoginPasswordTxt.Password.Trim();

            int k = 1;

            if (email.Length < 5)
            {
                LoginEmailTxt.ToolTip = "Це поле введено невірно!";
                LoginEmailTxt.Background = Brushes.LightPink;
                k = 0;
            }
            else
            {
                LoginEmailTxt.ToolTip = "";
                LoginEmailTxt.Background = Brushes.Transparent;
            }

            if (pass.Length < 5)
            {
                LoginPasswordTxt.ToolTip = "Це поле введено невірно!";
                LoginPasswordTxt.Background = Brushes.LightPink;
                k = 0;
            }
            else
            {
                LoginPasswordTxt.ToolTip = "";
                LoginPasswordTxt.Background = Brushes.Transparent;
            }

            if (k == 1)
            {
                User authUser = Authenticate(email, pass);

                if (authUser != null)
                {
                    UserSession.CurrentUserId = authUser.Id;

                    MessageBox.Show("Все добре!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                    MainWindow mainWindow = new MainWindow();

                    Application.Current.MainWindow = mainWindow;

                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не авторизовано. Щось ви ввели не так!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Register registerWindow = new Register();
            registerWindow.Show();
            this.Close();
        }
    }
}