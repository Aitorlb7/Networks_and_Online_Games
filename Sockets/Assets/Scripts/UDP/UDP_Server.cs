using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UDP_Server : Base_UDP
{


    void Start()
    {
        data = new byte[1024];        
        
        ip = new IPEndPoint(IPAddress.Any, ownPort);
        remoteIP = new IPEndPoint(IPAddress.Any, destPort);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ip);

        thread = new Thread(Listen);
        thread.Start();
    }

    protected override void Listen() 
    {
        AddTextToConsole("Starting Server Thread");

        base.Listen();
    }

}
