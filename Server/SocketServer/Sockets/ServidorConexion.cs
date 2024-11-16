using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.IO;
using BE;
using BL;

namespace Sockets
{
    public class ServidorConexion
    {
        public int PuertoEscucha;
        private TcpListener Escuchador;

        ClientesConectados clientesInterfaz = new ClientesConectados();

        private Thread threadJugadores;

        Thread threadEscuchador;

        private bool inicializacion = true;

        public event SeRecibieronDatosEventHandler SeRecibieronDatos;

        public delegate void SeRecibieronDatosEventHandler(string pDatos);

        int jugador;

        bool autorizarJugador;


        BLMetodos bl = new BLMetodos();

        public string[] ObtenerDireccionesLocales()
        {
            var mDireccionesIP = Dns.GetHostAddresses(Dns.GetHostName());
            string[] mDireccionesStr;
            mDireccionesStr = new string[mDireccionesIP.Length];
            for (int x = 0, loopTo = mDireccionesIP.Length - 1; x <= loopTo; x++)
                mDireccionesStr[x] = mDireccionesIP[x].ToString();


            return mDireccionesStr;
        }


        public void EscucharPuerto()
        {
            if (PuertoEscucha != 0)
            {
                try
                {
                    Escuchador = new TcpListener(PuertoEscucha);
                    Escuchador.Start();
                }
                catch
                {
                    throw;
                }

                threadJugadores = new Thread(EsperarDatos);
                threadJugadores.Start();
            }

        }

        public void EsperarDatos()
        {
            try
            {
                while (true)
                {
                    if (clientesInterfaz.Count() == 0)
                    {

                        Cliente cliente = new Cliente();
                        cliente.clienteTCP = Escuchador.AcceptTcpClient();
                        cliente.stream = cliente.clienteTCP.GetStream();
                        cliente.numCliente = 1;
                        cliente.miTurno = false;
                        clientesInterfaz.AgregarCliente(cliente);
                    }
                    else if (clientesInterfaz.Count() == 1)
                    {
                        Cliente cliente = new Cliente();
                        cliente.clienteTCP = Escuchador.AcceptTcpClient();
                        cliente.stream = cliente.clienteTCP.GetStream();
                        cliente.numCliente = 2;
                        cliente.miTurno = false;
                        clientesInterfaz.AgregarCliente(cliente);

                        //bl.eliminarDatosTabla();
                        break;
                    }
                }

                while (true)
                {
                    //if (networkStream == null | TcpClient1.Connected == false | !networkStream.CanRead)
                    //{
                    //    SeConectoCliente.Invoke("AAAA algo salió mal :((((");
                    //    break;
                    //}

                    //SeConectoCliente.Invoke(TcpClient.Available.ToString());
                    foreach (Cliente cliente in clientesInterfaz)
                    {
                        if (cliente.clienteTCP.Available > 0)
                        {
                            var mBytes = new byte[cliente.clienteTCP.ReceiveBufferSize + 1];
                            int bytesRead;

                            bytesRead = cliente.stream.Read(mBytes, 0, cliente.clienteTCP.ReceiveBufferSize);
                            

                            SeRecibieronDatos.Invoke(bytesRead.ToString());
                            if (bytesRead <= 0)
                            {
                                SeRecibieronDatos.Invoke("no tiene bytes para leer");
                                break;
                            }

                            string mDatosRecibidos = Encoding.UTF8.GetString(mBytes, 0, bytesRead);

                            ConexionLogicaDeJuego(cliente, mDatosRecibidos);
                        }

                        if (jugador == cliente.numCliente && autorizarJugador)
                        {
                            cliente.miTurno = !cliente.miTurno;
                            autorizarJugador = false;
                            //Enviar mensaje para activar boton e indicarturno de jugador correspondiente
                        }
                    }
                    inicializacion = false;

                }

                //LiberarCliente();
                //if (SeDesconectoCliente != null)
                //    SeDesconectoCliente.Invoke();

            }
            catch
            {
                if (threadJugadores != null && threadJugadores.ThreadState == ThreadState.Running)
                {
                    foreach (Cliente cliente in clientesInterfaz)
                    {
                        LiberarCliente(cliente.clienteTCP, cliente.stream);
                    }
                    
                    if (SeRecibieronDatos != null)
                        SeRecibieronDatos.Invoke("Error");
                    throw;
                }
            }
        }

        void ConexionLogicaDeJuego(Cliente cliente, string mDatosRecibidos)
        {
            if (inicializacion)
            {
                List<Datos> objetoRecibido = JsonConvert.DeserializeObject<List<Datos>>(mDatosRecibidos);
                //bl.pasarAInsertar(objetoRecibido);
                EnviarMensaje(cliente.clienteTCP, "Hola jugador" + cliente.numCliente);
                if (cliente.numCliente == 1)
                {
                    //Enviar mensaje para activar boton e indicarturno de jugador 1
                }
                else
                {
                    cliente.miTurno = true;
                }
            }
            else
            {
                if (cliente.numCliente == 1 && cliente.miTurno)
                {
                    jugador = 2;
                    cliente.miTurno = false;
                }
                else if (cliente.numCliente == 2 && cliente.miTurno)
                {
                    jugador = 1;
                    cliente.miTurno = false;
                }
                //Enviar mensaje de info de partida
                autorizarJugador = true;
            }
        }

        private void LiberarCliente(TcpClient cliente, NetworkStream networkStream)
        {
            if (cliente != null)
            {
                SeRecibieronDatos.Invoke("Se libero el cliente");
                if (networkStream != null)
                {
                    networkStream.Close();
                    networkStream.Dispose();
                    networkStream = null;
                }

                if (cliente.Connected)
                {
                    cliente.Client.Shutdown(SocketShutdown.Both);
                    cliente.Close();
                }

                cliente = null;
            }
        }

        #region metodos de envio
        public void EnviarMensaje(TcpClient cliente, Datos mensaje)
        {
            if (cliente != null && cliente.Connected)
            {
                NetworkStream stream = cliente.GetStream();
                if (stream.CanWrite)
                {
                    string serializacion = JsonConvert.SerializeObject(mensaje, Formatting.Indented);
                    byte[] datos = Encoding.ASCII.GetBytes(serializacion);
                    stream.Write(datos, 0, datos.Length);
                }
            }
        }

        public void EnviarMensaje(TcpClient cliente, string mensaje)
        {
            if (cliente != null && cliente.Connected)
            {
                NetworkStream stream = cliente.GetStream();
                if (stream.CanWrite)
                {
                    byte[] datos = Encoding.ASCII.GetBytes(mensaje);
                    stream.Write(datos, 0, datos.Length);
                }
            }
        }

        #endregion
    }
}
