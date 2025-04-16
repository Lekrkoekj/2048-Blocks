using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombItem : MonoBehaviour
{
    public bool canExplode;
    [SerializeField] private float explosionRadius;

    [SerializeField] private SphereCollider trigger;

    [SerializeField] private ParticleSystem explosionParticles;

    [SerializeField] private MeshRenderer model;

    [SerializeField] private float explosionForceAmount;
    [SerializeField] private float explosionForceRadius;

    private void Explode()
    {
        trigger.enabled = true;
        explosionParticles.Play();
        explosionParticles.GetComponent<ExplosionSound>().PlaySound();
        explosionParticles.transform.parent = null;
        
        model.enabled = false;
        trigger.radius = explosionRadius;

        Invoke("DelayDestroy", 0.5f);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionForceRadius);

        foreach (Collider collider in colliders)
        {
            if(collider.GetComponent<NumberedBlock>() || collider.GetComponent<BombItem>())
            {
                collider.GetComponent<Rigidbody>().AddExplosionForce(explosionForceAmount, transform.position, explosionForceRadius, 0.75f, ForceMode.Force);
            }
        }
    }

    private void DelayDestroy()
    {
        Destroy(gameObject);
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
