using alga.DB;
using Npgsql;
using System.Data;
using System.Windows;
using BCrypt.Net;
using System.Windows.Controls;

namespace alga
{
    /// <summary>
    /// Логика взаимодействия для Reg.xaml
    /// </summary>
    public partial class Reg : Window
    {
        DataBase dbHelper = new DataBase();
        public Reg()
        {
            InitializeComponent();
            
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Authorization auth = new Authorization();
            auth.Show();
            Close();
        }

        private void Reg_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim().ToString();
            string login = UsernameTextBox.Text.Trim().ToString();
            string password = PasswordTextBox.Password.Trim().ToString();
            string email = EmailTextBox.Text.Trim().ToString();

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            using (var connection = new NpgsqlConnection(dbHelper.connectionString))
            {
                connection.Open();
                string sql = "INSERT INTO users (login, password, nickname, email) VALUES (@login, @password, @nickname, @email)";
                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", hashedPassword); 
                    cmd.Parameters.AddWithValue("@nickname", username);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
