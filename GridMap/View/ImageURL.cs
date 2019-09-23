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

namespace GridMap
{
    public class ImageURL : Image
    {
        public string URL
        {
            get
            {
                return (string)GetValue(URLProperty);
            }
            set
            {
                try
                {
                    Source = new BitmapImage(new Uri(value));
                    SetValue(URLProperty, value);
                }
                catch (UriFormatException)
                {
                    Source = null;
                    SetValue(URLProperty, null);
                }
            }
        }
        public static readonly DependencyProperty URLProperty = DependencyProperty.Register("URL", typeof(string), typeof(ImageURL), new PropertyMetadata(null));
        private string _url = null;

        static ImageURL()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageURL), new FrameworkPropertyMetadata(typeof(ImageURL)));
        }
    }
}
