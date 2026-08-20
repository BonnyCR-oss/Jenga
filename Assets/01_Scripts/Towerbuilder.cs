using System.Collections.Generic;
using UnityEngine;
// Asume que los hijos directos están agrupados por nivel: Level_0, Level_1, ... Level_17
public class TowerBuilder : MonoBehaviour
{
    public float blockHeight = 0.15f; // alto de un bloque (ajustar a tu prefab)
    public float blockWidth = 0.06f;
    [System.NonSerialized] public List<List<JengaBlock>> Levels = new List<List<JengaBlock>>();
    [System.NonSerialized] public int TopLevelIndex; // nivel más alto que sigue completo/en construcción

    void Awake()
    {
        BuildFromHierarchy();
    }

    void BuildFromHierarchy()
    {
        Levels.Clear();
        foreach (Transform levelTransform in transform)
        {
            var blocksInLevel = new List<JengaBlock>();
            foreach (Transform blockTransform in levelTransform)
            {
                var block = blockTransform.GetComponent<JengaBlock>();
                if (block == null) block = blockTransform.gameObject.AddComponent<JengaBlock>();
                block.Init(this, Levels.Count);
                blocksInLevel.Add(block);
            }
            Levels.Add(blocksInLevel);
        }
        TopLevelIndex = Levels.Count - 1;
    }

    // Un bloque solo se puede retirar si NO está en el nivel superior actual
    public bool CanRemove(JengaBlock block)
    {
        return block.LevelIndex < TopLevelIndex;
    }

    public void OnBlockRemovedSuccessfully(JengaBlock block)
    {
        int emptiedLevel = block.LevelIndex;
        Levels[emptiedLevel].Remove(block);
        if (Levels[emptiedLevel].Count == 0 && LevelHasBlocksAbove(emptiedLevel))
        {
            Debug.Log($"COLAPSO: nivel {emptiedLevel} quedó vacío con bloques encima.");
            var stability = GetComponent<StabilityMonitor>();
            if (stability != null)
            {
                stability.ForceFall();
            }
            return;
        }

        PlaceOnTop(block);
    }

    bool LevelHasBlocksAbove(int levelIndex)
    {
        for (int i = levelIndex + 1; i < Levels.Count; i++)
        {
            if (Levels[i].Count > 0) return true;
        }
        return false;
    }

    void PlaceOnTop(JengaBlock block)
    {
        var currentTop = Levels[TopLevelIndex];
        bool needsNewLevel = currentTop.Count >= 3;

        int targetLevel = needsNewLevel ? TopLevelIndex + 1 : TopLevelIndex;
        if (needsNewLevel)
        {
            Levels.Add(new List<JengaBlock>());
            TopLevelIndex = targetLevel;
        }

        var levelList = Levels[targetLevel];
        int slot = levelList.Count;
        levelList.Add(block);

        float rotationY = (targetLevel % 2 == 0) ? 0f : 90f;

        // Reparentar a la raíz de la torre ANTES de calcular/mover,
        block.transform.SetParent(transform, true);
        Vector3 localPos = SlotLocalPosition(slot, rotationY, targetLevel);
        block.MoveToTower(localPos, Quaternion.Euler(0, rotationY, 0), targetLevel);
    }

    Vector3 SlotLocalPosition(int slot, float rotationY, int level)
    {
        float offset = (slot - 1) * blockWidth;
        Vector3 local = (rotationY == 0f) ? new Vector3(offset, 0, 0) : new Vector3(0, 0, offset);
        local.y = level * blockHeight;
        return local;
    }
}