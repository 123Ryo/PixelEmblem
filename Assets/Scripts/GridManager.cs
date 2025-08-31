using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public Tilemap groundTilemap;
    public Tilemap highlightTilemap; // 顯示移動範圍的 tilemap
    public Tile highlightTile; // 藍色 tile
    public Tilemap obstacleTilemap; // 障礙圖層
    public Tilemap attackTilemap; // 攻擊範圍圖層
    public Tile attackTile;       // 紅色 tile

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetWorldPositionFromGrid(Vector3Int gridPos)
    {
        return groundTilemap.GetCellCenterWorld(gridPos);
    }

    public Vector3Int GetGridPositionFromWorld(Vector3 worldPos)
    {
        return groundTilemap.WorldToCell(worldPos);
    }

    public bool IsWalkable(Vector3Int gridPos)
    {
        // 不可走條件：障礙物有 tile
        if (obstacleTilemap != null && obstacleTilemap.HasTile(gridPos))
            return false;

        // 地板沒有 tile → 也不可走
        if (!groundTilemap.HasTile(gridPos))
            return false;

        return true;
    }

    public bool HasUnitAt(Vector3Int gridPos)
    {
        Vector3 worldPos = GetWorldPositionFromGrid(gridPos);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null && hit.GetComponent<UnitController>() != null)
        {
            return true;
        }
        return false;
    }

    // 判斷該格子是否被角色佔據（用於移動範圍不能站上）
    public bool IsTileBlockedByUnit(Vector3Int gridPos)
    {
        return HasUnitAt(gridPos);
    }

    // 顯示 highlight tiles（藍色）
    public void ShowMoveRange(List<Vector3Int> positions)
    {
        Debug.Log("顯示移動範圍格子數量：" + positions.Count);

        foreach (var pos in positions)
        {
            highlightTilemap.SetTile(pos, highlightTile);
        }

        highlightTilemap.RefreshAllTiles();
    }

    // 清除所有 highlight tile(包含紅色攻擊範圍)
    public void ClearHighlights()
    {
        highlightTilemap.ClearAllTiles();
        attackTilemap.ClearAllTiles();
    }

    // 顯示 attackTilemap（紅色）
    public void ShowAttackRange(List<Vector3Int> positions)
    {
        foreach (var pos in positions)
        {
            attackTilemap.SetTile(pos, attackTile);
        }
        attackTilemap.RefreshAllTiles();
    }

    public UnitController GetUnitAt(Vector3Int gridPos)
    {
        Vector3 worldPos = GetWorldPositionFromGrid(gridPos);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null)
        {
            return hit.GetComponent<UnitController>();
        }
        return null;
    }

    /// <summary>
    /// ✅ 從場上移除指定格子的角色（由 CombatManager 呼叫）
    /// </summary>
    public void RemoveUnitAt(Vector3Int gridPos)
    {
        // 現在是物理檢查方式 → 不需要從 unitDict 等資料中移除
        // 這裡是為了搭配死亡後清理用的

        UnitController unit = GetUnitAt(gridPos);
        if (unit != null)
        {
            Debug.Log($"從 GridManager 移除 {unit.unitName} 位於 {gridPos} 的角色。");
            // 實際資料移除已由 CombatManager.Destroy(unit.gameObject) 處理
        }
    }

    
}
