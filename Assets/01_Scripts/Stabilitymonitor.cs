using System.Collections.Generic;
using UnityEngine;

// Colocar en el mismo GameObject que TowerBuilder (root de la torre).
public class StabilityMonitor : MonoBehaviour
{
    public float maxTiltDegrees = 20f;
    public float maxDriftMeters = 0.08f;

    private TowerBuilder _tower;
    private Dictionary<Transform, (Vector3 pos, Quaternion rot)> _initialState = new();
    private bool _fallTriggered;

    void Start()
    {
        _tower = GetComponent<TowerBuilder>();
        CaptureInitialState();
    }

    void CaptureInitialState()
    {
        foreach (var level in _tower.Levels)
            foreach (var block in level)
                _initialState[block.transform] = (block.transform.localPosition, block.transform.localRotation);
    }

    void Update()
    {
        if (_fallTriggered) return;

        foreach (var kvp in _initialState)
        {
            Transform t = kvp.Key;
            if (t == null) continue;

            JengaBlock block = t.GetComponent<JengaBlock>();
            if (block != null && block.IsBeingRemoved)
                continue; // se está moviendo a propósito, no cuenta como caída

            float tilt = Quaternion.Angle(t.localRotation, kvp.Value.rot);
            float drift = Vector3.Distance(
                new Vector3(t.localPosition.x, 0, t.localPosition.z),
                new Vector3(kvp.Value.pos.x, 0, kvp.Value.pos.z));

            if (tilt > maxTiltDegrees || drift > maxDriftMeters)
            {
                TriggerFall();
                return;
            }
        }
    }
    void TriggerFall()
    {
        _fallTriggered = true;
        GameManager.Instance.OnTowerFell();
    }

    // Permite que TowerBuilder dispare la caída cuando se vacía
    // por completo un nivel que tiene bloques encima.
    public void ForceFall()
    {
        if (_fallTriggered) return;
        TriggerFall();
    }

    // Llamar cada vez que un bloque se reubica exitosamente arriba, para no falsear la detección
    public void RefreshBaseline(Transform blockTransform)
    {
        _initialState[blockTransform] = (blockTransform.localPosition, blockTransform.localRotation);
    }
}