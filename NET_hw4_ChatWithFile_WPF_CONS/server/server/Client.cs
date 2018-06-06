using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class Client
    {
        public string Name { get; set; }
        public NetworkStream Stream { get; set; }
    }
}