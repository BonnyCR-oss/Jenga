using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Jugadores")]
    public int playerCount = 3;

    [Header("Eventos UI (opcional, conectar en el inspector)")]
    public UnityEvent<int> onTurnChanged;      // índice de jugador (0-based)
    public UnityEvent<int> onPlayerLost;       // índice de jugador que hizo caer la torre
    public UnityEvent onInvalidMove;           // feedback de intento inválido

    public bool TowerHasFallen { get; private set; }
    public int CurrentPlayer { get; private set; } // 0-based

    private bool _trackingStable;
    private bool _towerPlaced;
    private bool _gameOver;
    private StabilityMonitor _stabilityMonitor;

    void Awake()
    {
        Instance = this;
    }

    public void OnTowerPlaced(GameObject towerRoot)
    {
        _towerPlaced = true;
        _stabilityMonitor = towerRoot.GetComponent<StabilityMonitor>();
        CurrentPlayer = 0; // partida comienza con Jugador 1
        onTurnChanged?.Invoke(CurrentPlayer);
    }

    public void SetTrackingStable(bool stable)
    {
        _trackingStable = stable;
    }

    // La manipulación solo se habilita con torre colocada, tracking estable y partida en curso
    public bool CanInteract()
    {
        return _towerPlaced && _trackingStable && !_gameOver && !TowerHasFallen;
    }

    public void OnInvalidMove()
    {
        // Regla: un movimiento inválido no cambia el turno
        onInvalidMove?.Invoke();
    }

    public void OnValidMoveCompleted()
    {
        AdvanceTurn();
    }

    public void OnTowerFell()
    {
        if (_gameOver) return;
        TowerHasFallen = true;
        _gameOver = true;
        onPlayerLost?.Invoke(CurrentPlayer);
    }

    void AdvanceTurn()
    {
        // Después del último jugador, el turno regresa al Jugador 1
        CurrentPlayer = (CurrentPlayer + 1) % playerCount;
        onTurnChanged?.Invoke(CurrentPlayer);
    }
}