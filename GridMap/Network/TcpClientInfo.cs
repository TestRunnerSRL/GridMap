using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static GridMap.Connection;

namespace GridMap
{
    public class TcpClientInfo
    {
        public TcpClient tcpclient;
        public string user;
        public ConnectionStatus status;
        public DateTime lastping;

        public TcpClientInfo(TcpClient client)
        {
            this.tcpclient = client;
            this.user = null;
            this.status = ConnectionStatus.JOINED;
            this.lastping = DateTime.Now;
        }
    }
}
