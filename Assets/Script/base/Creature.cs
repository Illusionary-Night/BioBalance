using System.Collections;
using System.Collections.Generic;
using System;
//using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System.IO;


public partial class Creature : MonoBehaviour, ITickable
{


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
                Debug.Log("殺死");
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
                Debug.Log("餓死");
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
                Debug.Log("老死");
                Die();
                return;
            }
        }
        
        
        ////繁殖冷卻
        //if (ReproductionCooldown > 0)
        //{
        //    ReproductionCooldown -= 1;
        //}

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
        
        if (movement != null)
        {
            movement.MoveOnTick();
        }
    }
    public void ResetAllCooldowns()
    {
        ActionCooldown = 0;
        //ReproductionCooldown = 0;
        // 清空字典中的冷卻
        var keys = new List<ActionType>(actionCD.Keys);
        foreach (var key in keys) actionCD[key] = 0;
    }
    // set 1 then the creaute will never die 
    public void SetInvincible(bool isInvincible)
    {
        this.isInvincible = isInvincible;
    }
}
