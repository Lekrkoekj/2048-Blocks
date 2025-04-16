using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSound : MonoBehaviour
{
    [SerializeField] private AudioSource bombSound;
    // Start is called before the first frame update
    public void PlaySound()
    {
        Invoke("Destroy", 2f);
        if (GameManager.Instance.soundMuted == 0) bombSound.volume = 1;
        else bombSound.volume = 0;
        bombSound.pitch = 1 + Random.Range(-0.1f, 0.1f);
        bombSound.Play();
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
