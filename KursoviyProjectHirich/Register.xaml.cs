using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace KursoviyProjectHirich
{
    public partial class Register : Window
    {
        private DataContext db;

        public Register()
        {
            InitializeComponent();
            db = new DataContext();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string name = RegNameTxt.Text.Trim();
            string email = RegEmailTxt.Text.Trim().ToLower();
            string pass = RegPasswordTxt.Password.Trim();
            string pass_2 = RegConfirmPasswordTxt.Password.Trim();

            int a = 1;

            if (name.Length < 3)
            {
                RegNameTxt.ToolTip = "Це поле введено невірно!";
                RegNameTxt.Background = Brushes.LightPink;
                a = 0;
            }
            else
            {
                RegNameTxt.ToolTip = "";
                RegNameTxt.Background = Brushes.Transparent;
            }

            if (email.Length < 5 || !email.Contains("@") || !email.Contains("."))
            {
                RegEmailTxt.ToolTip = "Це поле введено невірно!";
                RegEmailTxt.Background = Brushes.LightPink;
                a = 0;
            }
            else
            {
                RegEmailTxt.ToolTip = "";
                RegEmailTxt.Background = Brushes.Transparent;
            }

            if (pass.Length < 5)
            {
                RegPasswordTxt.ToolTip = "Це поле введено невірно!";
                RegPasswordTxt.Background = Brushes.LightPink;
                a = 0;
            }
            else
            {
                RegPasswordTxt.ToolTip = "";
                RegPasswordTxt.Background = Brushes.Transparent;
            }

            if (pass != pass_2)
            {
                RegConfirmPasswordTxt.ToolTip = "Паролі не співпадають!";
                RegConfirmPasswordTxt.Background = Brushes.LightPink;
                a = 0;
            }
            else
            {
                RegConfirmPasswordTxt.ToolTip = "";
                RegConfirmPasswordTxt.Background = Brushes.Transparent;
            }

            if (a == 1)
            {
                if (IsUserExists(email))
                {
                    MessageBox.Show("Користувач з таким Email вже існує!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                RegisterUser(name, email, pass);

                MessageBox.Show("Все добре!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                User_control authWindow = new User_control();
                authWindow.Show();
                this.Close();
            }
        }

        private void BtnBackToLogin_Click(object sender, RoutedEventArgs e)
        {
            User_control authWindow = new User_control();
            authWindow.Show();
            this.Close();
        }
    }
}