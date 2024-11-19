using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace alga.DB
{
    /// <summary>
    /// Логика взаимодействия для ImageListWindow.xaml
    /// </summary>
    public partial class ImageListWindow : Window
    {
        DataBase data = new DataBase();
        public class ImageItem
        {
            public int ImageId { get; set; }
            public string Name { get; set; }
            public byte[] ImgData { get; set; }
        }

        public BitmapImage SelectedImage { get; private set; }

        public ImageListWindow()
        {
            InitializeComponent();
            LoadImages();
        }

        private void LoadImages()
        {

            var images = new List<ImageItem>();

            using (var connection = new NpgsqlConnection(data.connectionString))
            {
                connection.Open();
                string query = "SELECT image_id, name, img FROM Image";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            images.Add(new ImageItem
                            {
                                ImageId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                ImgData = (byte[])reader["img"]
                            });
                        }
                    }
                }
            }

            ImageList.ItemsSource = images;
        }

        private void SelectImage(object sender, RoutedEventArgs e)
        {
            if (ImageList.SelectedItem is ImageItem selectedItem)
            {
                using (var ms = new MemoryStream(selectedItem.ImgData))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    SelectedImage = bitmap;
                }

                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please select an image.");
            }
        }
    }
}
