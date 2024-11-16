using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace Socket
{
    public class SocketCliente
    {
        public string IPRemota;
        public string PuertoRemoto;
        private static TcpClient TcpClient;
        private static NetworkStream NetworkStream;
        private Thread thread;
        public bool writeOnce = false;
        public bool onlySend = false;
        string fileJSON = "Barco.json";

        public event SeRecibieronDatosEventHandler DatosRecibidos;

        public delegate void SeRecibieronDatosEventHandler(string datos);

        public event DesconectadoEventHandler SocketDesconectado;

        public delegate void DesconectadoEventHandler();

        public bool Conectar()
        {
            if (TcpClient== null)
            {
                TcpClient = new TcpClient();
                TcpClient.Connect(IPRemota, int.Parse(PuertoRemoto));
                NetworkStream = TcpClient.GetStream();
                IPRemota = TcpClient.Client.RemoteEndPoint.ToString();
                
            }
            return TcpClient.Connected;
        }

        public void iniciarHilo()
        {
            thread = new Thread(EsperarDatos);
            thread.Start();
        }

        public void generarJSON(List<Datos> listaDatos)
        {
            if (Conectar())
            {
                string mJson = JsonConvert.SerializeObject(listaDatos, Formatting.Indented);

                if (File.Exists(fileJSON))
                {
                    File.Delete(fileJSON);
                }
                File.WriteAllText(fileJSON, mJson);
                writeOnce = true;
                iniciarHilo();
                //socket.LiberarTodo();
            }
        }

        public void EsperarDatos()
        {
            try
            {
                if (TcpClient != null && TcpClient.Connected)
                {
                    while(true)
                    {
                        NetworkStream = TcpClient.GetStream(); //porque obtiene otra vez el stream?
                        if (NetworkStream == null | TcpClient.Connected == false | !NetworkStream.CanRead)
                        {
                            break;
                        }

                        if(TcpClient.Connected && NetworkStream.CanWrite && writeOnce)
                        {

                            string json = File.ReadAllText("Barco.json", Encoding.UTF8);
                            byte[] datosAEnviar = Encoding.UTF8.GetBytes(json);
                            NetworkStream.Write(datosAEnviar, 0, datosAEnviar.Length);
                            writeOnce = false;
                            
                        }

                        if (TcpClient.Available > 0)
                        {
                            var mBytes = new byte[TcpClient.ReceiveBufferSize + 1];
                            if (NetworkStream.Read(mBytes, 0, TcpClient.ReceiveBufferSize) <= 0)
                            {
                                break;
                            }

                            string mDatosRecibidos = Encoding.ASCII.GetString(mBytes);
                            if (DatosRecibidos != null)
                                DatosRecibidos.Invoke(mDatosRecibidos);
                        }
                    }
                    if (SocketDesconectado != null)
                        SocketDesconectado.Invoke();
                    LiberarTodo();


                }
            }
            catch (Exception)
            {
                if (thread != null && thread.ThreadState == System.Threading.ThreadState.Running)
                {
                    if (SocketDesconectado != null)
                        SocketDesconectado.Invoke();
                    LiberarTodo();
                }
            }
        }

        public void LiberarTodo()
        {
            if (NetworkStream != null)
            {
                NetworkStream.Close();
                NetworkStream.Dispose();
                NetworkStream = null;
            }

            if (TcpClient != null)
            {
                if (TcpClient.Connected)
                {
                    TcpClient.Client.Shutdown(SocketShutdown.Both);
                    TcpClient.Client.Close();
                }

                TcpClient = null;
            }

            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }

        #region Metodo de envio

        public void EnviarMensaje(Datos mensaje)
        {
            if (NetworkStream != null && NetworkStream.CanWrite)
            {
                string serializacion = JsonConvert.SerializeObject(mensaje, Formatting.Indented);
                byte[] datos = Encoding.ASCII.GetBytes(serializacion);
                NetworkStream.Write(datos, 0, datos.Length);
            }
        }

        public void EnviarMensaje(string mensaje)
        {
            if (NetworkStream != null && NetworkStream.CanWrite)
            {
                byte[] datos = Encoding.ASCII.GetBytes(mensaje);
                NetworkStream.Write(datos, 0, datos.Length);
            }
        }

        #endregion
    }
}
