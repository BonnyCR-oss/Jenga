using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ArrowTapHandler : MonoBehaviour
{
    public Action onTapped;

    void OnMouseDown()
    {
        onTapped?.Invoke();
    }
}