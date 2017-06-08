using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace servidor
{
    public partial class ServidorForm1 : Form
    {
        public delegate void ClientCarrier(conexionTCP conexionTcp);
        public event ClientCarrier OnClientConnected;
        public event ClientCarrier OnClientDisconnected;
        public delegate void DataRecived(conexionTCP conexionTcp, string data);
        public event DataRecived OnDataRecived;

        private TcpListener _tcpListener;
        private Thread _acceptedThread;
        private List<conexionTCP> connectedClients = new List<conexionTCP>();




        public ServidorForm1()
        {
            InitializeComponent();
        }

        private void ServidorForm1_Load(object sender, EventArgs e)
        {
            // TODO: esta línea de código carga datos en la tabla 'uNED.Usuarios' Puede moverla o quitarla según sea necesario.
            this.usuariosTableAdapter.Fill(this.uNED.Usuarios);
            OnDataRecived += MensajeRecibido;
            OnClientConnected += ConexionRecibida;
            OnClientDisconnected += ConexionCerrada;

            EscucharClientes("127.0.0.1",1982);
        }
        private void MensajeRecibido(conexionTCP conexionTcp, string datos)
        {
            var paquete = new paquete(datos);
            string comando = paquete.comando;
            if (comando == "login")
            {
                string contenido = paquete.contenido;
                List<string> valores = mapa.Deserializar(contenido);

                Invoke(new Action(() => textBox1.Text = valores[0]));
                Invoke(new Action(() => textBox2.Text = valores[1]));

                var msgPack = new paquete("resultado", "OK");
                conexionTcp.EnviarPaquete(msgPack);
            }

            if (comando == "insertar")
            {
                string contenido = paquete.contenido;
                List<string> valores = mapa.Deserializar(contenido);
                usuariosTableAdapter.Insert(valores[0], valores[1]);
                var msgPack = new paquete("resultado","Registros en SQL: OK");
                conexionTcp.EnviarPaquete(msgPack);
            }

        }
        private void ConexionRecibida(conexionTCP conexionTCP)
        {
            lock (connectedClients)
                if (!connectedClients.Contains(conexionTCP))
                    connectedClients.Add(conexionTCP);
            Invoke(new Action(() => label1.Text = string.Format("Clientes: {0}", connectedClients.Count)));
        }

        private void ConexionCerrada(conexionTCP conexionTCP)
        {
            lock (connectedClients)
                if (connectedClients.Contains(conexionTCP))
                {
                    int cliIndex = connectedClients.IndexOf(conexionTCP);
                    connectedClients.RemoveAt(cliIndex);
                }
            Invoke(new Action(()=> label1.Text=string.Format("Clientes: {0}", connectedClients.Count)));
        }
        private void EscucharClientes(string ipAddress, int port)
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
                _tcpListener.Start();
                _acceptedThread = new Thread(AceptarClientes);
                _acceptedThread.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
            }
        }
        private void AceptarClientes()
        {
            do
            {
                try
                {
                    var conexion = _tcpListener.AcceptTcpClient();
                    var srvClient = new conexionTCP(conexion)
                    {
                        ReadThread = new Thread(LeerDatos)
                    };

                    if (OnClientConnected != null)
                        OnClientConnected(srvClient);
                }
                catch (Exception e)
                {

                    MessageBox.Show(e.Message.ToString());
                }
                
            } while (true);
        }

        private void LeerDatos(object client)
        {
            var cli = client as conexionTCP;
            var charBuffer = new List<int>();

            do
            {
                try
                {
                    if (cli == null)
                        break;
                    if (cli.StreamReader.EndOfStream)
                        break;
                    int charCode = cli.StreamReader.Read();
                    if (charCode == -1)
                        break;
                    if (charCode != 0)
                    {
                        charBuffer.Add(charCode);
                        continue;
                    }
                    if (OnDataRecived != null)
                    {
                        var chars = new char[charBuffer.Count];
                        for (int i = 0; i < charBuffer.Count; i++)
                        {
                            chars[i] = Convert.ToChar(charBuffer[i]);
                        }
                        var message = new string(chars);
                        OnDataRecived(cli, message);
                    }
                    charBuffer.Clear();
                }
                catch (IOException)
                {

                    break;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString());
                    break;
                }
            } while (true);

            if (OnClientDisconnected != null)
                OnClientDisconnected(cli);
        }

        private void ServidorForm1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void usuariosBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.usuariosBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.uNED);

        }

        private void Actualizar_Click(object sender, EventArgs e)
        {
            this.usuariosTableAdapter.Fill(this.uNED.Usuarios);
        }
    }
}
