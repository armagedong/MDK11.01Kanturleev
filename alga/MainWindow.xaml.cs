using alga.DB;
using Microsoft.Win32;
using Npgsql;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
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
        }
        private void TableComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? selectedTable = tableComboBox.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedTable))
            {
                try
                {
                    string sql;
                    switch (selectedTable)
                    {
                        case "orders":
                            sql = $" SELECT o.order_id,s.name AS supplier_id,e.name AS employee_id,o.order_date,o.status FROM Orders o JOIN Suppliers s ON o.supplier_id = s.supplier_id join employees e On o.employee_id = e.employee_id";
                            break;
                        case "crops":
                            sql = $"Select * from crops";
                            break;
                        case "inventory":
                            sql = $"Select * from inventory";
                            break;
                        case "fields":
                            sql = $"SELECT f.field_id, f.location,f.size,f.soil_type,c.name AS crop_id FROM fields f JOIN crops c ON f.crop_id = c.crop_id";
                            break;
                        case "harvest":
                            sql = $"SELECT h.harvest_id,c.name AS crop_id,f.location as fields_id, h.harvest_amount,h.harvest_date FROM harvest h JOIN crops c ON h.crop_id = c.crop_id join fields f on h.field_id = f.field_id";
                            break;
                        default:
                            sql = $"Select * from {selectedTable}";
                            break;
                    }

                    using (var connection = new NpgsqlConnection(dbHelper.connectionString))
                    {
                        connection.Open();
                        
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
                DataTable? table = ((DataView)dataGrid.ItemsSource).Table;
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
            MessageBoxResult messageBoxResult = MessageBox.Show("Вы действительно хотите выйти?", "", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Authorization auth = new Authorization();
                auth.Show();
                this.Close();
            }
            
        
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

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            this.dataGrid.SelectAllCells();
            this.dataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, this.dataGrid);
            this.dataGrid.UnselectAllCells();

            String result = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
            try
            {
                StreamWriter sw = new StreamWriter("WpfData.csv");
                sw.WriteLine(result);
                sw.Close();
                Process.Start("WpfData.csv");
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application excelApp = new Excel.Application();
            excelApp.Visible = false;

            Excel.Workbook workbook = excelApp.Workbooks.Add(Type.Missing);
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];
            worksheet.Name = "DataGrid Data";
            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                worksheet.Cells[1, i + 1] = dataGrid.Columns[i].Header.ToString();
            }
            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                for (int j = 0; j < dataGrid.Columns.Count; j++)
                {
                    if (dataGrid.Columns[j].GetCellContent(dataGrid.Items[i]) is TextBlock cellContent)
                    {
                        worksheet.Cells[i + 2, j + 1] = cellContent.Text;
                    }
                }
            }
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "DataGridExport.xlsx"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                workbook.SaveAs(saveFileDialog.FileName);
                workbook.Close();
                excelApp.Quit();
                MessageBox.Show("Данные успешно экспортированы в Excel файл.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            Marshal.ReleaseComObject(worksheet);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(excelApp);
        }

        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                Excel.Application excelApp = new Excel.Application();
                Excel.Workbook workbook = excelApp.Workbooks.Open(filePath);
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];
                Excel.Range excelRange = worksheet.UsedRange;
                DataTable dataTable = new DataTable();
                for (int i = 1; i <= excelRange.Columns.Count; i++)
                {
                    dataTable.Columns.Add(excelRange.Cells[1, i].Value2.ToString());
                }
                for (int i = 2; i <= excelRange.Rows.Count; i++)
                {
                    DataRow row = dataTable.NewRow();
                    for (int j = 1; j <= excelRange.Columns.Count; j++)
                    {
                        row[j - 1] = excelRange.Cells[i, j].Value2?.ToString() ?? string.Empty;
                    }
                    dataTable.Rows.Add(row);
                }
                dataGrid.ItemsSource = dataTable.DefaultView;
                workbook.Close(false);
                excelApp.Quit();

                Marshal.ReleaseComObject(excelRange);
                Marshal.ReleaseComObject(worksheet);
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excelApp);
            }
        }
    }
}