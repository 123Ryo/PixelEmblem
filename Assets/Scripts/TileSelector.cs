using UnityEngine;
using System.Collections.Generic;

public class TileSelector : MonoBehaviour
{
    private UnitController selectedUnit;

    [Header("UI 顯示控制器")]
    public UnitStatusUI statusUI;

    [Header("攻擊場景管理器")]
    public AttackSceneManager attackSceneManager;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // 嘗試點選角色
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
            if (hit != null)
            {
                UnitController unit = hit.GetComponent<UnitController>();
                if (unit != null)
                {
                    statusUI.ShowUnit(unit); // ✅ 顯示角色狀態

                    Vector3Int unitGridPos = GridManager.Instance.GetGridPositionFromWorld(unit.transform.position);

                    if (unit.faction == UnitFaction.Player)
                    {
                        // ✅ 若該角色已經行動過，無法再選取
                        if (unit.hasActed)
                        {
                            Debug.Log("該角色本回合已行動，無法再操作");
                            return;
                        }

                        selectedUnit = unit;

                        var moveRange = unit.GetMoveRange(unitGridPos, unit.moveRange, unit);
                        GridManager.Instance.ClearHighlights();
                        GridManager.Instance.ShowMoveRange(moveRange);

                        var attackRange = unit.GetAttackRange(moveRange, unit.attackRange);
                        GridManager.Instance.ShowAttackRange(attackRange);

                        return;
                    }
                    else
                    {
                        // 是敵人 → 嘗試攻擊
                        if (selectedUnit != null)
                        {
                            Vector3Int enemyGridPos = GridManager.Instance.GetGridPositionFromWorld(unit.transform.position);
                            Vector3Int selectedGridPos = GridManager.Instance.GetGridPositionFromWorld(selectedUnit.transform.position);

                            var moveRange = selectedUnit.GetMoveRange(selectedGridPos, selectedUnit.moveRange);
                            var attackRange = selectedUnit.GetAttackRange(moveRange, selectedUnit.attackRange);

                            if (attackRange.Contains(enemyGridPos))
                            {
                                int distanceToEnemy = Mathf.Abs(selectedGridPos.x - enemyGridPos.x) + Mathf.Abs(selectedGridPos.y - enemyGridPos.y);

                                if (distanceToEnemy <= selectedUnit.attackRange)
                                {
                                    if (attackSceneManager != null)
                                    {
                                        CombatManager combatManager = FindObjectOfType<CombatManager>();
                                        if (combatManager != null)
                                        {
                                            combatManager.ResolveBattle(selectedUnit, unit, true);
                                        }
                                        else
                                        {
                                            Debug.LogWarning("CombatManager 找不到！");
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogWarning("AttackSceneManager 尚未指定！");
                                    }

                                    selectedUnit.hasActed = true; // ✅ 標記已行動
                                    selectedUnit = null;
                                    GridManager.Instance.ClearHighlights();
                                    statusUI.Hide();
                                }
                                else
                                {
                                    Vector3Int? attackPosition = null;
                                    foreach (var pos in moveRange)
                                    {
                                        int dist = Mathf.Abs(pos.x - enemyGridPos.x) + Mathf.Abs(pos.y - enemyGridPos.y);
                                        if (dist <= selectedUnit.attackRange && !GridManager.Instance.HasUnitAt(pos))
                                        {
                                            attackPosition = pos;
                                            break;
                                        }
                                    }

                                    if (attackPosition.HasValue)
                                    {
                                        UnitController attackerUnit = selectedUnit;

                                        System.Action onMoveComplete = null;
                                        onMoveComplete = () =>
                                        {
                                            CombatManager combatManager = FindObjectOfType<CombatManager>();
                                            if (combatManager != null)
                                            {
                                                combatManager.ResolveBattle(attackerUnit, unit, true);
                                            }

                                            attackerUnit.hasActed = true; // ✅ 標記已行動
                                            selectedUnit = null;
                                            GridManager.Instance.ClearHighlights();
                                            statusUI.Hide();

                                            attackerUnit.OnMoveComplete -= onMoveComplete;
                                        };

                                        attackerUnit.OnMoveComplete += onMoveComplete;
                                        attackerUnit.MoveTo(attackPosition.Value);
                                    }
                                    else
                                    {
                                        Debug.Log("找不到可移動至攻擊敵人的位置");
                                    }
                                }

                                return;
                            }
                        }

                        selectedUnit = null;

                        var enemyMoveRange = unit.GetMoveRange(unitGridPos, unit.moveRange, unit);
                        GridManager.Instance.ClearHighlights();
                        GridManager.Instance.ShowMoveRange(enemyMoveRange);

                        var enemyAttackRange = unit.GetAttackRange(enemyMoveRange, unit.attackRange);
                        GridManager.Instance.ShowAttackRange(enemyAttackRange);

                        return;
                    }
                }
            }

            // 沒點到角色 → 嘗試移動
            if (selectedUnit != null)
            {
                Vector3Int gridPos = GridManager.Instance.GetGridPositionFromWorld(mouseWorldPos);

                if (GridManager.Instance.IsWalkable(gridPos))
                {
                    var moveRange = selectedUnit.GetMoveRange(
                        GridManager.Instance.GetGridPositionFromWorld(selectedUnit.transform.position),
                        selectedUnit.moveRange
                    );

                    if (moveRange.Contains(gridPos) && !GridManager.Instance.HasUnitAt(gridPos))
                    {
                        UnitController movedUnit = selectedUnit;

                        System.Action onMoveComplete = null;
                        onMoveComplete = () =>
                        {
                            movedUnit.hasActed = true; // ✅ 不論是否能攻擊都標記已行動
                            selectedUnit = null;
                            GridManager.Instance.ClearHighlights();
                            statusUI.Hide();

                            movedUnit.OnMoveComplete -= onMoveComplete;
                        };

                        movedUnit.OnMoveComplete += onMoveComplete;
                        movedUnit.MoveTo(gridPos);
                        return;
                    }
                }
            }

            // 非合法 → 取消選取
            selectedUnit = null;
            GridManager.Instance.ClearHighlights();
            statusUI.Hide();
        }
    }

    /// <summary>
    /// 根據可移動範圍，擴張取得攻擊範圍，排除移動格子本身
    /// </summary>
    private List<Vector3Int> GetAttackRangeExcludingMove(List<Vector3Int> moveRange, int attackRange)
    {
        HashSet<Vector3Int> result = new HashSet<Vector3Int>();

        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1)
        };

        foreach (var pos in moveRange)
        {
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
            queue.Enqueue(pos);
            visited.Add(pos);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int distance = Mathf.Abs(pos.x - current.x) + Mathf.Abs(pos.y - current.y);

                if (distance > attackRange)
                    continue;

                bool isWithinMap = GridManager.Instance.groundTilemap.HasTile(current) ||
                                   (GridManager.Instance.obstacleTilemap != null && GridManager.Instance.obstacleTilemap.HasTile(current));

                if (isWithinMap)
                {
                    if (!moveRange.Contains(current))
                    {
                        result.Add(current);
                    }

                    foreach (var dir in directions)
                    {
                        var next = current + dir;
                        if (!visited.Contains(next))
                        {
                            visited.Add(next);
                            queue.Enqueue(next);
                        }
                    }
                }
            }
        }

        return new List<Vector3Int>(result);
    }
}
