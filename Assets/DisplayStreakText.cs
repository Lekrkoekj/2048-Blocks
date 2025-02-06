using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayStreakText : MonoBehaviour
{
    public int streakAmount;
    [SerializeField] private TMP_Text streakText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private GameObject canvas;
    [SerializeField] private float despawnDelay;
    // Start is called before the first frame update
    void Start()
    {
        streakText.text = $"Streak X{streakAmount}";
        coinsText.text = $"+{streakAmount} <sprite=0>";
        Invoke("DestroyText", despawnDelay);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 lookatPos = new Vector3(transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
        canvas.transform.LookAt(lookatPos);
    }

    private void DestroyText()
    {
        Destroy(gameObject);
    }
}
