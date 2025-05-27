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
    public UnityEvent<int, int> manaChanged;

    private InvincibilityController invincibilityController;

    Animator animator;
    [SerializeField] private int maxHealth = 100;
    public int MaxHealth
    {
        get { return maxHealth; }
        set { maxHealth = value; }
    }
    [SerializeField] private int health = 100;
    public int Health
    {
        get { return health; }
        set
        {
            health = value;
            healthChanged?.Invoke(health, MaxHealth);

            // If health drops below 0, character is no longer alive.
            if (health <= 0)
            {
                IsAlive = false;
            }
        }
    }

    [SerializeField] private int maxMana = 100; // 新增最大蓝量
    public int MaxMana
    {
        get { return maxMana; }
        set { maxMana = value; }
    }

    [SerializeField] private int mana = 100; // 新增当前蓝量 
    public int Mana
    {
        get { return mana; }
        set
        {
            int oldMana = mana;
            mana = Mathf.Clamp(value, 0, MaxMana); // 蓝量限制在0到MaxMana
            if (oldMana != mana) // 仅在蓝量变化时触发事件
            {
                manaChanged?.Invoke(mana, MaxMana);
                Debug.Log($"Mana updated from {oldMana} to {mana}/{MaxMana}");
            }
        }
    }
    [SerializeField] private bool isAlive = true;
    public bool IsAlive
    {
        get { return isAlive; }
        set
        {
            isAlive = value;
            if (animator != null) animator.SetBool(AnimationStrings.isAlive, value);
            if (value == false)
            {
                damageableDeath.Invoke();
            }
        }
    }
    // The velocity should not be changed while this is true but needs to be respected by 
    // other physics components like player controller
    public bool LockVelocity
    {
        get { return animator != null && animator.GetBool(AnimationStrings.lockVelocity); }
        set { if (animator != null) animator.SetBool(AnimationStrings.lockVelocity, value); }
    }

    [SerializeField] public float manaRegenRate = 2f; // 每秒回复2点，可调整
    [SerializeField] public float regenDelay = 2f; // 回复延迟2秒，可调整
    [SerializeField] private float manaRegenInterval = 0.5f; // 每0.5秒回复一次
    private float regenTimer = 0f; // 延迟计时器
    private float manaRegenTimer = 0f;
    private bool isRegenerating = false; // 是否在回复

    private void Awake()
    {
        animator = GetComponent<Animator>();
        invincibilityController = GetComponent<InvincibilityController>();
    }

    private void OnEnable()
    {
        damageableHit.AddListener(OnHit);
        damageableDeath.AddListener(OnDeath);
        healthChanged.AddListener(OnHealthChanged);
        manaChanged.AddListener(OnManaChanged);
    }

    private void OnDisable()
    {
        damageableHit.RemoveListener(OnHit);
        damageableDeath.RemoveListener(OnDeath);
        healthChanged.RemoveListener(OnHealthChanged);
        manaChanged.RemoveListener(OnManaChanged);
    }

    private void Update()
    {
        if (IsAlive && Mana < MaxMana)
        {
            if (!isRegenerating)
            {
                regenTimer += Time.deltaTime;
                if (regenTimer >= regenDelay)
                {
                    isRegenerating = true;
                    Debug.Log("Starting mana regeneration");
                }
            }

            if (isRegenerating)
            {
                manaRegenTimer += Time.deltaTime;
                if (manaRegenTimer >= manaRegenInterval) // 确保增量大于0
                {
                    int manaToAdd = Mathf.Max(1, (int)(manaRegenRate * manaRegenInterval)); // 按间隔计算
                    Mana += manaToAdd;
                    manaRegenTimer = 0f;
                }
            }
        }
    }

    // 减少蓝量
    public bool SpendMana(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("SpendMana called with negative amount; use RestoreMana instead.");
            return false;
        }
        if (Mana >= amount)
        {
            Mana -= amount;
            regenTimer = 0f; // 消耗后重置延迟
            isRegenerating = false; // 停止回复
            Debug.Log($"Spent {amount} mana, reset regen timer");
            return true; // 蓝量足够
        }
        Debug.Log("Not enough mana to spend!");
        return false; // 蓝量不足
    }

    // 恢复蓝量
    public bool RestoreMana(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("RestoreMana called with negative amount.");
            return false;
        }
        if (Mana < MaxMana)
        {
            Mana += Mathf.Min(amount, MaxMana - Mana);
            return true;
        }
        return false;
    }

    // Returns whether the damageable took damage or not.
    public bool Hit(int damage, Vector2 knockback)
    {
        // Check the invincibility status and personal status
        if (IsAlive && (invincibilityController == null || !invincibilityController.IsInvincible))
        {
            Health -= damage;
            if (animator != null) animator.SetTrigger(AnimationStrings.hitTrigger);
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
        if (IsAlive && Health < MaxHealth)
        {
            int maxHeal = Mathf.Max(MaxHealth - Health, 0);
            int actualHeal = Mathf.Min(maxHeal, healthRestore);

            Health += actualHeal;
            CharacterEvents.characterHealed.Invoke(gameObject, actualHeal);
            return true;
        }
        return false;
    }

    private void OnHit(int damage, Vector2 knockback) { }
    private void OnDeath() { }
    private void OnHealthChanged(int current, int max) { }
    private void OnManaChanged(int current, int max) { }
}