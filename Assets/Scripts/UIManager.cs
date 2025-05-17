using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab;
    public GameObject healthTextPrefab;
    public Canvas gameCanvas;
    [SerializeField] private GameObject player;
    //Player GameObject reference, can be set in the Inspector
    private Damageable playerDamageable;// The player's Damageable component

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
        }

    }

    private void OnEnable()
    {
        CharacterEvents.characterDamaged += (CharacterTookDamage);
        CharacterEvents.characterHealed += (CharacterHealed);
    }

    private void OnDisable()
    {
        CharacterEvents.characterDamaged -= (CharacterTookDamage);
        CharacterEvents.characterHealed -= (CharacterHealed);
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

    public void OnExitGame(InputAction.CallbackContext context)
    {
        if(context.started)
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
    }
    // Added restart input processing
    public void OnRestart(InputAction.CallbackContext context)
    {
        if (context.started && playerDamageable != null && !playerDamageable.IsAlive)
        {
            RestartGame();
            Debug.Log("Restart key pressed, restarting game");
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
