using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JengaBlock : MonoBehaviour
{
    [Header("Prefab de flecha")]
    public GameObject arrowPrefab;

    public int LevelIndex { get; private set; }
    public bool IsBeingRemoved { get; private set; }
    private TowerBuilder _tower;
    private GameObject _arrowA;
    private GameObject _arrowB;
    private Vector3 _slideAxis;
    private Vector3 _startLocalPos;

    private const float SlideDistance = 0.4f;
    private const float SlideSpeed = 1.2f;

    private int _slideDirection;
    public void Init(TowerBuilder tower, int levelIndex)
    {
        _tower = tower;
        LevelIndex = levelIndex;
    }

    void OnMouseDown()
    {
        TrySelect();
    }

    public void TrySelect()
    {
        if (!GameManager.Instance.CanInteract())
            return;

        if (IsBeingRemoved)
            return;

        if (!_tower.CanRemove(this))
        {
            GameManager.Instance.OnInvalidMove();
            return;
        }

        // Registrar este bloque como el seleccionado.
        // Si había otro, GameManager se encargará de cancelarlo.
        GameManager.Instance.SelectBlock(this);
        ShowArrows();
    }
    // FLECHAS

    void ShowArrows()
    {
        HideArrows();

        // Eje de deslizamiento según orientación del bloque.
        _slideAxis = transform.right;

        _arrowA = Instantiate(
            arrowPrefab,
            transform.position + _slideAxis * 0.15f,
            Quaternion.LookRotation(_slideAxis)
        );

        _arrowB = Instantiate(
            arrowPrefab,
            transform.position - _slideAxis * 0.15f,
            Quaternion.LookRotation(-_slideAxis)
        );

        AttachArrowHandler(_arrowA, 1);
        AttachArrowHandler(_arrowB, -1);
    }
    void AttachArrowHandler(GameObject arrow, int direction)
    {
        ArrowTapHandler handler = arrow.AddComponent<ArrowTapHandler>();

        handler.onTapped = () =>
        {
            StartSlide(direction);
        };
    }
    void HideArrows()
    {
        if (_arrowA != null)
        {
            Destroy(_arrowA);
            _arrowA = null;
        }

        if (_arrowB != null)
        {
            Destroy(_arrowB);
            _arrowB = null;
        }
    }
    public void CancelSelection()
    {
        if (IsBeingRemoved)
            return;

        HideArrows();

        GameManager.Instance.ClearSelectedBlock(this);
    }
    void StartSlide(int direction)
    {
        // Seguridad: solamente el bloque actualmente seleccionado
        // puede ejecutar el movimiento.
        if (GameManager.Instance.SelectedBlock != this)
            return;

        if (IsBeingRemoved)
            return;

        HideArrows();

        IsBeingRemoved = true;

        _slideDirection = direction;
        _startLocalPos = transform.localPosition;

        // Bloquear selección de otros bloques durante el movimiento.
        GameManager.Instance.BeginMove();

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        StartCoroutine(SlideRoutine());
    }

    System.Collections.IEnumerator SlideRoutine()
    {
        float traveled = 0f;

        while (traveled < SlideDistance)
        {
            float step = SlideSpeed * Time.deltaTime;

            transform.position +=
                _slideAxis * _slideDirection * step;

            traveled += step;

            yield return null;
        }

        FinishRemoval();
    }

    void FinishRemoval()
    {
        if (GameManager.Instance.TowerHasFallen)
            return;

        _tower.OnBlockRemovedSuccessfully(this);

    }
    public void MoveToTower(
     Vector3 localPos,
     Quaternion localRot,
     int newLevel)
    {
        LevelIndex = newLevel;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        StartCoroutine(MoveRoutine(localPos, localRot));
    }

    System.Collections.IEnumerator MoveRoutine(
        Vector3 targetLocalPos,
        Quaternion targetLocalRot)
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

        StabilityMonitor monitor = _tower.GetComponent<StabilityMonitor>();
        if (monitor != null)
        {
            monitor.RefreshBaseline(transform);
        }

        // Ahora sí, ya terminó completamente el movimiento del bloque.
        IsBeingRemoved = false;

        if (!GameManager.Instance.TowerHasFallen)
        {
            GameManager.Instance.OnValidMoveCompleted();
        }
    }
}