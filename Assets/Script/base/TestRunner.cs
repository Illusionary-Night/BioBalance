using UnityEngine;

public class TestRunner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public bool ManualControl = true;
    [SerializeField] public Species goat;
    [SerializeField] public Species slime;
    [SerializeField] public GameObject GoatParent;
    [SerializeField] public GameObject SlimeParent;
    void Start()
    {
        // 使用物件池取得新生物
        Vector3 spawnPosition = new Vector3(250,250,0);
        Creature creature1 = CreaturePool.GetCreature(goat, goat.ToCreatureAttributes(), spawnPosition, GoatParent.transform);
        creature1.gameObject.name ="creature_"+creature1.speciesID+"_"+creature1.UUID;
        Manager.Instance.RegisterCreature(creature1);


        // 使用物件池取得新生物
        Vector3 spawnPosition2 = new Vector3(255, 255, 0);
        Creature creature2 = CreaturePool.GetCreature(slime, slime.ToCreatureAttributes(), spawnPosition2, SlimeParent.transform);
        creature2.gameObject.name = "creature_" + creature2.speciesID + "_" + creature2.UUID;
        Manager.Instance.RegisterCreature(creature2);
    }
}
