using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    bool _isMyTurn;
    int _randomSeed;

    public static GameManager Instance;
    GemsManager gemsManager;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if(Instance == null) 
            Instance = this;
        else
            Destroy(gameObject);
    }

    // --------- Game Events ---------

    public void OnSceneLoaded(GemsManager gemsManager)
    {
        this.gemsManager = gemsManager;
        this.gemsManager.StartGame(_randomSeed, _isMyTurn);
    }

    // --------- Network Messages ---------

    public void OnStartGameRequest(bool isMyTurn, int randomSeed)
    {
        _isMyTurn = isMyTurn;
        _randomSeed = randomSeed;

        SceneManager.LoadScene("GameScene");
    }

    public void OnMakeMoveRequest(Vector2Int from, Vector2Int to)
    {
        gemsManager.MakeMoveRemote(from, to);
    }

    public void OnUpdateTurnsRequest(bool isMyTurn)
    {
        _isMyTurn = isMyTurn;
        // TODO: Update Game
    }

    public void OnSocketClosed()
    {
        SceneManager.LoadScene("Menu");
    }
}
