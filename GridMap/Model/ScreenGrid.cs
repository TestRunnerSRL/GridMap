using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GridMap.ScreenGrid;

namespace GridMap
{
    public class ScreenGrid : IEnumerable<ObservableCollection<Screen>>, INotifyCollectionChanged
    {
        private ObservableCollection<ObservableCollection<Screen>> screens;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event NotifyCollection2DChangedEventHandler Collection2DChanged;

        public ScreenGridViewModel GetViewModel()
        {
            var viewModel = new ScreenGridViewModel(this);
            viewModel.Collection2DChanged += Collection2DChanged;
            return viewModel;
        }

        public Screen this[int i, int j]
        {
            get
            {
                return screens[i][j];
            }
            set
            {
                screens[i][j] = value;
            }
        }

        public ScreenGrid()
        {
            LoadLayout();
            screens.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        private void SaveLayout()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            var JsonObject = new List<List<JsonScreen>>();
            foreach (var row in screens)
            {
                var new_row = new List<JsonScreen>();
                foreach (var screen in row)
                {
                    new_row.Add(screen.GetJsonObject());
                }
                JsonObject.Add(new_row);
            }

            using (var s = new StreamWriter(".\\config.json"))
            {
                serializer.Serialize(s, JsonObject);
            }
        }

        private void LoadLayout()
        {
            JsonSerializer serializer = new JsonSerializer();
            List<List<JsonScreen>> JsonObject;
            screens = new ObservableCollection<ObservableCollection<Screen>>();

            try
            {
                using (var s = new StreamReader(".\\config.json"))
                {
                    JsonObject = serializer.Deserialize(s, typeof(List<List<JsonScreen>>)) as List<List<JsonScreen>>;
                }

                int max_width = 0;
                foreach (var row in JsonObject)
                {
                    var new_row = new ObservableCollection<Screen>();
                    foreach (var screen in row)
                    {
                        new_row.Add(new Screen(OnScreensPropertyChange, screen));
                    }
                    screens.Add(new_row);

                    max_width = Math.Max(max_width, row.Count);
                }

                // Ensure that the grid has even rows by padding
                foreach(var row in screens)
                {
                    while (row.Count < max_width)
                    {
                        row.Add(new Screen(OnScreensPropertyChange, 0, 0));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                for (int i = 0; i < 8; i++)
                {
                    var new_row = new ObservableCollection<Screen>();
                    for (int j = 0; j < 8; j++)
                    {
                        new_row.Add(new Screen(OnScreensPropertyChange, i, j));
                    }
                    screens.Add(new_row);
                }
            }
            finally
            {
                UpdateScreenCoordinates();
            }
        }

        private void OnScreensPropertyChange(object sender, PropertyChangedEventArgs arg)
        {
            if (arg.PropertyName == "URL")
            {
                SaveLayout();
            }
        }

        private void UpdateScreenCoordinates()
        {
            foreach (var (x, row) in screens.Enumerate())
            {
                foreach (var (y, screen) in row.Enumerate())
                {
                    screen.X = x;
                    screen.Y = y;
                }
            }
        }

        public void AddRow(Direction direction)
        {
            ObservableCollection<Screen> new_row;
            switch (direction)
            {
                case Direction.Left:
                    new_row = new ObservableCollection<Screen>();
                    foreach (var (i, screen) in screens[0].Enumerate())
                    {
                        new_row.Add(new Screen(OnScreensPropertyChange, i, 0));
                    }
                    screens.Insert(0, new_row);
                    break;
                case Direction.Right:
                    new_row = new ObservableCollection<Screen>();
                    foreach (var (i, screen) in screens[0].Enumerate())
                    {
                        new_row.Add(new Screen(OnScreensPropertyChange, i, screens.Count));
                    }
                    screens.Add(new_row);
                    break;
                case Direction.Top:
                    foreach (var (i, row) in screens.Enumerate())
                    {
                        row.Insert(0, new Screen(OnScreensPropertyChange, 0, i));
                    }
                    break;
                case Direction.Bottom:
                    foreach (var (i, row) in screens.Enumerate())
                    {
                        row.Add(new Screen(OnScreensPropertyChange, row.Count, i));
                    }
                    break;
            }

            UpdateScreenCoordinates();
            SaveLayout();
        }

        public void DeleteRow(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    if (screens.Count <= 1) { return; }
                    screens.RemoveAt(0);
                    break;
                case Direction.Right:
                    if (screens.Count <= 1) { return; }
                    screens.RemoveAt(screens.Count - 1);
                    break;
                case Direction.Top:
                    if (screens[0].Count <= 1) { return; }
                    foreach (var row in screens)
                    {
                        row.RemoveAt(0);
                    }
                    break;
                case Direction.Bottom:
                    if (screens[0].Count <= 1) { return; }
                    foreach (var row in screens)
                    {
                        row.RemoveAt(row.Count - 1);
                    }
                    break;
            }

            UpdateScreenCoordinates();
            SaveLayout();
        }

        public void Swap(Screen screen1, Screen screen2)
        {
            this[screen1.X, screen1.Y] = screen2;
            this[screen2.X, screen2.Y] = screen1;

            UpdateScreenCoordinates();
            SaveLayout();
        }

        public IEnumerator<ObservableCollection<Screen>> GetEnumerator()
        {
            return screens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Screen> GetEnumeratorAll()
        {
            foreach(var row in screens)
            {
                foreach (var screen in row)
                {
                    yield return screen;
                }
            }
        }
    }
}
