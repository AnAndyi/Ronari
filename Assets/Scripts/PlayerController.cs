﻿using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float airWalkSpeed = 3f;
    public float jumpImpulse = 10f;
    public int jumpCount = 0; // Tracks total jumps used
    // Can change this if we want to jump more
    public float slideSpeed = 10f;
    public float slideDuration = 0.5f; // How long the slide lasts
    [SerializeField] private CapsuleCollider2D normalCollider;
    [SerializeField] private CapsuleCollider2D slideCollider; // Smaller size during slide
    private bool isSliding = false;
    private float slideTimer = 0f;
    [SerializeField] private float dodgeSpeed = 15f; // Dodge speed, adjustable in the Inspector
    [SerializeField] private float dodgeDuration = 0.3f; // The duration of the dodge can be adjusted in the Inspector
    private bool isDodging = false;
    private float dodgeTimer = 0f;
    [SerializeField] private float dodgeCooldown = 1f;
    private float dodgeCooldownTimer = 0f;
    private InvincibilityController invincibilityController;
    public CapsuleCollider2D ActiveCollider => isSliding ? slideCollider : normalCollider;

    public int maxJumps = 2; // Total allowed jumps (ground + 1 mid-air)
    Vector2 moveInput;
    TouchingDirections touchingDirections;
    Damageable damageable;
    public float CurrentMoveSpeed
    {
        get
        {
            if (CanMove)
            {
                if (IsMoving && !touchingDirections.IsOnWall)
                {
                    if (touchingDirections.IsGrounded)
                    {
                        if (IsRunning)
                        {
                            return runSpeed;
                        }
                        else
                        {
                            return walkSpeed;
                        }
                    }
                    else
                    {
                        // Air Move
                        return airWalkSpeed;
                    }
                }
                else
                {
                    // Idle Speed is 0.
                    return 0;
                }
            }
            else
            {
                // Movement locked
                return 0;
            }
        }
    }

    [SerializeField]
    private bool isMoving = false;
    public bool IsMoving
    {
        get { return isMoving; }
        private set
        {
            isMoving = value;
            if (animator != null) animator.SetBool(AnimationStrings.isMoving, value);
        }
    }

    [SerializeField]
    private bool isRunning = false;

    public bool IsRunning
    {
        get { return isRunning; }
        set
        {
            isRunning = value;
            if (animator != null) animator.SetBool(AnimationStrings.isRunning, value);
        }
    }

    public bool isFacingRight = true;

    public bool IsFacingRight
    {
        get { return isFacingRight; }
        private set
        {
            if (isFacingRight != value)
            {
                // Flip the local scale to make the player face the opposite direction
                transform.localScale *= new Vector2(-1, 1);
            }
            isFacingRight = value;
        }
    }

    public bool CanMove
    {
        get { return animator != null && animator.GetBool(AnimationStrings.canMove); }
    }

    public bool IsAlive
    {
        get { return animator != null && animator.GetBool(AnimationStrings.isAlive); }
    }

    Rigidbody2D rb;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();
        invincibilityController = GetComponent<InvincibilityController>();
        // Ensure colliders are assigned (can also assign via Inspector)
        if (invincibilityController == null)
        {
            Debug.LogError("InvincibilityController component missing on player!");
        }
        if (normalCollider == null || slideCollider == null)
        {
            CapsuleCollider2D[] colliders = GetComponents<CapsuleCollider2D>();
            if (colliders.Length >= 2)
            {
                normalCollider = colliders[0]; // First collider as normal
                slideCollider = colliders[1];  // Second as slide
            }
            else
            {
                Debug.LogError("Two CapsuleCollider2D components required!");
            }
        }
        // Set initial state
        if (normalCollider != null) normalCollider.enabled = true;
        if (slideCollider != null) slideCollider.enabled = false;
    }

    private void FixedUpdate()
    {
        if (!damageable.LockVelocity)
        {
            if (isSliding)
            {
                // Apply slide speed in facing direction
                float slideDirection = IsFacingRight ? 1f : -1f;
                rb.velocity = new Vector2(slideDirection * slideSpeed, rb.velocity.y);
                slideTimer -= Time.fixedDeltaTime;

                // 如果不在隧道内（IsOnCeiling为false）且时间结束，结束slide
                if (!touchingDirections.IsOnCeiling && slideTimer <= 0f)
                {
                    EndSlide();
                }
                // 如果在隧道内（IsOnCeiling为true），保持slide状态和动画
                else if (touchingDirections.IsOnCeiling)
                {
                    if (animator != null) animator.SetBool(AnimationStrings.isSliding, true);
                }
            }
            else if (isDodging)
            {
                float dodgeDirection = IsFacingRight ? 1f : -1f;
                rb.velocity = new Vector2(dodgeDirection * dodgeSpeed, rb.velocity.y);
                dodgeTimer -= Time.fixedDeltaTime;

                if (dodgeTimer <= 0f)
                {
                    EndDodge();
                }
            }
            else
            {
                rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);
            }
        }

        if (animator != null) animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
    }

    private void Update()
    {
        // Reset jump count when grounded
        if (touchingDirections.IsGrounded)
        {
            jumpCount = 0;
        }
        if (dodgeCooldownTimer > 0f)
        {
            dodgeCooldownTimer -= Time.deltaTime;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (IsAlive)
        {
            IsMoving = moveInput != Vector2.zero;
            SetFacingDirection(moveInput);
        }
        else
        {
            IsMoving = false;
        }
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !IsFacingRight)
        {
            // Face the right
            IsFacingRight = true;
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            // Face the left
            IsFacingRight = false;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsRunning = true;
        }
        else if (context.canceled)
        {
            IsRunning = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // Also check alive
        if (context.started && CanMove && jumpCount < maxJumps)
        {
            if (animator != null) animator.SetTrigger(AnimationStrings.jumpTrigger);
            rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
            jumpCount++;
        }
    }

    public void OnSlide(InputAction.CallbackContext context)
    {
        if (context.started && touchingDirections.IsGrounded && IsAlive && !isSliding && !isDodging)
        {
            StartSlide();
        }
    }

    private void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        if (normalCollider != null && slideCollider != null)
        {
            normalCollider.enabled = false;
            slideCollider.enabled = true;
            Debug.Log("Switched to SlideCollider");
        }
        if (animator != null)
        {
            animator.SetBool(AnimationStrings.isSliding, true);
            Debug.Log("Slide animation started");
        }
    }

    private void EndSlide()
    {
        // 仅在离开隧道时调用，避免反复切换
        if (!touchingDirections.IsOnCeiling)
        {
            isSliding = false;
            if (normalCollider != null && slideCollider != null)
            {
                normalCollider.enabled = true;
                slideCollider.enabled = false;
                Debug.Log("Switched to NormalCollider");
            }
            if (animator != null)
            {
                animator.SetBool(AnimationStrings.isSliding, false);
                Debug.Log("Slide animation ended");
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (animator != null) animator.SetTrigger(AnimationStrings.attackTrigger);
        }
    }

    public void OnRangedAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (animator != null) animator.SetTrigger(AnimationStrings.rangedAttackTrigger);
        }
    }

    public void OnHit(int damage, Vector2 knockback)
    {
        rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.started && IsAlive && !isDodging && dodgeCooldownTimer <= 0f)
        {
            StartDodge();
            dodgeCooldownTimer = dodgeCooldown;
        }
    }

    public void StartDodge()
    {
        isDodging = true;
        dodgeTimer = dodgeDuration;
        if (invincibilityController != null)
        {
            invincibilityController.StartInvincibility(dodgeDuration); // Invincibility duration is the same as dodge
        }
        if (animator != null)
        {
            animator.SetTrigger(AnimationStrings.dodgeTrigger);
        }
    }

    public void EndDodge()
    {
        isDodging = false;
        if (invincibilityController != null)
        {
            invincibilityController.EndInvincibility();
        }
    }
}