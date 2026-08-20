using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ARTowerPlacer : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject towerPrefab;
    public ARPlaneManager planeManager;
    public GameManager gameManager;

    [Header("Estabilidad de tracking")]
    public float stableTrackingSeconds = 1.5f;

    private ARRaycastManager _raycastManager;
    private List<ARRaycastHit> _hits = new List<ARRaycastHit>();

    private GameObject _placedTower;
    private float _stableTimer;

    public bool TrackingStable { get; private set; }

    void Awake()
    {
        _raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        // Si ya colocamos la torre, solamente vigilamos el tracking.
        if (_placedTower != null)
        {
            UpdateTrackingStability();
            return;
        }

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began)
            return;

        Debug.Log("TOQUE DETECTADO");

        // Intentar detectar un plano.
        bool hitPlane = _raycastManager.Raycast(
            touch.position,
            _hits,
            TrackableType.PlaneWithinPolygon
        );

        if (!hitPlane)
        {
            Debug.Log("NO SE ENCONTRO UN PLANO");
            return;
        }

        Debug.Log("PLANO ENCONTRADO");

        Pose hitPose = _hits[0].pose;

        // Comprobar que tenemos prefab.
        if (towerPrefab == null)
        {
            Debug.LogError("ERROR: Tower Prefab no está asignado.");
            return;
        }

        // Crear torre.
        _placedTower = Instantiate(
            towerPrefab,
            hitPose.position,
            hitPose.rotation
        );

        Debug.Log("TORRE COLOCADA");

        // Dejar de mostrar planos.
        if (planeManager != null)
        {
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }

            planeManager.enabled = false;
        }

        // Avisar al GameManager.
        if (gameManager != null)
        {
            gameManager.OnTowerPlaced(_placedTower);
        }
    }

    void UpdateTrackingStability()
    {
        bool trackingOk =
            ARSession.state == ARSessionState.SessionTracking;

        if (trackingOk)
        {
            _stableTimer += Time.deltaTime;
        }
        else
        {
            _stableTimer = 0f;
        }

        TrackingStable =
            _stableTimer >= stableTrackingSeconds;

        if (gameManager != null)
        {
            gameManager.SetTrackingStable(TrackingStable);
        }
    }
}