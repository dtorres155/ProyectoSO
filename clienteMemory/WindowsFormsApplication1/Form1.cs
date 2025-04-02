using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        List<string> conectados = new List<string>();
        Socket server;
        Thread atenderThread;
        bool escuchando = true;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            void AtenderServidor()
            {
                while (escuchando)
                {
                    try
                    {
                        byte[] buffer = new byte[512];
                        int bytesRecibidos = server.Receive(buffer);

                        // Si el servidor no envió datos, saltamos el bucle
                        if (bytesRecibidos == 0)
                        {
                            Console.WriteLine("⚠ El servidor no envió datos (0 bytes recibidos).");
                            continue;
                        }

                        string mensaje = Encoding.ASCII.GetString(buffer, 0, bytesRecibidos).Trim();

                        // 🔹 Depuración: Mostrar mensaje recibido
                        Console.WriteLine($"📩 Mensaje recibido: '{mensaje}' (Bytes: {bytesRecibidos})");

                        // Si el mensaje sigue vacío, el servidor podría estar enviando una cadena vacía
                        if (string.IsNullOrEmpty(mensaje))
                        {
                            Console.WriteLine("⚠ Advertencia: Mensaje recibido está vacío.");
                            continue;
                        }

                        // Si el mensaje es sobre conectados ("6/"), actualizamos la lista
                        string[] partes = mensaje.Split('/');
                        if (partes[0] == "6")
                        {
                            ActualizarListaConectados(partes);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Error al recibir datos: " + ex.Message);
                    }
                } }

        }

        

        private void ActualizarListaConectados(string[] partes)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ActualizarListaConectados(partes)));
            }
            else
            {
                conectados.Clear();

                int numConectados = int.Parse(partes[1]);
                for (int i = 2; i < 2 + numConectados; i++)
                {
                    conectados.Add(partes[i]);
                }

                // Actualizar DataGridView
                conectadosGrid.RowCount = conectados.Count;
                for (int i = 0; i < conectados.Count; i++)
                {
                    conectadosGrid.Rows[i].Cells[0].Value = conectados[i];
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //Creamos un IPEndPoint con el ip del servidor y puerto del servidor 
            //al que deseamos conectarnos
            IPAddress direc = IPAddress.Parse(IPBox.Text);
            IPEndPoint ipep = new IPEndPoint(direc, Convert.ToInt32(PortBox.Text));
            

            //Creamos el socket 
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(ipep);//Intentamos conectar el socket
                this.BackColor = Color.Green;
                MessageBox.Show("Conectado");

            }
            catch (SocketException ex)
            {
                //Si hay excepcion imprimimos error y salimos del programa con return 
                MessageBox.Show("No he podido conectar con el servidor");
                return;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

                
                // Enviamos nombre y altura
                 string mensaje = "4/" + partida.Text;
                 // Enviamos al servidor el nombre tecleado
                 byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                 server.Send(msg);

                 //Recibimos la respuesta del servidor
                 byte[] msg2 = new byte[80];
                 server.Receive(msg2);
                 mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
                if (mensaje == "-1")
                MessageBox.Show("No hemos encontrado esta partida");
                else
                MessageBox.Show("Esta partida es para"+mensaje+"jugadores");

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Mensaje de desconexión
            string mensaje = "0/";
        
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            // Nos desconectamos
            this.BackColor = Color.Gray;
            escuchando = false;
            server.Shutdown(SocketShutdown.Both);
            server.Close();
            // Detener el hilo de escucha
            if (atenderThread != null && atenderThread.IsAlive)
            {
                atenderThread.Abort();
            }

        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            //Mensaje de login
            string mensaje = "1/" + UserBox.Text + "/" + PasswordBox.Text;

            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            byte[] msg2 = new byte[80];
            server.Receive(msg2);
            mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
            if (mensaje == "1")
                MessageBox.Show("Bienvenido");
            else
                MessageBox.Show("No hemos encontrado su usuario o ha fallado la contraseña.");
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            //Mensaje de login
            string mensaje = "2/" + UserBox.Text + "/" + PasswordBox.Text;

            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            byte[] msg2 = new byte[80];
            server.Receive(msg2);
            mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
            if (Convert.ToInt32(mensaje )== 1)
                MessageBox.Show("Se ha registrado correctamente");
            else
                MessageBox.Show("Usuario ya registrado anteriormente");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string mensaje = "3/" + UserBox.Text ;

            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            byte[] msg2 = new byte[80];
            server.Receive(msg2);
            mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
            if (mensaje == "1")
                MessageBox.Show("Todavía no hay un tiempo registrado");
            else
                MessageBox.Show(mensaje);

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ModificarButton_Click(object sender, EventArgs e)
        {
            string mensaje = "5/" + UserBox.Text + "/" + nombre.Text + "/" + edad.Text;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            byte[] msg2 = new byte[80];
            server.Receive(msg2);
            mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
            if (mensaje == "1")
                MessageBox.Show("Se ha modificado correctamente");
            else
                MessageBox.Show("No se ha podido modificar");
        }

        private void MostrarConectados_Click(object sender, EventArgs e)
        {

            string mensaje = "6/";
            byte[] msg = Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            byte[] buffer = new byte[512]; // Asegurar tamaño suficiente
            int bytesRecibidos = server.Receive(buffer);

            mensaje = Encoding.ASCII.GetString(buffer, 0, bytesRecibidos);

            // 🔹 Depuración
            MessageBox.Show($"Mensaje recibido del servidor: '{mensaje}'");

            if (string.IsNullOrEmpty(mensaje) || mensaje == "No hay jugadores conectados.")
            {
                MessageBox.Show("No hay jugadores conectados en este momento.");
                return;
            }

            // Procesar lista de jugadores
            string[] partes = mensaje.Split('/');
            int numConectados = int.Parse(partes[0]);

            conectados.Clear();
            for (int i = 1; i <= numConectados; i++)
            {
                conectados.Add(partes[i]);
            }

            // Mostrar en DataGridView
            conectadosGrid.RowCount = conectados.Count;
            for (int i = 0; i < conectados.Count; i++)
            {
                conectadosGrid.Rows[i].Cells[0].Value = conectados[i];
            }
        }
    }
}
