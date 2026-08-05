

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.IO;


public partial class Creature : MonoBehaviour, ITickable
{
    public Rigidbody2D rb;
    public CreatureData data;
    public SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Species species, CreatureAttributes? parentAttr1 = null, CreatureAttributes? parentAttr2 = null)
    {
        data = GeneticsSystem.AttributeInheritance(species, parentAttr1, parentAttr2);
        data.species = species;
        data.UUID = System.Guid.NewGuid().ToString();
        rb = GetComponent<Rigidbody2D>();
        ResetRuntimeStates();
        //角色物件調適
        transform.localScale = new Vector3(data.size * constantData.NORMAL_SIZE, data.size * constantData.NORMAL_SIZE, 1f);
        // 生物圖片
        VisualSystem.SetCreatureSprite(this, species.creatureBase);
        //TODO: 這個OnEnable到底開不開？
        // OnEnable();
        VisualSystem.AutoSetLayer(gameObject);
        // ColorSystem.UpdateVisuals(data);
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
        if (data.isDead) return;
        // 委派給狀態機處理
        data.actionStateMachine.EvaluateAndExecute();
    }


    public void Die()
    {
        // 防止重複執行
        if (data.isDead) return;
        data.isDead = true;

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
        if (data.isDead || this == null) return;

        UpdateVitalSigns();
        UpdateCooldowns();
        UpdateGrowth();
        ColorSystem.UpdateColorGenes(data);



        if (data.actionCooldown <= 0) DoAction();

        MovementSystem.OnTick(data, rb);
        //-----------------------------------
    }

}
