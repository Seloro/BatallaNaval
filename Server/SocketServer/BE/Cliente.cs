using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Cliente
    {

        public TcpClient clienteTCP;

        public NetworkStream stream;

        public int numCliente;

        public bool miTurno;
    }
}
