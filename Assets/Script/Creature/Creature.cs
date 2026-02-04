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
    private ActionStateMachine actionStateMachine;

    // 防止重複銷毀和訪問已銷毀物件的標記
    private bool isDead = false;
    public bool IsDead => isDead;

    private bool isInvincible = false;
    public bool IsInvincible => isInvincible;
    public void Initialize(Species species, CreatureAttributes creatureAttributes, GameObject creature_object)
    {
        mySpecies = species;
        AttributeInheritance(species, creatureAttributes, creature_object);
        //個體編號
        UUID = System.Guid.NewGuid().ToString();
        isDead = false;
        //角色物件調適
        transform.localScale = new Vector3(size * constantData.NORMAL_SIZE, size * constantData.NORMAL_SIZE, 1f);
        movement = new Movement(this);
        // 初始化狀態機
        actionStateMachine = new ActionStateMachine(this);
        // 生物圖片
        SetCreatureSprite(species.creatureBase);
        OnEnable();
    }

    public void OnEnable()
    {
        Manager.Instance.TickManager?.RegisterTickable(OnTick);
    }
    public void OnDisable()
    {
        Manager.Instance.TickManager?.UnregisterTickable(OnTick);
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

        //生成肉
        Manager.Instance?.EnvEntityManager.SpawnEntity(EntityData.SpawnableEntityType.Meat, transform.position);

        if (Manager.Instance != null)
        {
            Manager.Instance.UnregisterCreature(this);
        }
        
        // 使用物件池回收，而不是直接銷毀
        CreaturePool.ReleaseCreature(this);
    }

    /// <summary>
    /// 重置生物狀態（供物件池重用時調用）
    /// </summary>
    public void ResetState()
    {
        isDead = false;
        isInvincible = false;
        underAttackDirection = Direction.None;
        
        // 重置狀態機
        actionStateMachine = null;
        movement = null;
        
        // 重置冷卻
        ResetAllCooldowns();
    }

    public void OnTick()
    {
        // 安全檢查
        if (isDead || this == null) return;

        //殺死
        if (health > 0)
        {
            health += healthRegeneration;
        }
        else
        {
            if (isInvincible)
            {
                health = 0.1f;
            }
            else
            {
                Debug.Log("殺死");
                Die();
                return;
            }
        }
        health = Mathf.Min(health, maxHealth);

        //餓死
        hunger = Mathf.Min(hunger, maxHunger);
        if (hunger > 0)
        {
            hunger -= hungerRate;
        }
        else
        {
            if (isInvincible)
            {
                hunger = 0;
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
        if (age < lifespan)
        {
            age += 1;
        }
        else
        {
            if (isInvincible)
            {
                age = lifespan;
            }
            else
            {
                //Debug.Log("老死");
                Die();
                return;
            }
        }
        
        //行動冷卻
        if (actionCooldown > 0)
        {
            actionCooldown -= 1;
        }

        foreach (var key in actionCD.Keys.ToList())
        {
            if (actionCD[key] > 0)
            {
                actionCD[key] -= 1;
            }
        }

        if (actionCooldown <= 0)
        {
            DoAction();
        }

        movement?.MoveOnTick();


    }
    public void ResetAllCooldowns()
    {
        actionCooldown = 0;
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
    public void Hurt(float damage)
    {
        underAttackDirection = Direction.None;
        health -= damage;
        health = Mathf.Max(health, 0);
    }

    /// <summary> 執行傷害並記錄攻擊來源方位，用於觸發受傷逃跑判定或者之後進一步的動畫或特效 </summary>
    public void Hurt(float damage, Vector2 attackerPosition)
    {
        // 計算攻擊者相對於自己的方位向量
        Vector2 direction = attackerPosition - (Vector2)transform.position;
        underAttackDirection = GetDirectionFromVector(direction);
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
        return underAttackDirection != Direction.None;
    }

    /// <summary> 取得受擊方位</summary>
    public Direction GetUnderAttackDirection()
    {
        return underAttackDirection;
    }

    /// <summary> 取得受擊方位並立刻重置狀態，確保單次受傷僅觸發一次反應 </summary>
    public Direction GetAndResetUnderAttackDirection()
    {
        Direction result = underAttackDirection;
        underAttackDirection = Direction.None;
        return result;
    }
    //Hurt Section-------------------------------------------------------------------------


    public void SetCreatureSprite(CreatureBase baseType)
    {
        // 1. 將 Enum 轉為字串 (例如 "Slime")
        string spriteName = baseType.ToString();

        // 2. 從 Resources 加載 (路徑需放在 Resources/Sprites/ 下)
        Sprite loadedSprite = Resources.Load<Sprite>($"Sprites/{spriteName}");

        if (loadedSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = loadedSprite;
        }
        else
        {
            Debug.LogError($"找不到對應圖片: Sprites/{spriteName}");
        }
    }
}
