using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UDP_Client : Base_UDP
{

    private string IP = "127.0.0.1";

    void Start()
    {
        data = new byte[1024];
        
        ip = new IPEndPoint(IPAddress.Any, ownPort);
        remoteIP = new IPEndPoint(IPAddress.Parse(IP), destPort);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ip);

        thread = new Thread(Listen);
        thread.Start();
    }

 
    protected override void Listen()
    {
        Debug.Log("Starting Client Thread");

        SendMessage();

        base.Listen();
    }
}
