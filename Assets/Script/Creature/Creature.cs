

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
        data.species = species;
        GeneticsSystem.AttributeInheritance(data, species, parentAttr1, parentAttr2);
        //個體編號
        UUID = System.Guid.NewGuid().ToString();
        rb = GetComponent<Rigidbody2D>();
        ResetRuntimeStates();
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
    /*
        //TODO: 可能要考慮掛到Species上面
        private void SetCreatureSprite(CreatureBase baseType)
        {

            // 1. 將 Enum 轉為字串 (例如 "Slime")
            string spriteName = baseType.ToString();

            // 2. 從 Resources 加載 (路徑需放在 Resources/Sprites/ 下)
            Sprite loadedSprite = Resources.Load<Sprite>($"Sprites/{spriteName}");

            if (loadedSprite != null)
            {
                GetComponent<SpriteRenderer>().sprite = loadedSprite;

                var col = GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    // 讓它自動抓！
                    // 取圖片寬高之中較大的一半作為半徑，完美貼合任何形狀的生物圖片
                    float maxDim = Mathf.Max(loadedSprite.bounds.size.x, loadedSprite.bounds.size.y);
                    col.radius = maxDim * 0.5f;
                }
            }
            else
            {
                Debug.LogError($"找不到對應圖片: Sprites/{spriteName}");
            }
        }
        //TODO: 有點忘記他為什麼要存在的東西
        private void AutoSetLayer(GameObject obj)
        {
            // 將名稱轉為索引編號 (例如 "Creature" 是第 6 層，layerIndex 就會是 6)
            int layerIndex = LayerMask.NameToLayer("Creature");

            if (layerIndex == -1)
            {
                Debug.LogError("找不到名為 'Creature' 的 Layer，請先在選單中手動建立！");
                return;
            }

            // 設置該物件及其所有子物件的 Layer
            SetLayerRecursive(obj, layerIndex);
        }

        private void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }
        */
}
