using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GridMap.NotifyCollection2DChangedEventArgs;

namespace GridMap
{
    public class ScreenGridViewModel : IEnumerable<ObservableCollection<Screen>>, INotifyCollectionChanged
    {
        private ScreenGrid screenGrid;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event NotifyCollection2DChangedEventHandler Collection2DChanged;

        public Screen this[int i, int j]
        {
            get
            {
                return screenGrid[i, j];
            }
            set
            {
                screenGrid[i, j] = value;
            }
        }

        public ScreenGridViewModel(ScreenGrid screenGrid)
        {
            this.screenGrid = screenGrid;
            screenGrid.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        public void AddRow(Direction direction)
        {
            screenGrid.AddRow(direction);
            Collection2DChanged?.Invoke(this, new NotifyCollection2DChangedEventArgs(NotifyCollection2DChangedAction.Add, direction));
        }

        public void DeleteRow(Direction direction)
        {
            screenGrid.DeleteRow(direction);
            Collection2DChanged?.Invoke(this, new NotifyCollection2DChangedEventArgs(NotifyCollection2DChangedAction.Remove, direction));
        }

        public void Swap(Screen screen1, Screen screen2)
        {
            screenGrid.Swap(screen1, screen2);
            Collection2DChanged?.Invoke(this, new NotifyCollection2DChangedEventArgs(NotifyCollection2DChangedAction.Swap, (screen1.X, screen1.Y), (screen2.X, screen2.Y)));
        }

        public IEnumerator<ObservableCollection<Screen>> GetEnumerator()
        {
            return screenGrid.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return screenGrid.GetEnumerator();
        }

        public IEnumerator<Screen> GetEnumeratorAll()
        {
            return screenGrid.GetEnumeratorAll();
        }
    }
}
