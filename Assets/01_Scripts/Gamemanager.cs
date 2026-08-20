using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Jugadores")]
    public int playerCount = 3;

    [Header("Eventos UI")]
    public UnityEvent<int> onTurnChanged;
    public UnityEvent<int> onPlayerLost;
    public UnityEvent onInvalidMove;

    public bool TowerHasFallen { get; private set; }
    public int CurrentPlayer { get; private set; }

    // Bloque actualmente seleccionado
    public JengaBlock SelectedBlock { get; private set; }

    // Evita seleccionar otro bloque mientras se está realizando el movimiento
    public bool MoveInProgress { get; private set; }

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

        CurrentPlayer = 0;
        TowerHasFallen = false;
        _gameOver = false;
        MoveInProgress = false;
        SelectedBlock = null;

        onTurnChanged?.Invoke(CurrentPlayer);
    }

    public void SetTrackingStable(bool stable)
    {
        _trackingStable = stable;
    }

    public bool CanInteract()
    {
        return _towerPlaced &&
               _trackingStable &&
               !_gameOver &&
               !TowerHasFallen &&
               !MoveInProgress;
    }

    // SELECCIÓN DE BLOQUES

    public void SelectBlock(JengaBlock block)
    {
        if (block == null)
            return;

        // Si ya había otro bloque seleccionado,
        // cancelamos automáticamente su selección.
        if (SelectedBlock != null && SelectedBlock != block)
        {
            SelectedBlock.CancelSelection();
        }

        SelectedBlock = block;
    }

    public void ClearSelectedBlock(JengaBlock block)
    {
        if (SelectedBlock == block)
        {
            SelectedBlock = null;
        }
    }
    public void CancelCurrentSelection()
    {
        if (SelectedBlock != null)
        {
            SelectedBlock.CancelSelection();
            SelectedBlock = null;
        }
    }

    // MOVIMIENTO

    public void BeginMove()
    {
        if (_gameOver || TowerHasFallen)
            return;

        MoveInProgress = true;
    }

    public void OnValidMoveCompleted()
    {
        // Limpiar selección
        if (SelectedBlock != null)
        {
            SelectedBlock.CancelSelection();
        }

        SelectedBlock = null;
        MoveInProgress = false;

        AdvanceTurn();
    }

    public void OnInvalidMove()
    {
        // Un movimiento inválido NO cambia el turno.
        onInvalidMove?.Invoke();
    }

    public void OnTowerFell()
    {
        if (_gameOver)
            return;
        Debug.Log($"TORRE CAYÓ (falso positivo si acabas de sacar un bloque) - jugador {CurrentPlayer + 1}");
        TowerHasFallen = true;
        _gameOver = true;
        MoveInProgress = false;

        if (SelectedBlock != null)
        {
            SelectedBlock.CancelSelection();
        }

        SelectedBlock = null;

        // CurrentPlayer es quien estaba realizando el movimiento.
        onPlayerLost?.Invoke(CurrentPlayer);
    }
    void AdvanceTurn()
    {
        CurrentPlayer = (CurrentPlayer + 1) % playerCount;
        Debug.Log($"Turno -> Jugador {CurrentPlayer + 1}");
        onTurnChanged?.Invoke(CurrentPlayer);
    }
}