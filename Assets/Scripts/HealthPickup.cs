using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : PickupBase
{
    public int healthRestore = 20;

    protected override bool ApplyPickup(Damageable damageable)
    {
        return damageable.Heal(healthRestore);
    }
}