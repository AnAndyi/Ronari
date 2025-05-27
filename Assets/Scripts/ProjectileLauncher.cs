using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    public Transform launchPoint;

    public GameObject projectilePrefab;

    [SerializeField] private int manaCost = 20; // 射击消耗的蓝量，可调整
    private Damageable playerDamageable;
    private UIManager uiManager;

    private void Awake()
    {
        GameObject player = transform.root.gameObject;
        playerDamageable = player.GetComponent<Damageable>();
        uiManager = FindObjectOfType<UIManager>(); // 查找UIManager
        if (playerDamageable == null)
        {
            Debug.LogError("No Damageable component found on player!");
        }
        if (uiManager == null)
        {
            Debug.LogError("No UIManager found in scene!");
        }
    }
    public void FireProjectile()
    {
        if (playerDamageable != null && playerDamageable.SpendMana(manaCost))
        {
            GameObject projectile = Instantiate(projectilePrefab, launchPoint.position, projectilePrefab.transform.rotation);
            Vector3 origScale = projectile.transform.localScale;

            // Flip the projectile's facing direction and movment based on the direction the character is facing.
            projectile.transform.localScale = new Vector3(
                origScale.x * transform.localScale.x > 0 ? 1 : -1,
                origScale.y * transform.localScale.x > 0 ? 1 : -1,
                origScale.z
            );
            Debug.Log($"Fired projectile, consumed {manaCost} MP");
        }   
        else
        {
            Debug.Log("Not enough mana to fire projectile!");
            if (uiManager != null)
            {
                uiManager.ShowManaDepletedMessage(gameObject); // 蓝量不足时显示“蓝量已耗尽”
            }
        }
    }
  
}
