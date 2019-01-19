using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using UnityEngine;

public class Server : MonoBehaviour
{
    public int port = 6321;

    private List<ServerClient> clients;
    private List<ServerClient> disconnectedList;

    private TcpListener server;
    private bool serverStarted = false;

    public void Init()
    {
        DontDestroyOnLoad(gameObject);
        clients = new List<ServerClient>();
        disconnectedList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            serverStarted = true;
            Application.runInBackground = true;

            StartListening();
        }
        catch (Exception e)
        {
            Debug.Log("[Server] Socket error: " + e.Message);
        }
    }

    private void Update()
    {
        if (!serverStarted) return;

        foreach (ServerClient client in clients)
        {
            if (!IsConnected(client.tcpClient))
            {
                client.tcpClient.Close();
                disconnectedList.Add(client);
                continue;
            }
            else
            {
                NetworkStream stream = client.tcpClient.GetStream();
                if (stream.DataAvailable)
                {
                    StreamReader reader = new StreamReader(stream, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(client, data);
                    }
                }
            }
        }

        for (int i = 0; i < disconnectedList.Count; i++)
        {
            // TODO: Report disconnection
            Debug.Log("[Server] Player " + disconnectedList[i].ClientName + " disconnected");
            clients.Remove(disconnectedList[i]);
            disconnectedList.RemoveAt(i);
        }
    }

    private void StartListening()
    {
        if (!serverStarted) return;

        Debug.Log("[Server] Waiting for connections...");
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private void AcceptTcpClient(IAsyncResult asyncResult)
    {
        TcpListener listener = asyncResult.AsyncState as TcpListener;

        ServerClient serverClient = new ServerClient(listener.EndAcceptTcpClient(asyncResult));
        serverClient.setIsHost(clients.Count == 0); // Assume the first client is the host
        clients.Add(serverClient);
        Debug.Log("[Server] " + serverClient.ClientName + " has connected");

        if (clients.Count == 2)
        {
            // Generate seed for game
            System.Random random = new System.Random();
            int seed = random.Next();

            // Send start game with the turn
            Broadcast(string.Format("StartGameRequest|{0}|{1}", true, seed), clients[0]);
            Broadcast(string.Format("StartGameRequest|{0}|{1}", false, seed), clients[1]);
        }
        else
        {
            StartListening();
        }
    }

    private bool IsConnected(TcpClient tcpClient)
    {
        try
        {
            if (tcpClient != null && tcpClient.Client != null && tcpClient.Client.Connected)
            {
                if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(tcpClient.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Server -> Clients
    private void Broadcast(string data, ServerClient client)
    {
        Debug.Log("[Server] Send to " + client.ClientName + ": " + data);
        try
        {
            StreamWriter writer = new StreamWriter(client.tcpClient.GetStream());
            writer.WriteLine(data);
            writer.Flush();
        }
        catch (Exception e)
        {
            Debug.Log("[Server] Write error : " + e.Message);
        }
    }

    private void Broadcast(string data, List<ServerClient> clientList)
    {
        Debug.Log("[Server] Send to ALL: " + data);
        foreach (ServerClient serverClient in clientList)
        {
            Broadcast(data, serverClient);
        }
    }

    private void BroadcastToExcept(string data, List<ServerClient> clientList, ServerClient client)
    {
        Debug.Log("[Server] Send to ALL: " + data);
        foreach (ServerClient serverClient in clientList)
        {
            if (serverClient != client)
                Broadcast(data, serverClient);
        }
    }

    // Client -> Server
    private void OnIncomingData(ServerClient serverClient, string data)
    {
        Debug.Log("[Server] Received '" + data + "' from " + serverClient.ClientName);
        string[] splitData = data.Split('|');
        switch (splitData[0])
        {
            case NetworkMessageTypes.PING: break; // Do nothing for now
            case NetworkMessageTypes.MAKE_MOVE:
                BroadcastToExcept(data, clients, serverClient);
                break;
            case NetworkMessageTypes.UPDATE_TURNS:
                BroadcastToExcept(data, clients, serverClient);
                break;
        }
    }

    private void OnApplicationQuit()
    {
        CloseServer();
    }

    private void OnDisable()
    {
        CloseServer();
    }

    private void CloseServer()
    {
        server.Stop();
        Debug.Log("[Server] Server stoped");
    }
}

// A representation of a client by the server
public class ServerClient
{
    public string ClientName { private set; get; }
    private bool _isHost;
    public TcpClient tcpClient;

    public ServerClient(TcpClient tcpClient)
    {
        ClientName = "Unknown";
        _isHost = false;
        this.tcpClient = tcpClient;
    }

    public void setIsHost(bool isHost)
    {
        _isHost = isHost;
        ClientName = _isHost ? "Host" : "Client";
    }

}
