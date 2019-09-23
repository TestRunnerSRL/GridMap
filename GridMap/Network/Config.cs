using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridMap
{
    public class Config : BindableBase
    {
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        private string _name = "user";

        public string IP
        {
            get { return _ip; }
            set { SetProperty(ref _ip, value); }
        }
        private string _ip = "localhost";

        public int Port
        {
            get { return _port; }
            set { SetProperty(ref _port, value); }
        }
        private int _port = 50000;
    }
}
