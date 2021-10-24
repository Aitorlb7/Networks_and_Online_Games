using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Base_UDP : MonoBehaviour
{
    protected Thread thread = null;

    protected Socket socket;
    protected IPEndPoint ip;
    protected EndPoint remoteIP;

    protected byte[] data = new byte[1024];

    protected int recievedData;
    protected string recievedMessage;

    protected readonly object lockObject = new object();


    public int ownPort;
    public int destPort;
    public string message;
    public int sleepSeconds;


    public Text consoleText;
    private List<string> consoleStrings = new List<string>();
    protected bool breakAndDisconect;
    bool updateConsole;

    private int logNum = 0;
    private int consoleLogLimit = 16;

    private void Update()
    {
        if(updateConsole)
        {   
            lock (lockObject)
            {
                if (logNum >= consoleLogLimit)
                {
                    consoleText.text = "";
                    logNum = 0;
                }

                logNum += consoleStrings.Count;

                foreach (string message in consoleStrings)
                {
                    consoleText.text += message + '\n';

                    consoleStrings.Remove(message);
                }

                updateConsole = false;
            }
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Disconecting Socket and aborting Thread");

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

    protected virtual void Listen()
    {
        while (true)
        {
            if (breakAndDisconect)
            {
                break;
            }

            try
            {
                recievedData = socket.ReceiveFrom(data, ref remoteIP);
            }
            catch
            {
                AddTextToConsole("ERROR: Can't recieve message");
                continue;
            }

            if (recievedData > 0)
            {
                recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

                recievedMessage.Trim('\0'); //Trim all zeros from the string and save space

                AddTextToConsole("Recieved: " + recievedMessage);

                Thread.Sleep(sleepSeconds);

                SendMessage();

            }
        }

        Debug.Log("Disconnecting Socket");

        breakAndDisconect = false;
        socket.Close();
        socket = null;

    }

    protected virtual void SendMessage()
    {
        try
        {
            data = Encoding.ASCII.GetBytes(message);

            socket.SendTo(data, remoteIP);

            data = new byte[1024];

            AddTextToConsole("Sent: " + message);

        }
        catch (Exception e)
        {
            AddTextToConsole("Failed to send message");
            Debug.LogError(e.StackTrace);
        }
    }

    protected void AddTextToConsole(string textToAdd)
    {
        lock (lockObject)
        {
            consoleStrings.Add(textToAdd);

            updateConsole = true;
        }
    }
}
