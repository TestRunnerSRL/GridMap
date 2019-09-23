using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace GridMap
{
    public class JsonScreen
    {
        public string URL { get; set; }
    }

    public class Screen : BindableBase
    {
        private static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        static Screen()
        {
            if (!Directory.Exists(".\\Images"))
                Directory.CreateDirectory(".\\Images");
        }

        private static string CopyFileToLocal(string file)
        {
            if (file == null || !File.Exists(file))
            {
                return null;
            }

            var abs_path = Path.GetFullPath(file);
            var hash = CalculateMD5(abs_path);
            var file_name = $"{hash}{Path.GetExtension(file)}";
            var new_path = Path.Combine(".\\Images\\", file_name);

            if (!File.Exists(new_path))
            {
                File.Copy(abs_path, new_path);
            }

            return file_name;
        }

        public string URL
        {
            get {
                if (_url == null) { return null; }
                return Path.Combine(".\\Images\\", _url);
            }
            set {
                string file = null;
                try
                {
                    file = CopyFileToLocal(value);
                }
                catch (Exception)
                {
                }

                SetProperty(ref _url, file, new List<string>() { "ShowLabel" });
            }
        }
        private string _url;

        public Visibility ShowLabel
        {
            get
            {
                if (_url == null || _isHover)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
        }

        public bool ImageLoaded
        {
            get { return _imageLoaded; }
            private set { _imageLoaded = value; }
        }
        private bool _imageLoaded = false;

        public bool IsHover
        {
            get { return _isHover; }
            set { SetProperty(ref _isHover, value, new List<string>() { "ShowLabel" }); }
        }
        private bool _isHover = false;

        private static System.Windows.Media.Brush _dragColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF));

        public System.Windows.Media.Brush DragColor
        {
            get
            {
                if (_isDrag)
                {
                    return _dragColor;
                }
                else
                {
                    return System.Windows.Media.Brushes.Transparent;
                }
            }
        }

        public bool IsDrag
        {
            get { return _isDrag; }
            set { SetProperty(ref _isDrag, value, new List<string>() { "DragColor" }); }
        }
        private bool _isDrag = false;

        public int X
        {
            get { return _x; }
            set { SetProperty(ref _x, value, new List<string>() { "Coordinates" }); }
        }
        private int _x = -1;

        public int Y
        {
            get { return _y; }
            set { SetProperty(ref _y, value, new List<string>() { "Coordinates" }); }
        }
        private int _y = -1;

        public string Coordinates
        {
            get
            {
                int dividend = _x + 1;
                string columnName = String.Empty;
                int modulo;

                while (dividend > 0)
                {
                    modulo = (dividend - 1) % 26;
                    columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                    dividend = (int)((dividend - modulo) / 26);
                }

                return $"{columnName}{_y + 1}";
            }
        }

        public Screen(PropertyChangedEventHandler property_changed, int x, int y, string s = null) {
            _x = x;
            _y = y;
            _url = s;
            PropertyChanged += property_changed;
        }

        public Screen(PropertyChangedEventHandler property_changed, JsonScreen screen)
        {
            this._url = screen.URL;
            PropertyChanged += property_changed;
        }

        public JsonScreen GetJsonObject()
        {
            return new JsonScreen() { URL = _url };
        }
    }

    public class ScreenToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) { return null; }

            try
            {
                return new BitmapImage(new Uri(Path.GetFullPath(value as string)));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
