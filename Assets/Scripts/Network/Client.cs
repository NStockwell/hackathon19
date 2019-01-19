using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    private bool isSocketReady = false;
    private bool isHost = false;
    private TcpClient tcpClient;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;
    private float pingDelta;

    public bool IsTurn { set; get; }

    public bool ConnectToServerAsHost(int port)
    {
        isHost = true;
        return ConnectToServer("127.0.0.1", port);
    }

    public bool ConnectToServer(string host, int port)
    {
        if (isSocketReady) return false;

        try
        {
            tcpClient = new TcpClient(host, port);
            stream = tcpClient.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            pingDelta = 1.0f;

            isSocketReady = true;
            DontDestroyOnLoad(gameObject);
        }
        catch (Exception e)
        {
            Debug.LogError("Socket error: " + e.Message);
            GameManager.Instance.OnSocketClosed();
        }

        return isSocketReady;
    }

    private void Update()
    {
        if (!isSocketReady) return;

        try
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                {
                    OnIncomingData(data);
                }
            }

            pingDelta -= Time.deltaTime;
            if(pingDelta <= 0) {
                Send(NetworkMessageTypes.PING);
                pingDelta = 1.0f;
            }
        } 
        catch(Exception e)
        {
            Debug.Log("[Client] " + e.Message);
            CloseSocket();
        }
    }

    // Sending mesasges to the server
    public void Send(string data)
    {
        if (!isSocketReady) return;

        writer.WriteLine(data);
        writer.Flush();
    }

    // Read messages from the server
    private void OnIncomingData(string data)
    {
        Debug.Log("[Client] Received data: " + data);
        string[] splitData = data.Split('|');
        if (splitData.Length == 0) return;

        switch (splitData[0])
        {
            case NetworkMessageTypes.START_GAME:
                if (splitData.Length >= 3)
                {
                    bool isMyTurn = bool.Parse(splitData[1]);
                    int randomSeed = int.Parse(splitData[2]);
                    GameManager.Instance.OnStartGameRequest(isMyTurn, randomSeed);
                }
                else
                {
                    Debug.Log("[Client] Could not parse: " + data);
                }
                break;
            case NetworkMessageTypes.MAKE_MOVE:
                if (splitData.Length >= 5)
                {
                    Vector2Int from = new Vector2Int(int.Parse(splitData[1]), int.Parse(splitData[2]));
                    Vector2Int to = new Vector2Int(int.Parse(splitData[3]), int.Parse(splitData[4]));
                    GameManager.Instance.OnMakeMoveRequest(from, to);
                }
                else
                {
                    Debug.Log("[Client] Could not parse: " + data);
                }
                break;
            case NetworkMessageTypes.UPDATE_TURNS:
                if(splitData.Length >= 2)
                {
                    bool isMyTurn = bool.Parse(splitData[1]);
                    GameManager.Instance.OnUpdateTurnsRequest(isMyTurn);
                }
                else
                {
                    Debug.Log("[Client] Could not parse: " + data);
                }
                break;
            default:
                Debug.LogError("[Client] Unhandled message + " + data);
                break;
        }

    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void OnDisable()
    {
        CloseSocket();
    }

    private void CloseSocket()
    {
        if (!isSocketReady) return;

        writer.Close();
        reader.Close();
        tcpClient.Close();
        isSocketReady = false;

        GameManager.Instance.OnSocketClosed();
    }

    public void SendMakeMove(ref Vector2Int from, ref Vector2Int to)
    {
        Send(string.Format("{0}|{1}|{2}|{3}|{4}", NetworkMessageTypes.MAKE_MOVE, from.x, from.y, to.x, to.y));
    }

    public void SendChangeTurns()
    {
        Send("ChangeTurns");
    }
}
