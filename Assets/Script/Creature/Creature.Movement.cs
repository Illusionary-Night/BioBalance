using System.Collections;
using System.Collections.Generic;
using System;
//using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System.IO;


public partial class Creature : MonoBehaviour, ITickable
{

    // 移動完成事件
    public event System.Action<Vector2Int> OnMovementComplete;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        movement.isColliding = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        movement.isColliding = false;
    }

    // 巢狀類別：Movement
    private class Movement
    {
        private Creature owner;
        private Rigidbody2D rb;                 // 優先使用物理剛體
        private Vector2Int Destination;         // 格座目標（整數格）
        public List<Vector2> path = null;      // 導航後的世界座標點 (連續)
        private int currentPathIndex = 0;
        private float stuckThreshold = 0.2f;    // 偵測被擠走/卡住的容忍距離
        private int stuckLimitTicks = 6;        // 超過幾次就重新導航
        private int stuckCounter = 0;
        private Vector2 lastRecordedPosition;
        private bool awake;
        public bool isColliding = false;
        private int stuck = 0;

        public Movement(Creature owner)
        {
            this.owner = owner;
            this.rb = owner.GetComponent<Rigidbody2D>(); // 可能為 null
                                                         // 初始化 lastRecordedPosition 為真實位置（權威）
            lastRecordedPosition = GetAuthoritativePosition();
            awake = false;
        }

        // 設定目的地（格座）
        public void SetDestination(Vector2Int destination)
        {
            // 安全檢查
            if (owner == null || owner.isDead) return;

            //Debug.Log("SetDestination");
            Destination = destination;
            awake = true;
            Navigate();
        }

        void PreventDrifting()
        {
            if (rb.linearVelocity != Vector2.zero && !isColliding)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        public void MoveOnTick()
        {
            // 安全檢查
            if (owner == null || owner.isDead || !awake || path == null || owner.isStunned)
            {
                return;
            }

            Vector2 currentActualPos = GetAuthoritativePosition();

            if (currentPathIndex < path.Count)
            {
                Vector2 target = path[currentPathIndex];

                // 1. 計算方向與預期速度
                Vector2 direction = (target - currentActualPos).normalized;
                Vector2 desiredVelocity = direction * owner.speed;

                // 2. 執行移動：直接給予物理速度
                // 這樣碰撞時，物理引擎可以自動把生物推開，而不會像 MovePosition 那樣硬擠
                rb.linearVelocity = desiredVelocity;

                // --- Stuck 判定邏輯 ---
                float actualMovedThisTick = Vector2.Distance(lastRecordedPosition, currentActualPos);

                // 物理系統中，如果被擋住，速度會被抵消，位移會變小
                if (actualMovedThisTick < (owner.speed * Time.fixedDeltaTime) * 0.5f)
                {
                    stuck++;
                }
                else
                {
                    stuck = 0;
                }

                lastRecordedPosition = currentActualPos;

                // 3. 轉向邏輯
                if (direction.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    // 物理旋轉建議使用 MoveRotation，這樣比較平滑且符合物理規則
                    rb.MoveRotation(angle);
                }

                // 4. 抵達路徑點判定 (稍微放寬一點距離，物理移動較難精準踩在點上)
                if (Vector2.Distance(currentActualPos, target) < 0.2f)
                {
                    currentPathIndex++;
                }
            }
            
            // 抵達目的地後的處理
            //rb.linearVelocity = Vector2.zero;

            Vector2 currentPos = GetAuthoritativePosition();
            if (Vector2.Distance(currentPos, Destination) < 1.8f)
            {
                //awake = false;
                owner.OnMovementComplete?.Invoke(Destination);
            }
        }

        // 導航呼叫 A* 或其它尋路系統
        public void Navigate()
        {
            // 安全檢查
            if (owner == null || owner.isDead) return;

            //Debug.Log("Navigate");
            Vector2Int start = Vector2Int.RoundToInt(GetAuthoritativePosition());
            Vector2Int goal = Destination;

            // 假設 AStar.FindPath 回傳 List<Vector2Int> 或 null
            // 使用 A* 演算法尋找路徑
            List<Vector2Int> rawPath = AStar.FindPath(start, goal, TerrainGenerator.Instance.GetDefinitionMap().GetTerrainWeight);
            if (rawPath == null || rawPath.Count == 0)
            {
                path = null;
                currentPathIndex = 0;
                return;
            }

            // 把格子座標轉成世界座標 (中心點)，視你的格子系統可能需要偏移
            path = rawPath.Select(v => new Vector2(v.x, v.y)).ToList();
            currentPathIndex = 0;
        }

        // 取得當前經過物理系統修正後的整數格座標（四捨五入）
        public Vector2Int GetVector2IntCurrentPosition()
        {
            Vector2 actual = GetAuthoritativePosition();
            return Vector2Int.RoundToInt(actual);
        }

        // 取得物理/Transform 的權威位置
        private Vector2 GetAuthoritativePosition()
        {
            // 安全檢查
            if (owner == null || owner.isDead || owner.gameObject == null)
                return Vector2.zero;

            if (rb != null) return rb.position;
            return owner.transform.position;
        }
        public Vector2Int GetDestination()
        {
            return Destination;
        }
        public int GetStuckTimes()
        {
            return stuck;
        }
        public void Push(Vector2 direction, float strength)
        {
            rb.AddForce(direction.normalized * strength, ForceMode2D.Impulse);
        }
    }

}
