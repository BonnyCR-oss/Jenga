using System.Collections.Generic;
using UnityEngine;

// Colocar en el root del prefab de la torre.
// Asume que los hijos directos están agrupados por nivel: Level_0, Level_1, ... Level_17
// y cada Level tiene 3 hijos = los 3 bloques de ese nivel.
public class TowerBuilder : MonoBehaviour
{
    public float blockHeight = 0.15f; // alto de un bloque (ajustar a tu prefab)

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

    // Llamado cuando un bloque se retira exitosamente (no cae la torre)
    public void OnBlockRemovedSuccessfully(JengaBlock block)
    {
        Levels[block.LevelIndex].Remove(block);
        PlaceOnTop(block);
    }

    void PlaceOnTop(JengaBlock block)
    {
        // ¿El nivel superior actual ya tiene 3 bloques? -> crear nivel nuevo, rotado 90°
        var currentTop = Levels[TopLevelIndex];
        bool needsNewLevel = currentTop.Count >= 3;

        int targetLevel = needsNewLevel ? TopLevelIndex + 1 : TopLevelIndex;
        if (needsNewLevel)
        {
            Levels.Add(new List<JengaBlock>());
            TopLevelIndex = targetLevel;
        }

        var levelList = Levels[targetLevel];
        int slot = levelList.Count; // posición 0,1,2 dentro del nivel
        levelList.Add(block);

        // Orientación: niveles pares e impares rotados 90° entre sí (estándar Jenga)
        float rotationY = (targetLevel % 2 == 0) ? 0f : 90f;
        Vector3 localPos = SlotLocalPosition(slot, rotationY, targetLevel);

        block.MoveToTower(localPos, Quaternion.Euler(0, rotationY, 0), targetLevel);
    }

    Vector3 SlotLocalPosition(int slot, float rotationY, int level)
    {
        // Separación entre los 3 bloques del nivel a lo largo del eje corto
        float offset = (slot - 1) * 0.06f; // ajustar según ancho real del bloque
        Vector3 local = (rotationY == 0f) ? new Vector3(offset, 0, 0) : new Vector3(0, 0, offset);
        local.y = level * blockHeight;
        return local;
    }
}