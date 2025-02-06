using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int coins;
    public bool gameOver;
    [SerializeField] private TMP_Text coinCounter;
    [SerializeField] private GameObject gameOverPanel;

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
        coins = PlayerPrefs.GetInt("coins", 0);
    }

    // Update is called once per frame
    void Update()
    {
        coinCounter.text = $"<sprite=0> {coins}";
    }

    public void GameOver()
    {
        gameOver = true;
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
