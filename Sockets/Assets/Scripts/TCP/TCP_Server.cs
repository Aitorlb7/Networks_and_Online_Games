using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
public class TCP_Server : MonoBehaviour
{
    private Thread thread = null;

    private Socket socket = null;
    private Socket client = null;
    private IPEndPoint ip;

    private byte[] data = new byte[1024];

    private int recievedData;
    private string recievedMessage;

    private readonly object lockConsole = new object();

    public int ownPort;

    public string message;
    public int sleepSeconds;
    public Text consoleText;

    private List<string> consoleStrings = new List<string>();
    private bool updateConsole;
    private bool clearConsole;
    void Start()
    {
        data = new byte[1024];

        ip = new IPEndPoint(IPAddress.Any, ownPort);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(ip);
        socket.Listen(1);

        thread = new Thread(Listen);
        thread.Start();       
    }

    
    void Update()
    {
        if (updateConsole)
        {
            lock (lockConsole)
            {
                if (clearConsole)
                {
                    consoleText.text = "";
                    clearConsole = false;
                }

                foreach (string message in consoleStrings)
                {
                    consoleText.text += message + '\n';

                    consoleStrings.Remove(message);

                }

                updateConsole = false;
            }
        }
    }

    private void Listen()
    {

        while (true)
        {
            if (client == null)
            {
                ClearServerConsole();

                client = socket.Accept();
                AddTextToConsole("Server Connected with " + client.RemoteEndPoint);
            }

            recievedData = client.Receive(data);

            if (recievedData == 0)
            {
                AddTextToConsole("Client "+ client.RemoteEndPoint + " Disconnected");

                client.Close();
                client = null;
                continue;
            }

            recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

            recievedMessage.Trim('\0'); //Trim all zeros from the string and save space

            AddTextToConsole("Recieved: " + recievedMessage);

            Thread.Sleep(sleepSeconds);

            SendMessage();


        }
    }

    private void SendMessage()
    {
        try
        {
            data = Encoding.ASCII.GetBytes(message);

            client.Send(data);

            data = new byte[1024];

            AddTextToConsole("Sent: " + message);

        }
        catch (Exception e)
        {
            AddTextToConsole("Failed to send message");
            Debug.LogError(e.StackTrace);
        }
    }


    private void OnDestroy()
    {
        Debug.Log("Disconecting Server and aborting Thread");

        if (socket != null)
        {
            socket.Close();
            socket = null;
        }

        if (thread.IsAlive)
        {
            thread.Abort();
            thread = null;
        }
    }

    private void AddTextToConsole(string textToAdd)
    {
        lock (lockConsole)
        {
            consoleStrings.Add(textToAdd);

            updateConsole = true;
        }

    }

    private void ClearServerConsole()
    {
        consoleStrings.Clear();
        updateConsole = true;
        clearConsole = true;
    }
}
