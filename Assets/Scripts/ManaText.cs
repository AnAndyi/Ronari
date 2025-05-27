using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManaText : MonoBehaviour
{
    public Vector3 moveSpeed = new Vector3(0, 80, 0);
    public float timeToFade = 1f;

    RectTransform textTransform;
    TextMeshProUGUI textMeshPro;

    private float timeElapsed = 0f;
    private Color startColor;

    private void Awake()
    {
        textTransform = GetComponent<RectTransform>();
        textMeshPro = GetComponent<TextMeshProUGUI>();
        startColor = textMeshPro.color;
    }

    private void Update()
    {
        textTransform.position += moveSpeed * Time.deltaTime;

        timeElapsed += Time.deltaTime;
        if (timeElapsed < timeToFade)
        {
            float fadeAlpha = startColor.a * (1 - (timeElapsed / timeToFade));
            textMeshPro.color = new Color(startColor.r, startColor.g, startColor.b, fadeAlpha);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 支持显示固定字符串
    public void SetText(string message)
    {
        textMeshPro.text = message;
        startColor = textMeshPro.color;
    }

    // 重载以支持数字（保持兼容性）
    public void SetText(int manaChange)
    {
        textMeshPro.text = (manaChange > 0 ? "+" : "") + manaChange.ToString();
        startColor = textMeshPro.color;
        if (manaChange > 0) textMeshPro.color = Color.blue; // 回复显示蓝色
        else textMeshPro.color = Color.white; // 消耗显示白色
    }

}