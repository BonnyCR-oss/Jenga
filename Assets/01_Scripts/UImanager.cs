using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
//   - onTurnChanged   (Int32) -> UpdateTurn
//   - onPlayerLost    (Int32) -> ShowGameOver
//   - onInvalidMove   ()      -> ShowInvalidMove
public class UImanager : MonoBehaviour
{
    [Header("Turno")]
    public TMP_Text turnText;

    public GameObject invalidMovePanel;
    public TMP_Text invalidMoveText;
    public float invalidMoveDisplaySeconds = 1.5f;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public Button restartButton;
    private int _turnCallCount = 0;

    private Coroutine _invalidMoveRoutine;

    void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (invalidMovePanel != null)
            invalidMovePanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    public void UpdateTurn(int playerIndex)
    {
        _turnCallCount++;
        if (turnText != null)
            turnText.text = $"Turno: Jugador {playerIndex + 1}";
    }
    public void ShowInvalidMove()
    {
        if (invalidMovePanel == null || invalidMoveText == null)
            return;

        invalidMoveText.text = "Movimiento inválido";

        if (_invalidMoveRoutine != null)
            StopCoroutine(_invalidMoveRoutine);

        _invalidMoveRoutine = StartCoroutine(ShowInvalidMoveRoutine());
    }

    IEnumerator ShowInvalidMoveRoutine()
    {
        invalidMovePanel.SetActive(true);
        yield return new WaitForSeconds(invalidMoveDisplaySeconds);
        invalidMovePanel.SetActive(false);
        _invalidMoveRoutine = null;
    }

    public void ShowGameOver(int loserIndex)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = $"¡Jugador {loserIndex + 1} perdió!\nLa torre se derrumbó.";
    }

    // Enganchado al RestartButton en Awake().
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}