using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickupBase : MonoBehaviour
{
    public Vector3 spinRotationSpeed = new Vector3(0, 180, 0);
    protected AudioSource pickupSource;

    protected virtual void Awake()
    {
        pickupSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        transform.eulerAngles += spinRotationSpeed * Time.deltaTime;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        Damageable damageable = collision.GetComponent<Damageable>();
        if (damageable != null && ApplyPickup(damageable))
        {
            if (pickupSource != null)
            {
                AudioSource.PlayClipAtPoint(pickupSource.clip, transform.position, pickupSource.volume);
            }
            Destroy(gameObject);
        }
    }

    protected abstract bool ApplyPickup(Damageable damageable);
}