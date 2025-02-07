using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSound : MonoBehaviour
{
    // Start is called before the first frame update
    public void PlaySound()
    {
        Invoke("Destroy", 2f);
        GetComponent<AudioSource>().pitch = 1 + Random.Range(-0.1f, 0.1f);
        GetComponent<AudioSource>().Play();
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
