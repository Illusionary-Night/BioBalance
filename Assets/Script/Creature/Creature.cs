

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.IO;


public partial class Creature : MonoBehaviour, ITickable
{
    private void Awake()
    {
        if (movement == null)
            movement = new Movement(this);

        if (actionStateMachine == null)
            actionStateMachine = new ActionStateMachine(this);

        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Species species, CreatureAttributes? parentAttr1 = null, CreatureAttributes? parentAttr2 = null)
    {
        mySpecies = species;
        AttributeInheritance(species, parentAttr1, parentAttr2);
        //個體編號
        UUID = System.Guid.NewGuid().ToString();

        ResetRuntimeStates();
        isDead = false;
        //角色物件調適
        transform.localScale = new Vector3(size * constantData.NORMAL_SIZE, size * constantData.NORMAL_SIZE, 1f);
        // 生物圖片
        SetCreatureSprite(species.creatureBase);
        //OnEnable();
        AutoSetLayer(gameObject);
        UpdateVisuals();
    }

    public void OnEnable()
    {
        MainManager.inGameManager.TickManager?.RegisterTickable(OnTick);
    }
    public void OnDisable()
    {
        MainManager.inGameManager.TickManager?.UnregisterTickable(OnTick);
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
        //TODO: 不該固定生成肉，應該根據生物屬性決定生成什麼東西（肉、骨頭、皮毛等），以及數量，可能是要放Species裡面
        MainManager.inGameManager?.EnvEntityManager.SpawnEntity(EntityData.SpawnableEntityType.Meat, transform.position);

        //TODO: 整合到CreaturePool 
        if (MainManager.inGameManager != null)
        {
            MainManager.inGameManager.UnregisterCreature(this);
        }
        //TODO:------------------------

        // 使用物件池回收，而不是直接銷毀
        CreaturePool.ReleaseCreature(this);
    }


    public void OnTick()
    {
        //TODO: 這邊要決定各種東西多久Update一次
        if (isDead || this == null) return;

        UpdateVitalSigns();
        UpdateCooldowns();
        UpdateGrowth();
        UpdateColorGenes();



        if (actionCooldown <= 0) DoAction();

        movement?.MoveOnTick();
        //-----------------------------------
    }

}
