using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuLogo : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Animator mainCanvasAnimator;

    public bool logoShown;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void ShowLogo()
    {
        logoShown = true;
        animator.SetBool("ShowLogo", true);
    }
    public void HideLogo()
    {
        logoShown = false;
        animator.SetBool("ShowLogo", false);
        mainCanvasAnimator.SetBool("GameStarted", true);
    }
}
