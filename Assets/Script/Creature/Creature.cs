/*
 * [區段名稱] Hurt Section
 * [區段說明] 負責 Creature 的生命值管理與受擊方向偵測。
 * [主要功能] 接收傷害數值、將攻擊者座標轉換為 8 向受擊方位、提供受擊狀態查詢。
 * [可用函式] void Hurt(int), void Hurt(int, Vector2), bool UnderAttack(), Direction GetUnderAttackDirection(), Direction GetAndResetDirection()
 * [測試區域] Inspector中的Debug Tools有拉桿可以設定受到攻擊的方向。
 */

using System.Collections;
using System.Collections.Generic;
using System;
//using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System.IO;


public partial class Creature : MonoBehaviour, ITickable
{
    public void OnEnable()
    {
        Manager.OnTick += OnTick;
    }
    public void OnDisable()
    {
        Manager.OnTick -= OnTick;
    }

    // 防止重複銷毀和訪問已銷毀物件的標記
    private bool isDead = false;
    public bool IsDead => isDead;

    private bool isInvincible = false;
    public bool IsInvincible => isInvincible;
    public void Initialize(CreatureAttributes creatureAttributes, GameObject creature_object)
    {
        AttributeInheritance(creatureAttributes, creature_object);
        //個體編號
        _UUID = System.Guid.NewGuid().ToString();
        isDead = false;
        //角色物件調適
        transform.localScale = new Vector3(size * constantData.NORMALSIZE, size * constantData.NORMALSIZE, 1f);
        movement = new Movement(this);
        // 初始化狀態機
        actionStateMachine = new ActionStateMachine(this);

        OnEnable();
    }
    public void DoAction()
    {
        if (isDead) return;
        // 委派給狀態機處理
        actionStateMachine.EvaluateAndExecute();
    }
    /// <summary>
    /// 取得狀態機實例（供 Action 使用）
    /// </summary>
    public ActionStateMachine GetStateMachine()
    {
        return actionStateMachine;
    }

    public void Die()
    {
        // 防止重複執行
        if (isDead) return;
        isDead = true;

        // 重要：先取消訂閱事件
        OnDisable();

        if (Manager.Instance != null)
        {
            // Spawn food item
            GameObject meat_prefab = Resources.Load<GameObject>("Prefabs/Edible/Meat");
            Instantiate(meat_prefab, transform.position, Quaternion.identity, Manager.Instance.EnvironmentEntities)
                .GetComponent<Edible>()
                .Initialize();
        }
        else
        {
            Debug.LogWarning("MeatPrefab is null");
        }
        
        if (Manager.Instance != null)
        {
            Manager.Instance.UnregisterCreature(this);
        }
        
        Destroy(gameObject);
    }
    public void OnTick()
    {
        // 安全檢查
        if (isDead || this == null) return;

        //test
        if (movement != null && movement.path != null)
        {
            //Debug.Log("path exist: " + (movement.path != null));
            //Debug.Log("Des:" + movement.GetDestination());
        }

        //殺死
        if (Health > 0)
        {
            Health += HealthRegeneration;
        }
        else
        {
            if (isInvincible)
            {
                Health = 0.1f;
            }
            else
            {
                //Debug.Log("殺死");
                Die();
                return;
            }
        }
        Health = Mathf.Min(Health, BaseHealth);

        //餓死
        hunger = Mathf.Min(hunger, maxHunger);
        if (Hunger > 0)
        {
            Hunger -= HungerRate;
        }
        else
        {
            if (isInvincible)
            {
                Hunger = 0;
            }
            else
            {
                //Debug.Log("餓死");
                Die();
                return;
            }
        }

        //老死
        age = Mathf.Max(age, 0);
        if (Age < Lifespan)
        {
            Age += 1;
        }
        else
        {
            if (isInvincible)
            {
                Age = Lifespan;
            }
            else
            {
                //Debug.Log("老死");
                Die();
                return;
            }
        }
        
        //行動冷卻
        if (ActionCooldown > 0)
        {
            ActionCooldown -= 1;
        }

        foreach (var key in actionCD.Keys.ToList())
        {
            if (actionCD[key] > 0)
            {
                actionCD[key] -= 1;
            }
        }

        if (ActionCooldown <= 0)
        {
            DoAction();
        }

        movement?.MoveOnTick();


    }
    public void ResetAllCooldowns()
    {
        ActionCooldown = 0;
        // 清空字典中的冷卻
        var keys = new List<ActionType>(actionCD.Keys);
        foreach (var key in keys) actionCD[key] = 0;
    }
    // set 1 then the creaute will never die 
    public void SetInvincible(bool isInvincible)
    {
        this.isInvincible = isInvincible;
    }

    //Hurt Section---------------------------------------------------------------------------

    /// <summary> 執行基礎傷害扣血，並確保生命值不低於 0 </summary>
    public void Hurt(int damage)
    {
        under_attack_direction = Direction.None;
        health -= damage;
        health = Mathf.Max(health, 0);
    }

    /// <summary> 執行傷害並記錄攻擊來源方位，用於觸發受傷逃跑判定或者之後進一步的動畫或特效 </summary>
    public void Hurt(int damage, Vector2 attackerPosition)
    {
        // 計算攻擊者相對於自己的方位向量
        Vector2 direction = attackerPosition - (Vector2)transform.position;
        under_attack_direction = GetDirectionFromVector(direction);
        health -= damage;
        health = Mathf.Max(health, 0);
    }

    /// <summary> 將向量轉換為 8 方向列舉，以 45 度角為一個判斷區間 </summary>
    private Direction GetDirectionFromVector(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return Direction.None;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle >= 337.5f || angle < 22.5f) return Direction.East;
        if (angle >= 22.5f && angle < 67.5f) return Direction.Northeast;
        if (angle >= 67.5f && angle < 112.5f) return Direction.North;
        if (angle >= 112.5f && angle < 157.5f) return Direction.Northwest;
        if (angle >= 157.5f && angle < 202.5f) return Direction.West;
        if (angle >= 202.5f && angle < 247.5f) return Direction.Southwest;
        if (angle >= 247.5f && angle < 292.5f) return Direction.South;
        if (angle >= 292.5f && angle < 337.5f) return Direction.Southeast;

        return Direction.None;
    }

    /// <summary> 檢查目前是否處於受擊狀態（方位不為 None 代表受擊中） </summary>
    public bool UnderAttack()
    {
        return under_attack_direction != Direction.None;
    }

    /// <summary> 取得受擊方位</summary>
    public Direction GetUnderAttackDirection()
    {
        return under_attack_direction;
    }

    /// <summary> 取得受擊方位並立刻重置狀態，確保單次受傷僅觸發一次反應 </summary>
    public Direction GetAndResetUnderAttackDirection()
    {
        Direction result = under_attack_direction;
        under_attack_direction = Direction.None;
        return result;
    }
    //Hurt Section-------------------------------------------------------------------------

}
