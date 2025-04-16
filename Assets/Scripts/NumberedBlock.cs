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
    [SerializeField] private Image warningIcon;
    [SerializeField] private Image warningFill;
    [SerializeField] private Color fillInColorInactive;
    [SerializeField] private Color warningIconColorNormal;
    [SerializeField] private Color warningIconColorInactive;


    [SerializeField] private GameObject streakTextPrefab;
    [SerializeField] private TMP_Text[] valueTexts;
    [SerializeField] private Color[] baseCubeColors;

    private Material material;
    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        SetCubeColor(value);
        if(GameManager.Instance.soundMuted == 1)
        {
            mergeSound.volume = 0;
            streakSound.volume = 0;
        }
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
                warningIcon.color = warningIconColorNormal;
                if (currentGameOverTime > 0.3f)
                {
                    warningFill.transform.parent.GetComponent<Animator>().SetBool("overTheLine", true);
                }
            }

            if (currentGameOverTime > 0 && currentGameOverTime < timeUntilGameOver && !overTheLine)
            {
                currentGameOverTime -= Time.deltaTime;
                warningFill.color = fillInColorInactive;
                warningIcon.color = warningIconColorInactive;
                if(currentGameOverTime > 0.3f)
                {
                    warningFill.transform.parent.GetComponent<Animator>().SetBool("overTheLine", false);
                }
            }
            
            if (currentGameOverTime > 0.3f)
            {
                warningFill.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                warningFill.transform.parent.gameObject.SetActive(false);
            }
            if(currentGameOverTime > timeUntilGameOver && !GameManager.Instance.gameOver)
            {
                GameManager.Instance.GameOver();
            }
            warningFill.fillAmount = currentGameOverTime / timeUntilGameOver;
        }
    }

    private void FixedUpdate()
    {
        warningFill.transform.position = Camera.main.WorldToScreenPoint(transform.position);
    }

    private void MergeBlock(GameObject target)
    {
        Vector3 thisVelocity = GetComponent<Rigidbody>().velocity;
        Vector3 targetVelocity = target.GetComponent<Rigidbody>().velocity;
        if(thisVelocity.sqrMagnitude >= targetVelocity.sqrMagnitude)
        {
            value *= 2;
            SetCubeColor(value);
            GetComponent<Rigidbody>().AddForce(Random.Range(-25f, 25f), Random.Range(70f, 150f), Random.Range(-27.5f, 27.5f));
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
            GameManager.Instance.RemoveBlock(target.GetComponent<NumberedBlock>());
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
            blockComponent.GetComponent<Rigidbody>().AddForce(Random.Range(0f, 0f), Random.Range(70f, 150f), Random.Range(0f, 0f));
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
            GameManager.Instance.RemoveBlock(this);
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
