using UnityEngine;
using UnityEngine.UI;

public class GameHudController : MonoBehaviour
{
    [SerializeField]
    private Slider _hpTopPlayerHUD;
    [SerializeField]
    private Slider _hpBottomPlayerHUD;
    private int _hpTopPlayer = 100;
    private int _hpBottomPlayer = 100;
    public int HPTopPlayer { get => _hpTopPlayer; set => _hpTopPlayer = value; }
    public int HPBottomPlayer { get => _hpBottomPlayer; set => _hpBottomPlayer = value; }

    [SerializeField]
    private PlayerAnimationController animTopPlayer;
    [SerializeField]
    private PlayerAnimationController animBottomPlayer;


    // Start is called before the first frame update
    void Start()
    {
        _hpTopPlayerHUD.value = HPTopPlayer;
        _hpBottomPlayerHUD.value = _hpBottomPlayer;
    }

    void Update()
    {
        if(HPTopPlayer != _hpTopPlayerHUD.value || HPBottomPlayer != _hpBottomPlayerHUD.value)
        {
            UpdateHud();
        }
    }

    private void UpdateHud()
    {
        _hpTopPlayerHUD.value = HPTopPlayer;
        _hpBottomPlayerHUD.value = HPBottomPlayer;
    }

    public void OnPlayerAttack(bool topPlayer, int attackType)
    {
        if (topPlayer)
        {
            animTopPlayer.SetAttackAnimation(attackType);
        }
        else
        {
            animBottomPlayer.SetAttackAnimation(attackType);
        }

        OnPlayerHit(!topPlayer);
    }

    private void OnPlayerHit(bool topPlayer)
    {
        if (topPlayer)
        {
            animTopPlayer.SetHitAnimation();
        }
        else
        {
            animBottomPlayer.SetHitAnimation();
        }
    }

    public void OnPlayerRecover(bool topPlayer)
    {
        if (topPlayer)
        {
            animTopPlayer.SetRecoverAnimation();
        }
        else
        {
            animBottomPlayer.SetRecoverAnimation();
        }
    }

    public void OnPlayerDefense(bool topPlayer, int defenseType)
    {
        if (topPlayer)
        {
            animTopPlayer.SetDefenseAnimation(defenseType);
        }
        else
        {
            animBottomPlayer.SetDefenseAnimation(defenseType);
        }
    }

    public void OnPlayerWin(bool topPlayer)
    {
        if (topPlayer)
        {
            animTopPlayer.SetWinAnimation();
            animBottomPlayer.SetDeadAnimation();
        }
        else
        {
            animTopPlayer.SetDeadAnimation();
            animBottomPlayer.SetWinAnimation();
        }
    }

    public void ResetAnimation()
    {
        animTopPlayer.ResetAnimations();
        animBottomPlayer.ResetAnimations();
    }
}
