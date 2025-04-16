using System.Collections;
using System;
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
    [SerializeField] private TMP_Text highScoreCounter;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject restartPanel;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject powerupsContainer;

    public int bombPrice;
    [SerializeField] private Button bombButton;
    [SerializeField] private TMP_Text bombPriceText;

    [SerializeField] private List<NumberedBlock> activeBlocks = new();

    public int score;
    public int highScore;
    private float currentlyDisplayedScore;
    [SerializeField] private float scoreCountUpSpeed;
    [SerializeField] private TMP_Text currentScoreText;

    private bool startGameOverScoreCount;
    private float displayedGameOverScore;
    [SerializeField] private TMP_Text gameOverScoreText;

    public int soundMuted;
    [SerializeField] private Sprite soundOnImg;
    [SerializeField] private Sprite soundOffImg;
    [SerializeField] private Image soundButtonIcon;
    [SerializeField] private AudioSource soundMuteButtonSound;

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
        highScore = PlayerPrefs.GetInt("highScore", 0);
        bombPriceText.text = $"<sprite=0> {bombPrice}";
        soundMuted = PlayerPrefs.GetInt("soundMuted", 0);
        SetSoundMute(Convert.ToBoolean(soundMuted));
    }

    // Update is called once per frame
    void Update()
    {
        coinCounter.text = $"{coins}  <sprite=0>";
        highScoreCounter.text = $"{highScore}  <sprite=1>";

        currentScoreText.text = currentlyDisplayedScore.ToString("0");

        if (gameOver)
        {
            gameOverScoreText.text = "Score: " + displayedGameOverScore.ToString("0");
        }


        if (Input.GetKeyDown("c") && Application.isEditor)
        {
            coins += 100;
        }
    }

    public void SetSoundMute(bool muteSound)
    {
        if (muteSound)
        {
            soundMuted = 1;
            soundButtonIcon.sprite = soundOffImg;
            soundMuteButtonSound.volume = 0;
            activeBlocks.ForEach((block) =>
            {
                block.mergeSound.volume = 0;
                block.streakSound.volume = 0;
            });
        }
        else
        {
            soundMuted = 0;
            soundButtonIcon.sprite = soundOnImg;
            soundMuteButtonSound.volume = 1;
            activeBlocks.ForEach((block) =>
            {
                if (block == null) return;
                block.mergeSound.volume = 1;
                block.streakSound.volume = 1;
            });
        }

        PlayerPrefs.SetInt("soundMuted", soundMuted);
    }

    public void ToggleSoundMuted()
    {
        if(soundMuted == 0)
        {
            SetSoundMute(true);
        }
        else
        {
            SetSoundMute(false);
        }
        soundMuteButtonSound.Play();
    }

    IEnumerator CountUpScore(float baseSpeed)
    {
        while (currentlyDisplayedScore < score)
        {
            float difference = score - currentlyDisplayedScore;
            float increment = Mathf.Max(baseSpeed / 2, difference * 0.025f); // Scale speed based on difference
            currentlyDisplayedScore += increment;
            currentlyDisplayedScore = Mathf.Clamp(currentlyDisplayedScore, 0, score);
            yield return null;
        }
    }

    IEnumerator CountUpGameOverScore(float baseSpeed)
    {
        yield return new WaitForSeconds(1f);
        while (displayedGameOverScore < score)
        {
            float difference = score - displayedGameOverScore;
            float increment = Mathf.Max(baseSpeed / 2, difference * 0.025f); // Scale speed based on difference
            displayedGameOverScore += increment;
            displayedGameOverScore = Mathf.Clamp(displayedGameOverScore, 0, score);
            yield return null;
        }
    }

    public void AddBlock(NumberedBlock block)
    {
        activeBlocks.Add(block);
        CalculateScore();
    }

    public void RemoveBlock(NumberedBlock block)
    {
        activeBlocks.Remove(block);
        CalculateScore();
    }

    Coroutine scoreCoroutine;
    private void CalculateScore()
    {
        if(scoreCoroutine != null) StopCoroutine(scoreCoroutine);
        int totalScore = 0;
        activeBlocks.ForEach(block =>
        {
            totalScore += block.value;
        });
        score = totalScore;
        scoreCoroutine = StartCoroutine(CountUpScore(scoreCountUpSpeed));

        highScore = PlayerPrefs.GetInt("highScore", 0);

        if(score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("highScore", highScore);
        }
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
        currentScoreText.gameObject.SetActive(false);
        highScoreCounter.gameObject.SetActive(false);
        StartCoroutine(CountUpGameOverScore(scoreCountUpSpeed));
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
        currentScoreText.gameObject.SetActive(false);
    }

    public void CloseRestartScreen()
    {
        restartPanel.SetActive(false);
        powerupsContainer.SetActive(true);
        restartButton.SetActive(true);
        coinCounter.gameObject.SetActive(true);
        currentScoreText.gameObject.SetActive(true);
    }
}
