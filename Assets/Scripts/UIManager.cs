using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab;
    public GameObject healthTextPrefab;
    public GameObject manaTextPrefab; // 新增蓝量文本预制件
    public Canvas gameCanvas;
    [SerializeField] private GameObject player;
    //Player GameObject reference, can be set in the Inspector
    private Damageable playerDamageable;// The player's Damageable component
    private int lastMana = -1; // 记录上一次蓝量
    private float manaTextTimer = 0f;
    private int accumulatedManaChange = 0;
    [SerializeField] private float manaTextInterval = 2f; // 每2秒显示一次
    [SerializeField] private int manaTextThreshold = 5; // 累计5点变化时显示

    private void Awake()
    {
        gameCanvas = FindObjectOfType<Canvas>();
        // Get the player and Damageable components
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player"); // 假设玩家有Player标签
            if (player == null)
            {
                Debug.LogError("UIManager: Player GameObject not found! Please assign in Inspector or set Player tag.");
            }
        }
        if (player != null)
        {
            playerDamageable = player.GetComponent<Damageable>();
            if (playerDamageable == null)
            {
                Debug.LogError("UIManager: Damageable component not found on player!");
            }
            else
            {
                lastMana = playerDamageable.Mana; // 初始化上一次蓝量
            }
        }
    }

    private void OnEnable()
    {
        CharacterEvents.characterDamaged += CharacterTookDamage;
        CharacterEvents.characterHealed += CharacterHealed;
        if (playerDamageable != null)
        {
            playerDamageable.manaChanged.AddListener(OnPlayerManaChanged);
        }
    }

    private void OnDisable()
    {
        CharacterEvents.characterDamaged -= CharacterTookDamage;
        CharacterEvents.characterHealed -= CharacterHealed;
        if (playerDamageable != null)
        {
            playerDamageable.manaChanged.RemoveListener(OnPlayerManaChanged);
        }
    }

    private void Update()
    {
        if (accumulatedManaChange > 0)
        {
            manaTextTimer += Time.deltaTime;
            if (accumulatedManaChange >= manaTextThreshold || manaTextTimer >= manaTextInterval)
            {
                if (player != null)
                {
                    Vector3 spawnPosition = Camera.main.WorldToScreenPoint(player.transform.position);
                    ManaText manaText = Instantiate(manaTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform)
                        .GetComponent<ManaText>();
                    manaText.SetText(accumulatedManaChange);
                    accumulatedManaChange = 0;
                    manaTextTimer = 0f;
                    Canvas.ForceUpdateCanvases();
                }
            }
        }
    }

    public void CharacterTookDamage(GameObject character, int damageReceived)
    {
        // Create text at character hit
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);

        TMP_Text tmpText = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform)
            .GetComponent<TMP_Text>();

        tmpText.text = damageReceived.ToString();
    }

    public void CharacterHealed(GameObject character, int healthRestored)
    {
        // Create text at character heald
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);

        TMP_Text tmpText = Instantiate(healthTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform)
            .GetComponent<TMP_Text>();

        tmpText.text = healthRestored.ToString();
    }

    // 新方法：显示“蓝量已耗尽”
    public void ShowManaDepletedMessage(GameObject character)
    {
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);
        ManaText manaText = Instantiate(manaTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform)
            .GetComponent<ManaText>();
        manaText.SetText("Mana has been exhausted");
    }

    private void OnPlayerManaChanged(int newMana, int maxMana)
    {
        if (playerDamageable != null && lastMana >= 0)
        {
            int manaChange = newMana - lastMana;
            lastMana = newMana;

            if (manaChange > 0 && newMana < maxMana)
            {
                accumulatedManaChange += manaChange;
            }
        }
    }

    public void OnExitGame()
    {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        Debug.Log(this.name + " : " + this.GetType() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif

#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#elif (UNITY_STANDALONE)
            Application.Quit();
#elif (UNITY_WEBGL)
            SceneManager.LoadScene("QuitScene");
#endif
    }

    public void OnRestart()
    {
        if (playerDamageable != null && !playerDamageable.IsAlive)
        {
            RestartGame();
            Debug.Log("Restart triggered, restarting game");
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}