using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvincibilityController : MonoBehaviour
{
    [SerializeField] private float invincibilityDuration = 0.5f; 
    // Invincibility duration, can be
    // adjusted in the Inspector
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    public bool IsInvincible => isInvincible; // Expose properties for other scripts to inspect

    // Activate invincibility

    // Start is called before the first frame update
    public void StartInvincibility(float duration = -1f)
    {
        isInvincible = true;
        invincibilityTimer = duration >= 0f ? duration : invincibilityDuration;
        Debug.Log($"{gameObject.name} is now invincible for {invincibilityTimer}s");
    }

    // End invincibility
    public void EndInvincibility()
    {
        isInvincible = false;
        invincibilityTimer = 0f;
        Debug.Log($"{gameObject.name} is no longer invincible");
    }

    private void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                EndInvincibility();
            }
        }
    }

}
