using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Newtonsoft.Json;


namespace GridMap
{
    public class GridMapViewModel : BindableBase
    {
        public ScreenGridViewModel Screens
        {
            get { return _screensViewModel; }
            set { SetProperty(ref _screensViewModel, value); }
        }
        private ScreenGrid _screens;
        private ScreenGridViewModel _screensViewModel;

        public float Zoom
        {
            get { return _zoom; }
            set { SetProperty(ref _zoom, value); }
        }
        private float _zoom = 1.0f;

        public Connection Connection {
            get { return _connection; }
            set { SetProperty(ref _connection, value); }
        }
        private Connection _connection = new Connection();

        public GridMapViewModel()
        {
            _screens = new ScreenGrid();
            _screensViewModel = _screens.GetViewModel();
        }
    }
}
