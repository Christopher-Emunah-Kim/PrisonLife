/// <summary>
/// MiningGrid 인스펙터 확장. "Generate Grid" 버튼으로 8×16 GridCell 자동 생성 및 _cells 배열 연결.
/// Prefab 필수 — 미연결 시 생성 불가.
/// 소유: Editor 전용 (빌드 제외)
/// 의존: MiningGrid, GridCell
/// </summary>
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MiningGrid))]
public class MiningGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("─── Grid Generator ───", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Inspector의 Cell Prefab 슬롯에 GridCell Prefab을 연결한 뒤 버튼을 누르세요.\n" +
            "기존 모든 자식 오브젝트를 삭제하고 8×16 GridCell을 재생성합니다.",
            MessageType.Info);

        if (GUILayout.Button("Generate Grid (8 × 16)", GUILayout.Height(30)))
        {
            GenerateGrid();
        }
    }

    private void GenerateGrid()
    {
        MiningGrid grid = (MiningGrid)target;

        SerializedObject so = new SerializedObject(grid);
        so.Update();

        // Prefab 필수 체크
        SerializedProperty prefabProp = so.FindProperty("_cellPrefab");
        GameObject cellPrefab = prefabProp?.objectReferenceValue as GameObject;

        if (cellPrefab == null)
        {
            EditorUtility.DisplayDialog(
                "GridCell Prefab 없음",
                "MiningGrid Inspector의 Cell Prefab 슬롯에 GridCell Prefab을 연결하세요.",
                "확인");
            return;
        }

        SerializedProperty spacingProp = so.FindProperty("_cellSpacing");
        float spacing = spacingProp != null ? spacingProp.floatValue : 1f;

        int cols = 8;
        int rows = 16;

        // 기존 자식 오브젝트 전부 삭제 (GridCell 유무 관계없이 전체)
        Transform gridTransform = grid.transform;
        for (int i = gridTransform.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(gridTransform.GetChild(i).gameObject);
        }

        GridCell[] cells = new GridCell[cols * rows];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                GameObject cellGO = (GameObject)PrefabUtility.InstantiatePrefab(cellPrefab, gridTransform);
                Undo.RegisterCreatedObjectUndo(cellGO, "Generate GridCell");

                cellGO.name                      = $"Cell_{col}_{row}";
                cellGO.transform.localPosition   = new Vector3(col * spacing, 0f, row * spacing);

                GridCell cellComp = cellGO.GetComponent<GridCell>();
                if (cellComp == null)
                {
                    cellComp = Undo.AddComponent<GridCell>(cellGO);
                }

                cells[row * cols + col] = cellComp;
            }
        }

        // _cells 배열 연결
        SerializedProperty cellsProp = so.FindProperty("_cells");
        cellsProp.arraySize = cells.Length;

        for (int i = 0; i < cells.Length; i++)
        {
            cellsProp.GetArrayElementAtIndex(i).objectReferenceValue = cells[i];
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(grid);

        Debug.Log($"[MiningGrid] {cols}×{rows} = {cells.Length}개 GridCell 생성 완료. spacing={spacing}");
    }
}
