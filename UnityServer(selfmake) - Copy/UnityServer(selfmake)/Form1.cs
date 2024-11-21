using Microsoft.VisualBasic.ApplicationServices;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityServer_selfmake_
{

    public partial class Form1 : Form
    {
        // management attribute
        int playerNum= 0;
        int playerUdpPort = 11000;
        public Dictionary<string, (TcpClient tcpClient, UdpClient udpClient)> users = new Dictionary<string, (TcpClient, UdpClient)>();
        // make a list for all udp client and it contains ID and sequeceNumber (ID == playerUdpPort)
        // connection attribuet
        private TcpListener tcpListener;
        private UdpClient udpListener;
        private int tcpPort = 10000;
        private int udpPort = 10001;
        StreamReader sr;
        StreamWriter sw;
        bool isRunning=false;
        int mess_num = 0;
        enum networkSubheader { PASS , CURRENT, JOIN, SPEAK , EXIT,WIN }
        float goal;
        public Form1()
        {
            InitializeComponent();
        } 

        private void Form1_Load(object sender, EventArgs e)
        {
        }
        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        private async Task StartServerAsync()
        {
            tcpListener = new TcpListener(IPAddress.Any,10000);
            IPEndPoint udpEndpoint = new IPEndPoint(IPAddress.Any, 10001);
            udpListener = new UdpClient(udpEndpoint);
            tcpListener.Start();
            richTextBox_notification.Text += $"Server started on port {tcpPort}\n";
            // Start listening for TCP clients
            ListenForTcpClientsAsync();
            MessageBox.Show("call listen UDP");
            // Start listening for UDP data (position updates, etc.)
            await ListenForUdpDataAsync();
            
        }

        private async Task ListenForTcpClientsAsync()
        {
            while (true)
            {
                try
                {
                    TcpClient Client = new TcpClient();
                    Client  = await tcpListener.AcceptTcpClientAsync();
                    if (Client.Connected)
                    {
                        richTextBox_notification.Text += "a client has connect to sever \n";
                        HandleTcpClientAsync(Client); // ko dc de await vi se ko acept dc client moi
                    }
                    
                }
                catch (Exception exception)
                {
                    richTextBox_notification.Text += $"Error accepting TCP client: {exception.Message}\n";
                }
            }
        }

        private async Task HandleTcpClientAsync(TcpClient tcpClient)
        {
            //using (tcpClient)
            StreamReader reader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8);
            StreamWriter writer = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8) { AutoFlush = true };
            {
                string clientip = tcpClient.Client.RemoteEndPoint.ToString();
                string[] ip = clientip.Split(':');
                clientip = ip[0];
                //string clientip = tcpClient.Client.RemoteEndPoint.AddressFamily.ToString();//CHAM HOI????   
                // Đọc tin nhắn từ client
                string? hello_message = await reader.ReadLineAsync();
                // Kiểm tra định dạng gói tin hello_message
                if (hello_message.Contains("|")) //"{username}|{ip}"
                {
                    string[] parts = hello_message.Split('|');
                    string username = parts[0].Trim();
                    //string ipEndpoint = parts[1].Trim();
                    // Xác thực tên người dùng
                    string? clientId = AuthenticateUsername(username, clientip);
                    if (clientId != null)
                    {
                        playerNum++;
                        // Khởi tạo UdpClient cho client này (nếu cần)
                        UdpClient udpClient = new UdpClient();
                        // Lưu cả TcpClient và UdpClient vào dictionary
                        //users[clientId] = (tcpClient, udpClient);
                        users.Add(clientId, (tcpClient, udpClient));
                        await writer.WriteLineAsync($"{networkSubheader.PASS.ToString()}|{playerUdpPort+playerNum}");
                        richTextBox_notification.Text += ($"Client kết nối thành công với ID: {clientId}\n");
                        //NotifyNewClientOfExistingPlayers(tcpClient);
                        await SendClientListAsync(writer);
                        await NotifyAllClients(username,clientId,networkSubheader.JOIN);
                        while (tcpClient.Connected)
                        {
                            await ClientCheck(clientId,tcpClient);
                        }
                        richTextBox_notification.Text += $"{clientId} has been removed \n";
                    }
                    else
                    {
                        await writer.WriteLineAsync("ALREADY");
                        tcpClient.Close();
                    }
                }
            }
        }
        private string? AuthenticateUsername(string username, string ipEndpoint)
        {
            // Tạo clientId theo định dạng {username}|{IPEndPoint}
            string clientId = $"{username}|{ipEndpoint}";

            // Kiểm tra nếu username đã tồn tại trong dictionary
            foreach (var key in users.Keys)
            {
                if (key.StartsWith(username + "|"))
                {

                    return null; // Trả về null nếu trùng username
                }
            }
            return clientId;
        }

        private async Task ListenForUdpDataAsync()
        {
            while (isRunning)
            {
                UdpReceiveResult udpResult = await udpListener.ReceiveAsync();
                string message = Encoding.ASCII.GetString(udpResult.Buffer);
                string[] parts = message.Split('|');
                if (message == null)
                {
                    MessageBox.Show("no udpmessage");
                }
                else
                {
                    //int? index = Move(int.Parse(parts[0]), parts[1]);
                    //if (index != null)
                    //{
                    //   await NotifyAllClients(index,networkSubheader.WIN );
                    //}
                    BroadcastPositionUpdate(message);
                    //textBox_UDPreceiver.Text += "I";
                }
            }
        } //chua

        private void ProcessUdpMessage(string message, IPEndPoint remoteEndPoint)
        {
            string[] parts = message.Split(',');
            if (parts.Length == 4 &&
                parts[0] == "POS" &&
                float.TryParse(parts[1], out float x) &&
                float.TryParse(parts[2], out float y) &&
                float.TryParse(parts[3], out float z))
            {
                //BroadcastPositionUpdate(remoteEndPoint.ToString(), x, y, z);
            }
            else
            {
                richTextBox_notification.Text += $"Invalid UDP message format from {remoteEndPoint}: {message}\n";
            }
        } //chua

        private int? Move(int PlayerIndex, string pos)
        {
            //this.transform.position=  
            string[] parts = pos.Split(',');

            if (parts.Length == 3)
            {
                float x = float.Parse(parts[0].Trim());
                float y = float.Parse(parts[1].Trim());
                float z = float.Parse(parts[2].Trim());
               
                if(y>= goal)
                {
                    return PlayerIndex;
                }

              
               
            }
            else
            {
            }
            return null;
        }
        private void BroadcastPositionUpdate(string message)
        {
            string Broadcastmessage = message;
            byte[] data = Encoding.ASCII.GetBytes(Broadcastmessage);
            int index = 0;

            foreach (var entry in users )
            {
                if (entry.Value.udpClient != null /*&& !entry.Key.Contains(username)*/) // client has to listen to he same port!!
                {
                    ++index;
                    string[] parts = entry.Key.Split('|');
                    //IPAddress clientIP = new IPAddress();
                    IPEndPoint clientUdpEP = IPEndPoint.Parse($"{parts[1]}:{playerUdpPort+index}");
                    //richTextBox_notification.Text += $"{username}";
                    entry.Value.udpClient.Send(data, data.Length, clientUdpEP);
                }
            }
        }
        private async void button_LISTEN_Click(object sender, EventArgs e)
        {
            isRunning = true;
            await StartServerAsync();
            //TcpClient Listener =new
            //DisconnectClientCheck();
        }
       
        private async Task NotifyAllClients(string username, string clientId, networkSubheader subheader) 
        {
            
            string formattedMessage = $"{subheader}|{username}";
            foreach (var user in users) 
            { 
                if (user.Key != clientId) 
                {
                    StreamWriter writer = new StreamWriter(user.Value.tcpClient.GetStream(), Encoding.UTF8) { AutoFlush = true }; 
                    await writer.WriteLineAsync(formattedMessage); 
                } 
            } 
        }
        private async Task NotifyAllClients(int? playerIndex, networkSubheader subheader)
        {

            string formattedMessage = $"{subheader}|{playerIndex}";
            foreach (var user in users)
            {
                    StreamWriter writer = new StreamWriter(user.Value.tcpClient.GetStream(), Encoding.UTF8) { AutoFlush = true };
                    await writer.WriteLineAsync(formattedMessage);
            }
        }
        private async Task SendClientListAsync(StreamWriter writer)
        {
            var clientNames = new List<string>();

            foreach (var user in users)
            {
                // Extract the name part of the key (format: name|ip|desport)
                string[] keyParts = user.Key.Split('|');
                if (keyParts.Length > 0)
                {
                    string name = keyParts[0];
                    clientNames.Add(name);
                }
            }
            string clientListMessage = $"{networkSubheader.CURRENT}|{string.Join("|", clientNames)}";
            await writer.WriteLineAsync(clientListMessage);
            // dellete disconnect from the list
        }
        private async Task ClientCheck(string clientid,TcpClient tcp)
        {
            StreamReader streamReader = new StreamReader(tcp.GetStream());
            string? mess = await streamReader.ReadLineAsync();
            //richTextBox_notification.Text += mess;
            string[] parts = mess.Split('|');
            
            if (StripControlChars(parts[0]) == networkSubheader.EXIT.ToString()) //process when client disconnect
            {
                richTextBox_notification.Text += $"{parts[1]} has disconnected\n";
                tcp.Dispose();
                tcp.Close();
                users.Remove(clientid);
                await NotifyAllClients(parts[1], clientid, networkSubheader.EXIT);
            }
            else if(StripControlChars(parts[0]) == networkSubheader.SPEAK.ToString())
            {

            }
            

            //Console.WriteLine($"Client removed: {clientEndpoint}");
        }
        public string StripControlChars(string s)
        {
            return Regex.Replace(s, @"[^\x20-\x7F]", "");
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            tcpListener.Stop();
            udpListener.Close();
        }
    }
}