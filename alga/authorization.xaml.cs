using Npgsql;
using QRCoder;
using QRCoder.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace alga
{
    /// <summary>
    /// Логика взаимодействия для authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        public string connectionString = "Host=localhost;Username=postgres;Password=root123;Database=alga_plus";
        const int maxAttempts = 3;
        int loginAttempts = 0;
        public Authorization()
        {
            InitializeComponent();
            QRCodeGenerator qr = new QRCodeGenerator();
            QRCodeData qrData = qr.CreateQrCode("https://www.youtube.com/watch?v=dQw4w9WgXcQ&ab_channel=RickAstley", QRCodeGenerator.ECCLevel.H);
            XamlQRCode qRCode = new XamlQRCode(qrData);
            DrawingImage qrCodeAsXaml = qRCode.GetGraphic(2);
            QRCode.Source = qrCodeAsXaml;


        }

        private void Reg_Click(object sender, RoutedEventArgs e)
        {
            Reg reg = new Reg();
            Close();
            reg.Show();
        }

        private void Enter_Clicl(object sender, RoutedEventArgs e)
        {
            string userInput = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                // Запрос для поиска пользователя по нику или почте
                string sql = "SELECT password, nickname FROM users WHERE email = @userInput OR nickname = @userInput";
                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@userInput", userInput);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string? hashedPasswordFromDb = reader["password"].ToString();
                            string? nickname = reader["nickname"].ToString();
                            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, hashedPasswordFromDb);

                            if (isPasswordCorrect)
                            {
                                MessageBox.Show($"Добро пожаловать, {nickname}!");
                                MainWindow mainWindow = new MainWindow();
                                mainWindow.Show();
                                loginAttempts = 0;
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Неверный пароль!");
                                loginAttempts++;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден!");
                        }
                    }
                }
            }
        }

        private void LoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
