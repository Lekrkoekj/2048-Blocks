using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    public static SceneSwitcher Instance { get; private set; }

    [SerializeField] private Animator canvasAnimator;
    private int indexToLoad;
    private bool switching;

    [SerializeField] private Image transitionGraphic;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color darkColor;
    private float globalTime;
    

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void SwitchScene(int index)
    {
        if(switching)
        {
            Debug.LogWarning("Already switching scenes! - Action cancelled");
            return;
        }
        transitionGraphic.color = normalColor;
        transitionGraphic.material.SetColor("_Color", normalColor);
        canvasAnimator.SetBool("SceneLoading", true);
        indexToLoad = index;
        switching = true;
    }

    [SerializeField] private void LoadScene()
    {
        SceneManager.LoadScene(indexToLoad);
        FinishLoadingScene();
    }

    [SerializeField] private void FinishLoadingScene()
    {
        canvasAnimator.SetBool("SceneLoading", false);
        indexToLoad = 0;
        switching = false;
    }


    // Update is called once per frame
    void Update()
    {
        globalTime += Time.unscaledDeltaTime;
        transitionGraphic.material.SetFloat("_GlobalTime", globalTime);
    }
}
