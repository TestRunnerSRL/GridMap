using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridMap
{
    public enum Direction { Top, Bottom, Left, Right }

    public class NotifyCollection2DChangedEventArgs : EventArgs
    {
        public enum NotifyCollection2DChangedAction { Add, Remove, Clear, Swap }
        public NotifyCollection2DChangedAction action;
        public Direction direction;
        public (int x, int y) pos1;
        public (int x, int y) pos2;

        public NotifyCollection2DChangedEventArgs(NotifyCollection2DChangedAction action)
        {
            if (action != NotifyCollection2DChangedAction.Clear)
            {
                throw new InvalidOperationException("Constructor supports only the 'Reset' action");
            }

            this.action = action;
        }
        public NotifyCollection2DChangedEventArgs(NotifyCollection2DChangedAction action, Direction direction)
        {
            if (action != NotifyCollection2DChangedAction.Add && action != NotifyCollection2DChangedAction.Remove)
            {
                throw new InvalidOperationException("Constructor supports only the 'Reset' action");
            }

            this.action = action;
            this.direction = direction;
        }
        public NotifyCollection2DChangedEventArgs(NotifyCollection2DChangedAction action, (int x, int y) pos1, (int x, int y) pos2)
        {
            if (action != NotifyCollection2DChangedAction.Swap)
            {
                throw new InvalidOperationException("Constructor supports only the 'Reset' action");
            }

            this.action = action;
            this.pos1 = pos1;
            this.pos2 = pos2;
        }
    }

    public delegate void NotifyCollection2DChangedEventHandler(object sender, NotifyCollection2DChangedEventArgs args);
}
