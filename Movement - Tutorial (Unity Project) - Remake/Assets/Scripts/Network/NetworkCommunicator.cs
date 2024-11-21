using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Win32.SafeHandles;
using static UnityEngine.Networking.UnityWebRequest;



public class NetworkCommunicator : MonoBehaviour
{
    // SERVER'S CONNECTION ATTRIBUTE

    // server address
    public int tcpPort = 10000;  // Default port, you can set it in the Inspector
    public int udpPort = 10001;
    // public IPAddress serverIP = IPAddress.Parse("127.0.0.1");  // Server IP, change as needed
    [SerializeField] TMP_InputField ServerIP;
    // connection attribute                                                // public IPAddress machineIP= GetLocalIPAddress();
    public IPEndPoint TCPendPoint;
    public IPEndPoint UDPendPoint;
    public int timeout = 5000;  // Connection timeout in milliseconds


    // CLIENT'S CONNECTION ATTRIBUTE

    // connections
    public TcpClient tcpClient;
    public UdpClient udpClient_Send;
    public UdpClient udpClient_Recv;
    //client listen port for udp;
    private int udpListenPort; // SETUP LATER FOR LAN HOST!!!
    // streams
    public StreamReader sr;
    public StreamWriter sw;

    public BinaryWriter bw;
    public BinaryReader br;

    private NetworkStream stream;

    // variables
    string server_message;
    private bool isconnect = false;
    private bool isActive = false;
    private bool loadCurrent = false;
    // sub header for connection
    enum networkSubheader
    {
        PASS,
        ALREADY,
        CURRENT,
        JOIN,
        SPEAK,
        EXIT
    }
    // player object attribute
    // private string username; //get from input; // SETUP LATER FOR LAN HOST!!!
    [SerializeField] TMP_InputField Username;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    //public Transform lastTransform;
    [SerializeField] private Transform playerTransform;  // Assign this in the Unity Inspector to the player’s transform
    [SerializeField] private Transform orientationTransform; // to get the player's rotation
    //public spawn instance
    [SerializeField] public GameObject Player;
    [SerializeField] public GameObject Camera;
    //[SerializeField] public GameObject OtherP;
    // spawn list to spawn
    private List<string> playersToSpawn = new List<string>();
    public Spawning spawner;
    public string target = string.Empty;
    public int indexPlayer;
    //public game event
    public event EventHandler<OnReceiveServerMessageEventArgs> OnReceiveServerMessage;
    public class OnReceiveServerMessageEventArgs : EventArgs
    {
        public string message;
    }
    // thread
    private Thread UDPListen;

    private void Awake()
    {

    }
    private async void Start()
    {

        string username = Username.text;
        //username = UnityEngine.Random.Range(10, 100).ToString();

        await ConnectServer();
        if (isconnect)
        {
            Instantiate(Camera);
            Instantiate(Player);
            Camera.SetActive(true);
            Player.SetActive(true);
            Debug.Log("Spawn player");
        }
        if (playerTransform != null)
        {
            lastPosition = playerTransform.position;
            lastRotation = orientationTransform.rotation;
        }
        Debug.Log("done start");

        // Start listening for server messages in a separate task
        _ = Task.Run(() => ListenTcpFromServer());
        ListeningSyncData();


    }

    void DemoSpawn(string Pname)
    {
        string username = Username.text;
        if (Pname != username)
        {
            int index = spawner.SpawnPrefab(Pname) - 1;
            TextMeshProUGUI TMP = spawner.spawnedObjects[index].GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>();
            TMP.text = Pname;
            Debug.Log("Demo spawn: " + spawner.spawnedObjects[index] + " index = " + index);
        }
        else
        {
            indexPlayer = spawner.AddPlayer() - 1;
            Debug.Log("Demo spawn: " + spawner.spawnedObjects[indexPlayer] + " index = " + indexPlayer);
        }

    }
    private Vector3 Move(string pos)
    {
        //this.transform.position=  
        string[] parts = pos.Split(',');

        if (parts.Length == 3)
        {
            // Step 2: Parse each part to a float
            float x = float.Parse(parts[0].Trim());
            float y = float.Parse(parts[1].Trim());
            float z = float.Parse(parts[2].Trim());

            // Step 3: Create a Vector3 from the values
            Vector3 newPosition = new Vector3(x, y, z);
            //Rigidbody.position = newPosition;
            //Debug.Log($"Position updated to: {newPosition}");
            return newPosition;
        }
        else
        {
            Debug.LogError("Invalid position string format!");
            return Vector3.zero;
        }

    }
    async Task ConnectServer()
    {
        IPAddress serverIP = IPAddress.Parse(ServerIP.text);
        tcpClient = new TcpClient();
        udpClient_Send = new UdpClient();
        TCPendPoint = new IPEndPoint(serverIP, tcpPort);
        UDPendPoint = new IPEndPoint(serverIP, udpPort);
        // Set a connection timeout
        tcpClient.SendTimeout = timeout;
        tcpClient.ReceiveTimeout = timeout;
        try
        {
            // Attempt to connect
            //await tcpClient.ConnectAsync(serverIP, tcpPort);
            tcpClient.Connect(serverIP, tcpPort);

            // Enable auto flush for immediate sending
            sr = new StreamReader(tcpClient.GetStream());
            sw = new StreamWriter(tcpClient.GetStream()) { AutoFlush = true };
            Debug.Log("Connected to server!");
            // Send authorization message
            await AuthorizedProcess();
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.Message);
        }
        catch (IOException e)
        {
            Debug.LogError("IOException: " + e.Message);
        }
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
    // Start is called before the first frame update

    public string StripControlChars(string s)
    {
        return Regex.Replace(s, @"[^\x20-\x7F]", "");
    }
    private async Task ListenTcpFromServer()
    {
        try
        {
            while (true)
            {
                server_message = await sr.ReadLineAsync();
                if (server_message != null)
                {
                    server_message = StripControlChars(server_message);
                    ProcessServerMessage(server_message);
                }
            }
        }
        catch (IOException e)
        {
            Debug.LogError("IOException: " + e.Message);
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
        }
    }
    private void ProcessServerMessage(string message)
    {
        //Debug.Log("Received message from server: " + message);
        string[] parts = message.Split('|');

        if (parts.Length > 0)
        {
            string commandStr = parts[0].Trim();
            //Debug.Log("string Command : " + commandStr+ "length= " + commandStr.Length);

            if (Enum.TryParse(commandStr, true, out networkSubheader command)) // using true to ignore case
            {
                //Debug.Log("enum command: " + command);

                switch (command)
                {
                    case networkSubheader.CURRENT:
                        Debug.Log("handle current");
                        HandleCurrentClient(parts);
                        break;

                    case networkSubheader.JOIN:
                        Debug.Log("handle joined");
                        HandleClientJoined(parts);
                        break;
                    case networkSubheader.EXIT:
                        Debug.Log("handle exit");
                        HandleExitClient(parts);
                        Debug.Log(parts[1]);
                        break;
                    // Add more cases for other message types
                    default:
                        Debug.LogError("Unhandled command: " + command);
                        break;
                }
            }
            else
            {
                Debug.LogError("Invalid command received: " + commandStr);
            }
        }
        else
        {
            Debug.LogError("Message format incorrect: " + message);
        }
    }

    public void HandleExitClient(string[] parts)
    {
        target = parts[1];
    }
    private void HandleCurrentClient(string[] parts)
    {
        lock (playersToSpawn)
        {
            for (int i = 1; i < parts.Length; i++)
            {
                playersToSpawn.Add(parts[i]);
            }
        }
    }

    private void HandleClientJoined(string[] parts)
    {
        lock (playersToSpawn)
        {
            playersToSpawn.Add(parts[1]);
        }

    }
    
    private async void Update()
    {

        if (isconnect)
        {
            if (playersToSpawn.Count > 0)
            {
                foreach (var pname in playersToSpawn)
                {
                    DemoSpawn(pname);
                }
                playersToSpawn.Clear();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                Disconnect();
            }
            if (target != string.Empty)
            {
                /*Debug.Log("Destroy " + target);
                GameObject go = GameObject.Find(target);
                Destroy(go);*/
                spawner.DeleteObjectByName(target);
            }
            if (!tcpClient.Connected)
            {
                Debug.Log("Disconnected from server.");
            }
        }
    }

    private async void FixedUpdate()
    {
        if (playerTransform != null)
        {
            if (playerTransform.position != lastPosition/*|| orientationTransform.rotation != lastRotation*/)
            {
                lastPosition = playerTransform.position;
                SendTransformData(); // GOING TO MAKE
            }
        }
    }
    async Task AuthorizedProcess()
    {
        string username = Username.text;
        string hello_message = $"{username}|{null}";  // Replace with actual authorization info if needed
        try
        {
            await sw.WriteLineAsync(hello_message);
            Debug.Log("Authorization message sent.");
            server_message = sr.ReadLine();
            server_message = StripControlChars(server_message);
            string[] part = server_message.Split('|');
            if (part[0] == "PASS")
            {
                Debug.Log("Authorizatize passed");
                udpListenPort = int.Parse(part[1].Trim());
                udpClient_Recv = new UdpClient(udpListenPort);
                isconnect = true;
                OnReceiveServerMessage?.Invoke(this, new OnReceiveServerMessageEventArgs { message = "PASS" });
            }
            else
            {
                
                tcpClient.Dispose();
                tcpClient.Close();
                OnReceiveServerMessage?.Invoke(this, new OnReceiveServerMessageEventArgs { message = "ALREADY" });
                Debug.Log("This name already been chosen!");
                gameObject.SetActive(false);
            }
            //sr.Close();
        }
        catch (IOException e)
        {
            Debug.LogError("Failed to send authorization message: " + e.Message);
        }
    }

    void SendTransformData()
    {
        string message = $"{indexPlayer}|{playerTransform.position.x},{playerTransform.position.y},{playerTransform.position.z}";
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);// need to be configured
        udpClient_Send.Send(bytes, bytes.Length, UDPendPoint);
    }
    // Clean up resources when the application quits or object is destroyed

    private async void ListeningSyncData()
    {
        while (isconnect)
        {
            try
            {
                UdpReceiveResult result = await udpClient_Recv.ReceiveAsync();
                string transformData = Encoding.UTF8.GetString(result.Buffer);
                SyncTransformData(result.Buffer);
                //OnUpdateTransform?.Invoke(this, new OnUpdateTransformEventArgs { message =transformData} );
                //Debug.Log("UDP syncData received: "+ transformData);
            }
            catch (SocketException e)
            {
                Debug.LogError("UDP Socket Exception: " + e.Message);
                break;
            }
        }
    }

    private async void SyncTransformData(byte[] buffer)
    {
        string message = Encoding.UTF8.GetString(buffer);
        string[] part = message.Split('|');
        int index = int.Parse(part[0].Trim());
        if (index != indexPlayer)
        {
            Vector3 targetPosition = Move(part[1]);
            StartSmoothMovement(spawner.spawnedObjects[index], targetPosition);
        }
        Debug.Log("run");
        //OtherPlayer.transform.position = (Vector3)part[1];
        //OnUpdateTransform?.Invoke(this, EventArgs.Empty);
    }
    private void StartSmoothMovement(GameObject playerObject, Vector3 targetPosition)
    {
        StartCoroutine(SmoothMoveCoroutine(playerObject, targetPosition, 0.075f)); // Adjust duration as needed
    }

    private IEnumerator SmoothMoveCoroutine(GameObject playerObject, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = playerObject.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            playerObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        // Ensure exact final position
        playerObject.transform.position = targetPosition;
    }
    private void OnApplicationQuit()
    {
        Disconnect();
        if (tcpClient != null)
        {
            sr?.Close();
            sw?.Close();
            stream?.Close();
            tcpClient?.Close();

            Debug.Log("Disconnected from server.");
        }
    }
    private void Disconnect()
    {
        string username = Username.text;
        sw.WriteLineAsync($"{networkSubheader.EXIT.ToString()}|{username}");
        isconnect = false;
        tcpClient.Close();
        tcpClient.Dispose();
        udpClient_Send.Close();
        udpClient_Send.Dispose();
        udpClient_Recv.Close();
        udpClient_Recv.Dispose();
        Debug.Log("Disconnected from server.");
    }
}