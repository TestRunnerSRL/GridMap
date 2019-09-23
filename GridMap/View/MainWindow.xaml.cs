using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GridMapViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel = new GridMapViewModel();

            btn_add_top.Click    += (s, a) => ViewModel.Screens.AddRow(Direction.Top);
            btn_add_bottom.Click += (s, a) => ViewModel.Screens.AddRow(Direction.Bottom);
            btn_add_left.Click   += (s, a) => ViewModel.Screens.AddRow(Direction.Left);
            btn_add_right.Click  += (s, a) => ViewModel.Screens.AddRow(Direction.Right);
            btn_del_top.Click    += (s, a) => ViewModel.Screens.DeleteRow(Direction.Top);
            btn_del_bottom.Click += (s, a) => ViewModel.Screens.DeleteRow(Direction.Bottom);
            btn_del_left.Click   += (s, a) => ViewModel.Screens.DeleteRow(Direction.Left);
            btn_del_right.Click  += (s, a) => ViewModel.Screens.DeleteRow(Direction.Right);

            //menuHostButton.Click  += (s, a) => ViewModel.Connection.Host(50000);
            //menuJoinButton.Click  += (s, a) => ViewModel.Connection.Join("localhost", 50000);
            //menuLeaveButton.Click += (s, a) => ViewModel.Connection.Leave();

            Logger.AddWindowHandler(this);
        }

        private object GetImage(DragEventArgs args)
        {
            var paths = args.Data.GetData(DataFormats.FileDrop);
            if (paths != null)
            {
                var imagepaths = paths as string[] ?? new string[] { };
                return imagepaths.FirstOrDefault((p) => new List<string> {
                    ".jpg",
                    ".jpeg",
                    ".png"
                }.Find((ext) => p.ToLower().EndsWith(ext)) != null);
            }

            return args.Data.GetData(typeof(Screen));
        }

        public void ImageDrop(object sender, DragEventArgs args)
        {
            var element = sender as FrameworkElement;
            var drop_obj = GetImage(args);

            if (drop_obj == null) { return; }

            Screen dest_screen = element.DataContext as Screen;

            if (drop_obj is string image_path)
            {
                dest_screen.URL = image_path;
                dest_screen.IsDrag = false;
            }

            if (drop_obj is Screen source_screen)
            {
                ViewModel.Screens.Swap(source_screen, dest_screen);

                source_screen.IsDrag = false;
                source_screen.IsHover = false;
                dest_screen.IsDrag = false;
                dest_screen.IsHover = false;
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            var element = sender as FrameworkElement;
            ((Screen)element.DataContext).IsHover = true;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            var element = sender as FrameworkElement;
            ((Screen)element.DataContext).IsHover = false;
        }

        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (GetImage(e) == null)
            {
                e.Effects = DragDropEffects.None;
                ((Screen)element.DataContext).IsDrag = false;
            }
            else
            {
                ((Screen)element.DataContext).IsDrag = true;
            }
            ((Screen)element.DataContext).IsHover = true;
        }

        private void Border_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            ((Screen)element.DataContext).IsDrag = false;
            ((Screen)element.DataContext).IsHover = false;
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var element = sender as FrameworkElement;
                DataObject data = new DataObject(typeof(Screen), ((Screen)element.DataContext));
                DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            }
        }

        private Point lastMousePos;

        private void MapGrid_MouseMove(object sender, MouseEventArgs e)
        {
            var viewer = sender as ScrollViewer;
            Point newMousePos = e.GetPosition(viewer);

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                viewer.ScrollToVerticalOffset(viewer.VerticalOffset + lastMousePos.Y - newMousePos.Y);
                viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset + lastMousePos.X - newMousePos.X);
            }

            lastMousePos = newMousePos;
        }

        private void ScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lastMousePos = e.GetPosition(sender as ScrollViewer);
        }

        private void MenuHostButton_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new HostWindow(ViewModel.Connection.config);
            configWindow.Owner = this;
            if (configWindow.ShowDialog() ?? false)
            {
                ViewModel.Connection.Host();
            }
        }

        private void MenuJoinButton_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new JoinWindow(ViewModel.Connection.config);
            configWindow.Owner = this;
            if (configWindow.ShowDialog() ?? false)
            {
                ViewModel.Connection.Join();
            }
        }

        private void MenuLeaveButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Connection.Leave();
        }
    }
}
