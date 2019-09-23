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
using System.Windows.Threading;

namespace GridMap
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        public NotificationWindow(Window owner, string title, string description = null)
        {
            Owner = owner;
            InitializeComponent();
            Title.Text = title ?? "";
            if (description == null)
            {
                Description.Visibility = Visibility.Collapsed;
            }
            else
            {
                Description.Text = description;
            }

            Left = Owner.Left + Owner.ActualWidth - ActualWidth - 20;
            Top = Owner.Top + Owner.ActualHeight - ActualHeight - 20;

            Owner.SizeChanged += OwnerResize;
            Owner.LocationChanged += OwnerResize;
            
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() => {
                Left = Owner.Left + Owner.ActualWidth - ActualWidth - 20;
                Top = Owner.Top + Owner.ActualHeight - ActualHeight - 20;
            }));

            ShowActivated = false;
            Focusable = false;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OwnerResize(object sender, EventArgs args)
        {
            this.Left = this.Owner.Left + this.Owner.ActualWidth - this.ActualWidth - 20;
            this.Top = this.Owner.Top + this.Owner.ActualHeight - this.ActualHeight - 20;
        }

        private void Grid_MouseEnter(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(this, "Opened", false);
        }

        private void Grid_MouseLeave(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(this, "Closing", false);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Opening", false);
        }
    }
}
