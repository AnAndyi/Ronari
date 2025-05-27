using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    public Slider manaSlider;
    public TMP_Text manaBarText;

    private Damageable playerDamageable;

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.Log("No player found in the scene. Make sure it is tagged 'Player'");
        }

        playerDamageable = player.GetComponent<Damageable>();
    }

    void Start()
    {
        manaSlider.value = CalculateSliderPercentage(playerDamageable.Mana, playerDamageable.MaxMana);
        manaBarText.text = "MP " + playerDamageable.Mana + " / " + playerDamageable.MaxMana;
    }

    private void OnEnable()
    {
        if (playerDamageable != null)
        {
            playerDamageable.manaChanged.AddListener(OnPlayerManaChanged);
        }
    }

    private void OnDisable()
    {
        if (playerDamageable != null)
        {
            playerDamageable.manaChanged.RemoveListener(OnPlayerManaChanged);
        }
    }

    private float CalculateSliderPercentage(float currentMana, float maxMana)
    {
        return currentMana / maxMana;
    }

    private void OnPlayerManaChanged(int newMana, int maxMana)
    {
        manaSlider.value = CalculateSliderPercentage(newMana, maxMana);
        manaBarText.text = "MP " + newMana + " / " + maxMana;
        Canvas.ForceUpdateCanvases(); // 强制刷新UI
        
    }
}
