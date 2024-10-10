using Npgsql;
using QRCoder;
using QRCoder.Xaml;
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

namespace alga
{
    /// <summary>
    /// Логика взаимодействия для authorization.xaml
    /// </summary>
    public partial class authorization : Window
    {
        public string connectionString = "Host=localhost;Username=postgres;Password=root123;Database=alga_plus";
        public authorization()
        {
            InitializeComponent();
            QRCodeGenerator qr = new QRCodeGenerator();
            QRCodeData qrData = qr.CreateQrCode("https://www.youtube.com/watch?v=dQw4w9WgXcQ&ab_channel=RickAstley", QRCodeGenerator.ECCLevel.H);
            XamlQRCode qRCode = new XamlQRCode(qrData);
            DrawingImage qrCodeAsXaml = qRCode.GetGraphic(3);
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

            if (string.IsNullOrEmpty(userInput) || string.IsNullOrEmpty(password))
            {
                errorMessage.Text = "Пожалуйста, введите данные для авторизации.";
                return;
            }

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM users WHERE (email = @userInput OR login = @userInput) AND password = @password";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@userInput", userInput);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string nickname = reader["nickname"].ToString();
                                MessageBox.Show($"Добро пожаловать, {nickname}!");
                                MainWindow mainWindow = new MainWindow();
                                mainWindow.Show();
                                this.Close();
                            }
                            else
                            {
                                errorMessage.Text = "Неверный email/никнейм или пароль.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage.Text = "Ошибка при подключении к базе данных: " + ex.Message;
            }
        }
    }
}
