using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public static class NetworkMessageTypes
{
    public const string START_GAME = "StartGameRequest";
    public const string MAKE_MOVE = "MakeMoveRequest";
    public const string UPDATE_TURNS = "UpdateTurnsRequest";
    public const string PING = "PingRequest";
}