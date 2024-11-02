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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace alga.DB
{
    /// <summary>
    /// Логика взаимодействия для CapchaWindow.xaml
    /// </summary>
    public partial class CapchaWindow : UserControl
    {
        private string captchaText;
        private Capcha captchaGenerator = new Capcha();

        public CapchaWindow()
        {
            InitializeComponent();
            RefreshCaptcha();
        }

        private void RefreshCaptcha()
        {
            BitmapImage captchaImage = captchaGenerator.GenerateCaptchaBitmapImage(out captchaText);
            img.Source = captchaImage;
        }

        private void VerifyCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (CaptchaInput.Text == captchaText)
            {
                MessageBox.Show("Captcha verified!");
            }
            else
            {
                MessageBox.Show("Incorrect captcha. Try again.");
                RefreshCaptcha();
            }
        }
    }
}
