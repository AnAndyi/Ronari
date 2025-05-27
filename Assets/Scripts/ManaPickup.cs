using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaPickup : PickupBase
{
    public int manaRestore = 20;

    protected override bool ApplyPickup(Damageable damageable)
    {
        return damageable.RestoreMana(manaRestore);
    }
}