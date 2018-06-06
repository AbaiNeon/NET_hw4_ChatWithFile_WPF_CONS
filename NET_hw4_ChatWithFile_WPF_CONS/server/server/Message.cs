using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace server
{
    public class Message
    {
        public string From { get; set; }
        public string Text { get; set; }
    }
}