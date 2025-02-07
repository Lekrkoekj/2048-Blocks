using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int coins;
    public bool gameOver;
    [SerializeField] private TMP_Text coinCounter;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject restartPanel;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject powerupsContainer;

    public int bombPrice;
    [SerializeField] private Button bombButton;
    [SerializeField] private TMP_Text bombPriceText;

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
        bombPriceText.text = $"<sprite=0> {bombPrice}";
    }

    // Update is called once per frame
    void Update()
    {
        coinCounter.text = $"<sprite=0>  {coins}";
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt("coins", coins);
        if (coins >= bombPrice)
        {
            bombButton.interactable = true;
        }
        else
        {
            bombButton.interactable = false;
        }
    }

    public void GameOver()
    {
        gameOver = true;
        gameOverPanel.SetActive(true);
        powerupsContainer.SetActive(false);
        restartButton.SetActive(false);
        coinCounter.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenRestartScreen()
    {
        restartPanel.SetActive(true);
        powerupsContainer.SetActive(false);
        restartButton.SetActive(false);
        coinCounter.gameObject.SetActive(false);
    }

    public void CloseRestartScreen()
    {
        restartPanel.SetActive(false);
        powerupsContainer.SetActive(true);
        restartButton.SetActive(true);
        coinCounter.gameObject.SetActive(true);
    }
}
