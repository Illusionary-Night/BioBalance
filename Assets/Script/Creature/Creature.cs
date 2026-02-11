

using System.Collections;
using System.Collections.Generic;
using System;
//using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System.IO;


public partial class Creature : MonoBehaviour, ITickable
{
    

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
        AutoSetLayer(gameObject);
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


    public void OnTick()
    {
        if (isDead || this == null) return;

        UpdateVitalSigns();
        UpdateCooldowns();
        UpdateGrowth();

        if (stunTimer > 0)
        {
            isStunned = true;
            stunTimer--;
        }
        else
        {
            isStunned= false;
        }


        if (actionCooldown <= 0)DoAction();

        movement?.MoveOnTick();
    }
    
}
