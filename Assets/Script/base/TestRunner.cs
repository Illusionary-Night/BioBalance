using UnityEngine;

public class TestRunner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public CreatureAttributes CreatureAttributes;
    [SerializeField] public GameObject CreatureObject;
    void Start()
    {
        CreatureObject.GetComponent<Creature>().Initialize(CreatureAttributes,CreatureObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
