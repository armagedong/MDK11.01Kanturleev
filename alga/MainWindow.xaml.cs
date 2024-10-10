using alga.DB;
using Npgsql;
using System.Data;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace alga
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataBase dbHelper = new DataBase();
        public MainWindow()
        {
            InitializeComponent();
            List<string> tables = dbHelper.GetTables();
            tableComboBox.ItemsSource = tables;
            //ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ
        }
        private void TableComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedTable = tableComboBox.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedTable))
            {
                try
                {

                    using (var connection = new NpgsqlConnection(dbHelper.connectionString))
                    {
                        connection.Open();
                        string sql = $"SELECT * FROM {selectedTable}";
                        NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sql, connection);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        dataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage.Text = "Ошибка при загрузке данных таблицы: " + ex.Message;
                }
            }
        }
        private void AddField_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable table = ((DataView)dataGrid.ItemsSource).Table;
                DataRow newRow = table.NewRow();
                table.Rows.Add(newRow);
            }
            catch (Exception ex)
            {
                errorMessage.Text = "Ошибка при добавлении поля: " + ex.Message;
            }
        }
        private void DeleteField_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedRow = dataGrid.SelectedItem as DataRowView;
                if (selectedRow != null)
                {
                    selectedRow.Delete();
                }
            }
            catch (Exception ex)
            {
                errorMessage.Text = "Ошибка при удалении поля: " + ex.Message;
            }
        }
        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            string selectedTable = tableComboBox.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedTable))
            {
                try
                {
                    using (var connection = new NpgsqlConnection(dbHelper.connectionString))
                    {
                        connection.Open();
                        DataTable table = ((DataView)dataGrid.ItemsSource).Table;
                        NpgsqlDataAdapter adapter = new NpgsqlDataAdapter($"SELECT * FROM {selectedTable}", connection);
                        NpgsqlCommandBuilder commandBuilder = new NpgsqlCommandBuilder(adapter);
                        adapter.Update(table);
                        table.AcceptChanges();
                    }
                }
                catch (Exception ex)
                {
                    errorMessage.Text = "Ошибка при сохранении изменений: " + ex.Message;
                }
            }
        }
        private void LoadTables()
        {
            try
            {
                using (var connection = new NpgsqlConnection(dbHelper.connectionString))
                {
                    connection.Open();
                    string sql = "SELECT table_name FROM information_schema.tables WHERE table_schema='public';";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tableComboBox.Items.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage.Text = "Ошибка при загрузке таблиц: " + ex.Message;
            }
        }
        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var editedRow = e.Row.Item as DataRowView;
            var editedColumn = e.Column.Header.ToString();
            var newValue = (e.EditingElement as TextBox).Text;

            errorMessage.Text = $"Изменено значение в колонке: {editedColumn}, новое значение: {newValue}";

        }
        private void Out_Click(object sender, RoutedEventArgs e)
        {
            authorization auth = new authorization();
            auth.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                this.IsEnabled = false;
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    pd.PrintVisual(dataGrid, "Отчет");
                }
            }
            finally
            {
                this.IsEnabled = true;
            }

        }
    }
}