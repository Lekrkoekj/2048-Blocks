using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberedBlock : MonoBehaviour
{
    public int value;
    public bool canMerge = true;
    [SerializeField] private float maxStreakTime;

    private float currentStreakTime;
    public int currentStreak;
    public bool streakRunning;

    public AudioSource mergeSound;
    public AudioSource streakSound;

    private float currentGameOverTime;
    [SerializeField] private float timeUntilGameOver;
    private bool overTheLine;

    [SerializeField] private GameObject streakTextPrefab;
    [SerializeField] private Image warningIconFill;
    [SerializeField] private TMP_Text[] valueTexts;
    [SerializeField] private Color[] baseCubeColors;

    private Material material;
    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        SetCubeColor(value);
    }

    public void SetCubeColor(int value)
    {
        int timesMerged = (int)Mathf.Log(value, 2) - 1;
        int colorInArray = timesMerged % baseCubeColors.Length;
        int loopThroughArrayCount = (int)Mathf.Floor(timesMerged / baseCubeColors.Length);

        float darkenAmountPerLoop = 0.75f;
        Color selectedColor = baseCubeColors[colorInArray];
        Color newColor = new Color(
            selectedColor.r * Mathf.Pow(darkenAmountPerLoop, loopThroughArrayCount), 
            selectedColor.g * Mathf.Pow(darkenAmountPerLoop, loopThroughArrayCount), 
            selectedColor.b * Mathf.Pow(darkenAmountPerLoop, loopThroughArrayCount)
        );

        material.SetColor("_Color", newColor);
    }

    // Update is called once per frame
    void Update()
    {
        // Display value on block
        foreach (var text in valueTexts)
        {
            text.text = value.ToString();
        }

        // Streak
        if(currentStreakTime < maxStreakTime && streakRunning)
        {
            currentStreakTime += Time.deltaTime;
        }
        if(currentStreakTime > maxStreakTime)
        {
            currentStreak = 0;
            streakRunning = false;
        }

        // Game over warning timer
        if (canMerge && !GameManager.Instance.gameOver)
        {
            overTheLine = transform.position.z < -9.5f && transform.position.y > 0 && transform.position.y < 8;
            if (currentGameOverTime < timeUntilGameOver && overTheLine)
            {
                currentGameOverTime += Time.deltaTime;
            }

            if (currentGameOverTime > 0 && currentGameOverTime < timeUntilGameOver && !overTheLine)
            {
                currentGameOverTime -= Time.deltaTime;
            }
            warningIconFill.transform.position = Camera.main.WorldToScreenPoint(transform.position);
            if (currentGameOverTime > 0.3f)
            {
                warningIconFill.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                warningIconFill.transform.parent.gameObject.SetActive(false);
            }
            if(currentGameOverTime > timeUntilGameOver && !GameManager.Instance.gameOver)
            {
                GameManager.Instance.GameOver();
            }
            warningIconFill.fillAmount = currentGameOverTime / timeUntilGameOver;
        }
    }

    private void MergeBlock(GameObject target)
    {
        Vector3 thisVelocity = GetComponent<Rigidbody>().velocity;
        Vector3 targetVelocity = target.GetComponent<Rigidbody>().velocity;
        if(thisVelocity.sqrMagnitude >= targetVelocity.sqrMagnitude)
        {
            value *= 2;
            SetCubeColor(value);
            GetComponent<Rigidbody>().AddForce(Random.Range(-100f, 100f), Random.Range(70f, 150f), Random.Range(-45f, 45f));
            AddToStreak();
            mergeSound.pitch = 1 + Random.Range(-0.1f, 0.1f);
            mergeSound.Play();
            if (currentStreak > 1)
            {
                GameManager.Instance.AddCoins(currentStreak);
                GameObject streakText = Instantiate(streakTextPrefab);
                streakText.GetComponent<DisplayStreakText>().streakAmount = currentStreak;
                streakText.transform.position = transform.position + new Vector3(0, 1, 0);
                streakSound.pitch = 1 + 0.1f * currentStreak - 0.2f;
                streakSound.Play();
            }
            Destroy(target);
        }
        else
        {
            NumberedBlock blockComponent = target.GetComponent<NumberedBlock>();
            blockComponent.currentStreak = Mathf.Max(blockComponent.currentStreak, currentStreak) + 1;
            blockComponent.currentStreakTime = 0;
            blockComponent.streakRunning = true;
            blockComponent.value *= 2;
            blockComponent.SetCubeColor(blockComponent.value);
            blockComponent.GetComponent<Rigidbody>().AddForce(Random.Range(-100f, 100f), Random.Range(70f, 150f), Random.Range(-45f, 45f));
            blockComponent.mergeSound.pitch = 1 + Random.Range(-0.1f, 0.1f);
            blockComponent.mergeSound.Play();
            if (blockComponent.currentStreak > 1)
            {
                GameManager.Instance.AddCoins(blockComponent.currentStreak);
                GameObject streakText = Instantiate(streakTextPrefab);
                streakText.GetComponent<DisplayStreakText>().streakAmount = blockComponent.currentStreak;
                streakText.transform.position = blockComponent.transform.position + new Vector3(0, 1, 0);
                blockComponent.streakSound.pitch = 1 + 0.1f * blockComponent.currentStreak - 0.2f;
                blockComponent.streakSound.Play();
            }
            Destroy(gameObject);
        }
    }

    public void AddToStreak()
    {
        currentStreakTime = 0;
        streakRunning = true;
        currentStreak++;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (canMerge)
        {
            if (collision.collider.tag == gameObject.tag)
            {
                if (value == collision.collider.GetComponent<NumberedBlock>().value)
                {
                    MergeBlock(collision.gameObject);
                }
            }
        }
    }
}
