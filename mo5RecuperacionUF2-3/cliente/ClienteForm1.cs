using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cliente
{
    public partial class ClienteForm1 : Form
    {
        public static conexionDeRed conexionDeRed = new conexionDeRed();
        public static string IPADDRESS = "127.0.0.1";
        public const int PORT = 1982;



        public ClienteForm1()
        {
            InitializeComponent();
        }

        private void ClienteForm1_Load(object sender, EventArgs e)
        {
            conexionDeRed.OnDataRecieved += MensajeRecibido;

            if (!conexionDeRed.Connectar(IPADDRESS, PORT))
            {
                MessageBox.Show("Error conectando con el servidor!");

                return;
            }
        }

        private void ClienteForm1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MensajeRecibido(string datos)
        {
            var paquete = new paquete(datos);

            string comando = paquete.comando;

            if (comando == "resultado")
            {
                string contenido = paquete.contenido;

                Invoke(new Action(() => label1.Text = string.Format("Respuesta: {0}", contenido)));
            }
        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            if (conexionDeRed.TcpClient.Connected)
            {
                var msgPack = new paquete("login", string.Format("{0},{1}", textBox1.Text, textBox2.Text));
                conexionDeRed.EnviarPaquete(msgPack);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (conexionDeRed.TcpClient.Connected)
            {
                var msgPack = new paquete("insertar", string.Format("{0},{1}", textBox1.Text, textBox2.Text));

                conexionDeRed.EnviarPaquete(msgPack);
            }
        }
    }
}
