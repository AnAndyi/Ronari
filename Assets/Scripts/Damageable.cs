using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    public UnityEvent<int, Vector2> damageableHit;
    public UnityEvent damageableDeath;
    public UnityEvent<int, int> healthChanged;
    private InvincibilityController invincibilityController;

    Animator animator;
    [SerializeField]
    private int _maxHealth = 100;
    public int MaxHealth 
    {
        get
        {
            return _maxHealth;
        }
        set
        {
            _maxHealth = value;
        }
    }
    [SerializeField]
    private int _health = 100;

    public int Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
            healthChanged?.Invoke(_health, MaxHealth);

            // If health drops below 0, character is no longer alive.
            if(_health <= 0 )
            {
                IsAlive = false;
            }
        }
    }
    [SerializeField]
    private bool _isAlive = true;
    

    public bool IsAlive { 
        get
        {
            return _isAlive;
        }
        set
        {
            _isAlive = value;
            animator.SetBool(AnimationStrings.isAlive, value);
            Debug.Log("IsAlive set " + value);

            if(value == false)
            {
                damageableDeath.Invoke();
            }

        }
    }
    // The velocity should not be changed while this is true but needs to be respected by 
    // othe physics components like player controller
    public bool LockVelocity
    {
        get
        {
            return animator.GetBool(AnimationStrings.lockVelocity);
        }
        set
        {
            animator.SetBool(AnimationStrings.lockVelocity, value);
        }

    }
    private void Awake()
    {
        animator = GetComponent<Animator>();
        invincibilityController = GetComponent<InvincibilityController>();
    }

    // Returns whether the damageable took damage or not.
    public bool Hit(int damage, Vector2 knockback)
    {
        // Check the invincibility status and personal status
        if (IsAlive && (invincibilityController == null || !invincibilityController.IsInvincible))
        {
            Health -= damage;
            animator.SetTrigger(AnimationStrings.hitTrigger);
            LockVelocity = true;
            damageableHit?.Invoke(damage, knockback);
            CharacterEvents.characterDamaged.Invoke(gameObject, damage);
            Debug.Log($"{gameObject.name} took {damage} damage");
            return true;
        }
        else if (!IsAlive)
        {
            Debug.Log($"{gameObject.name} is dead, cannot be hit");
        }
        else
        {
            Debug.Log($"{gameObject.name} is invincible, hit ignored");
        }
        return false;
    }
    // Returns whether the character was healed or not
    public bool Heal(int healthRestore)
    {
        if(IsAlive && Health < MaxHealth)
        {
            int maxHeal = Mathf.Max(MaxHealth - Health, 0);
            int actualHeal = Mathf.Min(maxHeal, healthRestore);

            Health += actualHeal;
            CharacterEvents.characterHealed.Invoke(gameObject, actualHeal);
            return true;
        }
        return false;
    }
    
}
