using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Image = System.Drawing.Image;

namespace hw4_ChatWithFile_Client_WPF
{
    public partial class MainWindow : Window
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private string _serverIp; //127.0.0.1
        private int _port; //3535
        private string _nik;

        public MainWindow()
        {
            InitializeComponent();

            _client = null;
            _stream = null;
        }

        private void BtnSendClick(object sender, RoutedEventArgs e)
        {
            Send();
        }

        private void BtnConnectClick(object sender, RoutedEventArgs e)
        {
            _serverIp = txtBoxServerIP.Text;
            _port = Int32.Parse(txtBoxPort.Text);
            _nik = txtBpxNik.Text;

            _client = new TcpClient();
            _client.Connect(_serverIp, _port);

            _stream = _client.GetStream();

            Task receiveThread = new Task(() => ReceiveMessage());
            receiveThread.Start();
        }

        private void ReceiveMessage()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = _stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Default.GetString(data, 0, bytes));
                    }
                    while (_stream.DataAvailable);

                    string json = builder.ToString();
                    string json2 = "";
                    Message messageResp = null;

                    if (json.Contains("&image#"))
                    {
                        string imageString = json.Split(new[] { "&image#" }, StringSplitOptions.None)[1];

                        byte[] buffer = Encoding.Default.GetBytes(imageString);

                        using (Image image = Image.FromStream(new MemoryStream(buffer)))
                        {
                            image.Save("output.jpg", ImageFormat.Jpeg);  // Or Png
                        }

                        json2 = json.Split(new[] { "&image#" }, StringSplitOptions.None)[0];
                        messageResp = JsonConvert.DeserializeObject<Message>(json2);
                    }
                    else
                    {
                        messageResp = JsonConvert.DeserializeObject<Message>(json);
                    }
                    
                    Dispatcher.Invoke(() => { txtBoxChat.Text += messageResp.From + ": " + messageResp.Text + Environment.NewLine; });
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _stream.Close();
                _client.Close();
            }
        }

        private void Send()
        {
            try
            {
                Message newMessage = new Message() { From = txtBpxNik.Text, Text = txtBoxMyMsg.Text };
                string filePath = txtBoxFilePath.Text;
                string json = JsonConvert.SerializeObject(newMessage);
                string imageString = "";

                // если есть файл для передачи
                if ( !(String.IsNullOrEmpty(filePath)) )
                {
                    Image img = Image.FromFile(filePath);
                    byte[] arr;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        arr = ms.ToArray();
                    }
                    imageString = Encoding.Default.GetString(arr, 0, arr.Length);
                    imageString = "&image#" + imageString;
                }

                byte[] buffer = Encoding.Default.GetBytes(json + imageString);

                _stream.Write(buffer, 0, buffer.Length);
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void BtnDisconnectClick(object sender, RoutedEventArgs e)
        {
            _stream.Close();
            _client.Close();
        }

        private void BtnBrowseClick(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                txtBoxFilePath.Text = filename;
            }
        }
    }
}
