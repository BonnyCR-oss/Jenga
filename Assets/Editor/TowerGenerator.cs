#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Colocar este script en una carpeta llamada "Editor" dentro de Assets
// (Assets/Editor/TowerGenerator.cs). Fuera de una carpeta "Editor" no va a funcionar.
public class TowerGenerator : EditorWindow
{
    [MenuItem("Tools/Jenga AR/Generar Torre")]
    public static void GenerateTower()
    {
        GameObject blockPrefab = Selection.activeObject as GameObject;
        if (blockPrefab == null)
        {
            EditorUtility.DisplayDialog("Falta seleccionar",
                "Seleccioná el prefab 'Block' en el Project antes de correr esto.", "OK");
            return;
        }

        // Parámetros — AJUSTAR a las medidas reales de tu bloque
        int levels = 18;
        float blockHeight = 0.045f;   // igual al alto del bloque
        float blockLength = 0.225f;   // igual al largo del bloque
        float slotOffset = blockLength / 3f; // separación entre los 3 bloques de un nivel

        GameObject towerRoot = new GameObject("Tower");
        towerRoot.AddComponent<TowerBuilder>();
        towerRoot.AddComponent<StabilityMonitor>();

        for (int level = 0; level < levels; level++)
        {
            GameObject levelGO = new GameObject($"Level_{level}");
            levelGO.transform.SetParent(towerRoot.transform, false);
            levelGO.transform.localPosition = new Vector3(0, level * blockHeight, 0);

            bool rotated = (level % 2 == 1); // niveles impares rotados 90°
            levelGO.transform.localRotation = rotated ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;

            for (int slot = 0; slot < 3; slot++)
            {
                GameObject block = (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab, levelGO.transform);
                float offset = (slot - 1) * slotOffset;
                block.transform.localPosition = new Vector3(offset, 0, 0);
                block.transform.localRotation = Quaternion.identity;
                block.name = $"Block_{level}_{slot}";
            }
        }

        Selection.activeGameObject = towerRoot;
        EditorUtility.DisplayDialog("Listo", "Torre generada en la escena. Ahora arrastrala a Assets para guardarla como prefab.", "OK");
    }
}
#endif