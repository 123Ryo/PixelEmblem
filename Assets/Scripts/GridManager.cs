using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// 全局單例管理器，負責所有網格相關的數據查詢、座標轉換及視覺化服務。
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance; 

    // --- Tilemap 設定 ---
    public Tilemap groundTilemap;    // 遊戲中的基礎地面圖層
    public Tilemap highlightTilemap; // 顯示移動範圍的 Tilemap 圖層
    public Tile highlightTile;       // 用於移動範圍的 藍色 tile
    public Tilemap obstacleTilemap;  // 障礙圖層
    public Tilemap attackTilemap;    // 顯示攻擊範圍的 Tilemap 圖層
    public Tile attackTile;          // 用於攻擊範圍的 紅色 tile

    private void Awake()
    {
        // 確保 GridManager 的單例初始化
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

    /// <summary>
    /// 判斷指定網格座標是否可以通行 (沒有障礙物且有地板)。
    /// </summary>
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

    /// <summary>
    /// 用在戰鬥系統中, 判斷指定網格座標是否有 單位 佔據。
    /// </summary>
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

    /// <summary>
    /// 用在移動系統中,判斷該格子是否被角色佔據並阻擋移動。(預留未來擴展,某些特殊單位不會阻擋移動)
    /// </summary>
    public bool IsTileBlockedByUnit(Vector3Int gridPos)
    {
        return HasUnitAt(gridPos);
    }

    /// <summary>
    /// 顯示移動範圍的藍色高亮 Tile。
    /// </summary>
    public void ShowMoveRange(List<Vector3Int> positions)
    {
        Debug.Log("顯示移動範圍格子數量：" + positions.Count);

        foreach (var pos in positions)
        {
            highlightTilemap.SetTile(pos, highlightTile);
        }

        highlightTilemap.RefreshAllTiles();
    }

    /// <summary>
    /// 清除所有高亮 Tilemap (包括移動範圍和攻擊範圍)。
    /// </summary>
    public void ClearHighlights()
    {
        highlightTilemap.ClearAllTiles();
        attackTilemap.ClearAllTiles();
    }

    /// <summary>
    /// 顯示攻擊範圍的紅色高亮 Tile。
    /// </summary>
    public void ShowAttackRange(List<Vector3Int> positions)
    {
        foreach (var pos in positions)
        {
            attackTilemap.SetTile(pos, attackTile);
        }
        attackTilemap.RefreshAllTiles();
    }

    /// <summary>
    /// 獲取指定網格座標上的 UnitController 實例。
    /// </summary>
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
}
