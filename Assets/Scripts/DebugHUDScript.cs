using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHUDScript : MonoBehaviour
{
    public GameHudController gameHUDController;
    private bool topPlayer = false;
    
    void Update()
    {
        topPlayer = false;
        //Left shift for top player
        if(Input.GetKey(KeyCode.LeftShift))
        {
            topPlayer = true;
        }

        //Attack
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                gameHUDController.OnPlayerAttack(topPlayer, 1);
            }
            else
            {
                gameHUDController.OnPlayerAttack(topPlayer, 2);
            }
        }
        //Defense
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                gameHUDController.OnPlayerDefense(topPlayer, 1);
            }
            else
            {
                gameHUDController.OnPlayerDefense(topPlayer, 2);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            gameHUDController.OnPlayerRecover(topPlayer);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            gameHUDController.OnPlayerWin(topPlayer);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gameHUDController.ResetAnimation();
        }
    }
}
