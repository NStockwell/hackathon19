using UnityEngine;

public class PowerupController : MonoBehaviour
{
    public GameHudController ghc;
    
    private const int TYPE_01 = 1;
    private const int TYPE_02 = 2;
    
    public void SetAnimation(GemColor gemColor, int gemCount, bool isTopPlayer)
    {
        Debug.Log(gemColor);
        switch (gemColor)
        {
            case GemColor.Blue:     //RECOVER
                ghc.OnPlayerRecover(isTopPlayer);
                break;
            case GemColor.Red:      //ATTACK 01
                ghc.OnPlayerAttack(isTopPlayer, TYPE_01);
                break;
            case GemColor.Purple:   //ATTACK 02
                ghc.OnPlayerAttack(isTopPlayer, TYPE_02);
                break;
            case GemColor.Green:    //DEFENSE 01
                ghc.OnPlayerDefense(isTopPlayer, TYPE_01);
                break;
            case GemColor.Yellow:   //DEFENSE 02
                ghc.OnPlayerDefense(isTopPlayer, TYPE_02);
                break;
            case GemColor.Empty:
            case GemColor.COUNT:
            case GemColor.INVALID:
            default:
                Debug.Log("INVALID GEM");
                break;
        }
    }
}
