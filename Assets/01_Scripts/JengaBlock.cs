using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JengaBlock : MonoBehaviour
{
    [Header("Prefab de flecha (asignar en el inspector o vía prefab)")]
    public GameObject arrowPrefab;

    public int LevelIndex { get; private set; }
    public bool IsBeingRemoved { get; private set; }

    private TowerBuilder _tower;
    private GameObject _arrowA, _arrowB;
    private Vector3 _slideAxis; // eje local de deslizamiento (perpendicular a la orientación del nivel)
    private Vector3 _startLocalPos;
    private const float SlideDistance = 0.4f;
    private const float SlideSpeed = 1.2f;
    private int _slideDirection; // -1, 0, +1

    public void Init(TowerBuilder tower, int levelIndex)
    {
        _tower = tower;
        LevelIndex = levelIndex;
    }

    void OnMouseDown() // en móvil, reemplazar por tu manejador de tap/raycast si no usás mouse simulation
    {
        TrySelect();
    }

    public void TrySelect()
    {
        if (!GameManager.Instance.CanInteract()) return;
        if (IsBeingRemoved) return;

        if (!_tower.CanRemove(this))
        {
            // Movimiento inválido: no cambia el turno, solo feedback
            GameManager.Instance.OnInvalidMove();
            return;
        }

        ShowArrows();
    }

    void ShowArrows()
    {
        HideArrows();
        // Eje de deslizamiento = orientación del bloque en su nivel actual
        _slideAxis = transform.right;

        _arrowA = Instantiate(arrowPrefab, transform.position + _slideAxis * 0.15f, Quaternion.LookRotation(_slideAxis));
        _arrowB = Instantiate(arrowPrefab, transform.position - _slideAxis * 0.15f, Quaternion.LookRotation(-_slideAxis));

        AttachArrowHandler(_arrowA, 1);
        AttachArrowHandler(_arrowB, -1);
    }

    void AttachArrowHandler(GameObject arrow, int direction)
    {
        var handler = arrow.AddComponent<ArrowTapHandler>();
        handler.onTapped = () => StartSlide(direction);
    }

    void HideArrows()
    {
        if (_arrowA != null) Destroy(_arrowA);
        if (_arrowB != null) Destroy(_arrowB);
    }

    void StartSlide(int direction)
    {
        HideArrows();
        IsBeingRemoved = true;
        _slideDirection = direction;
        _startLocalPos = transform.localPosition;

        // Habilitar física real en este bloque: al moverlo, puede golpear vecinos y tumbar la torre
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; // lo movemos a mano, pero mantiene collider activo para detectar choques

        StartCoroutine(SlideRoutine());
    }

    System.Collections.IEnumerator SlideRoutine()
    {
        float traveled = 0f;
        while (traveled < SlideDistance)
        {
            float step = SlideSpeed * Time.deltaTime;
            transform.position += _slideAxis * _slideDirection * step;
            traveled += step;
            yield return null;
        }

        FinishRemoval();
    }

    void FinishRemoval()
    {
        // El StabilityMonitor ya marcó GameManager.Instance.TowerHasFallen si la torre cayó
        // durante el deslizamiento. Si sigue en false, el movimiento fue válido y exitoso.
        if (GameManager.Instance.TowerHasFallen) return;

        _tower.OnBlockRemovedSuccessfully(this);
        GameManager.Instance.OnValidMoveCompleted();
    }

    public void MoveToTower(Vector3 localPos, Quaternion localRot, int newLevel)
    {
        LevelIndex = newLevel;
        IsBeingRemoved = false;
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Animación simple hacia la posición final sobre la torre
        StartCoroutine(MoveRoutine(localPos, localRot));
    }

    System.Collections.IEnumerator MoveRoutine(Vector3 targetLocalPos, Quaternion targetLocalRot)
    {
        float t = 0f;
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            transform.localPosition = Vector3.Lerp(startPos, targetLocalPos, t);
            transform.localRotation = Quaternion.Slerp(startRot, targetLocalRot, t);
            yield return null;
        }

        var monitor = _tower.GetComponent<StabilityMonitor>();
        if (monitor != null) monitor.RefreshBaseline(transform);
    }
}