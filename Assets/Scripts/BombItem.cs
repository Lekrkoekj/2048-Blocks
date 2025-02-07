using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombItem : MonoBehaviour
{
    public bool canExplode;
    private bool exploded = false;
    [SerializeField] private float explosionRadius;

    [SerializeField] private SphereCollider trigger;

    [SerializeField] private ParticleSystem explosionParticles;

    [SerializeField] private MeshRenderer model;

    // Update is called once per frame
    void Update()
    {
        if(exploded)
        {
            trigger.radius = Mathf.Lerp(trigger.radius, explosionRadius, 0.05f);
            if(trigger.radius > explosionRadius * 0.99f)
            {
                Destroy(gameObject);
            }
        }
    }
    private void Explode()
    {
        trigger.enabled = true;
        exploded = true;
        explosionParticles.Play();
        explosionParticles.transform.parent = null;
        model.enabled = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!canExplode) return;
        if (collider.GetComponent<NumberedBlock>())
        {
            Destroy(collider.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canExplode) return;
        if(collision.collider.GetComponent<NumberedBlock>() || collision.collider.GetComponent<BombItem>())
        {
            Explode();
        }
    }
}
