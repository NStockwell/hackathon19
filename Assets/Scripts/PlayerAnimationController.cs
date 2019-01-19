using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimationController : MonoBehaviour
{
    //[SerializeField]
    //private Animator backgroundAnimation;
    [SerializeField]
    private Animator avatarAnimation;
    [SerializeField]
    private Animator fxAnimation;

    [SerializeField]
    private Color colorAttack01;
    [SerializeField]
    private Color colorAttack02;
    [SerializeField]
    private Color colorDefense01;
    [SerializeField]
    private Color colorDefense02;
    [SerializeField]
    private Color colorRecover;
    [SerializeField]
    private Image fxImage;

    [SerializeField]
    private float intimidatonDelayMin = 2.0f;
    [SerializeField]
    private float intimidatonDelayMax = 10.0f;


    //Player intimidate animation vars
    private float delay = 0.0f;
    private float timer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        delay = Random.Range(intimidatonDelayMin, intimidatonDelayMax);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(timer > delay)
        {
            SetBlinkAnimation();
            timer = 0.0f;
            delay = Random.Range(intimidatonDelayMin, intimidatonDelayMax);
        }
    }

    private void SetBlinkAnimation()
    {
        avatarAnimation.SetTrigger("BLINK");
    }

    public void SetHitAnimation()
    {
        avatarAnimation.SetTrigger("HIT");
    }

    public void SetRecoverAnimation()
    {
        SetAnimationColor(colorRecover);
        fxAnimation.SetTrigger("RECOVER");
    }

    public void SetAttackAnimation(float type)
    {
        if(type > 1)
        {
            SetAnimationColor(colorAttack01);
        }
        else
        {
            SetAnimationColor(colorAttack02);
        }
        fxAnimation.SetTrigger("ATTACK");
        fxAnimation.SetFloat("TYPE", type);
    }

    public void SetDefenseAnimation(float type)
    {
        if (type > 1)
        {
            SetAnimationColor(colorDefense01);
        }
        else
        {
            SetAnimationColor(colorDefense02);
        }
        fxAnimation.SetTrigger("DEFENSE");
        fxAnimation.SetFloat("TYPE", type);
    }

    public void SetDeadAnimation()
    {
        SetAnimationColor(Color.red);
        avatarAnimation.SetBool("DEAD", true);
        fxAnimation.SetBool("DEAD", true);
    }

    public void SetWinAnimation()
    {
        avatarAnimation.SetBool("WIN", true);
    }

    public void SetAnimationColor(Color newColor)
    {
        fxImage.color = newColor;
    }

    public void ResetAnimations()
    {
        avatarAnimation.SetBool("DEAD", false);
        fxAnimation.SetBool("DEAD", false);
        avatarAnimation.SetBool("WIN", false);
        fxAnimation.SetTrigger("RESET");
        avatarAnimation.SetTrigger("RESET");

    }
}
