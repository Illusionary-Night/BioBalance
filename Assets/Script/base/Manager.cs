using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    // Singleton instance
    public static Manager Instance { get; private set; }

    // Initialize the EnvEntityManager
    public EnvEntityManager EnvEntityManager { get; private set; }

    // �K�Q�ݩʡG���ѹ� EnvironmentEntities ���X��
    public Transform EnvironmentEntities => EnvEntityManager?.EnvironmentEntities;

    [SerializeField] private readonly Dictionary<int, Species> species = new();
    public Dictionary<int, Species> Species => species;
    void Start()
    {
        Initialize();
    }
    private void Awake()
    {
        // One instance only
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // �b Awake ����l�� EnvEntityManager�A�T�O�b��L�}���� Start ���e�i��
        EnvEntityManager = new EnvEntityManager();
    }
    private void Initialize()
    {
        // Ensure TickManager exists
        if (TickManager.Instance == null)
        {
            new GameObject("TickManager").AddComponent<TickManager>();
        }

        // �ҥ� EnvEntityManager �� Tick �q�\
        EnvEntityManager?.OnEnable();
    }

    private void OnDisable()
    {
        EnvEntityManager?.OnDisable();
    }

    private void PredatorUpdate(Creature new_creature)
    {
        // ���o�s�ͪ������ةw�q
        var newSpecies = new_creature.mySpecies;

        // --- �Ĥ@�����G��X�֬O�o���s�ͪ����Ѽ� ---
        foreach (var speciesEntry in Manager.Instance.Species.Values)
        {
            // �p�G�o�Ӫ��ت��y���M��]�t�s�ͪ��� ID�A�����N�O�Ѽ�
            if (speciesEntry.preyIDList.Contains(newSpecies.speciesID))
            {
                // �N�Ӫ��� ID �[�J�s�ͪ����ѼĲM�� (�p�G�٨S�[�L)
                if (!new_creature.predatorIDList.Contains(speciesEntry.speciesID))
                {
                    new_creature.predatorIDList.Add(speciesEntry.speciesID);
                }
            }
        }

        // --- �ĤG�����G�i���s�ͪ����y���A�s�ͪ��O���̪��Ѽ� ---
        foreach (var preyID in newSpecies.preyIDList)
        {
            // ���ؼ��y������
            if (Manager.Instance.Species.TryGetValue(preyID, out var preySpecies))
            {
                // �M���Ӫ��ت��Ҧ�����
                foreach (var preyCreature in preySpecies.creatures.Values)
                {
                    // �i�D�y���G�s�ͪ������جO�A���Ѽ�
                    if (!preyCreature.predatorIDList.Contains(newSpecies.speciesID))
                    {
                        preyCreature.predatorIDList.Add(newSpecies.speciesID);
                    }
                }
            }
        }
    }
    public void RegisterCreature(Creature newCreature)
    {
        int id = newCreature.speciesID;

        // ����������ظ��
        if (!species.TryGetValue(id, out var speciesData))
        {
            // �o�O�s����
            speciesData = newCreature.mySpecies;
            species.Add(id, speciesData);

            // --- �۰ʤƮe���ͦ� ---
            // �b EnvironmentEntities �U�إߤ@�ӥH���ةR�W���Ū���
            GameObject container = new GameObject($"{speciesData.name}_Container");
            container.transform.SetParent(this.EnvironmentEntities);

            // �A�Ʀܥi�H��o�� Transform �s�i Species ���󤤡]�p�G Species ���w�d���^
            // speciesData.runtimeContainer = container.transform; 

            Debug.Log($"[Manager] ���U�s���بëإ߮e��: {speciesData.name}");
        }

        // �Τ@�B�z Parent ���
        // �o�̴M���~�إߩΤw�s�b���e��
        Transform targetContainer = EnvironmentEntities.Find($"{speciesData.name}_Container");
        if (targetContainer != null)
        {
            newCreature.transform.SetParent(targetContainer);
        }
        else
        {
            Debug.LogWarning($"container miss! {speciesData.name}_Container");
        }

        // �[�J�r��
        if (!speciesData.creatures.TryAdd(newCreature.UUID, newCreature))
        {
            return;
        }

        PredatorUpdate(newCreature);
    }
    public void UnregisterCreature(Creature deadCreature)
    {
        int id = deadCreature.speciesID;

        if (species.TryGetValue(id, out var speciesData))
        {
            if (speciesData.creatures.Remove(deadCreature.UUID))
            {
                // �u���������\�~��������޿�
                // �Ҧp�G�M�Ÿӥͪ��� CD �r��Ϊ��A�A�קK������^����ݯd�¸��
                // deadCreature.OnRecycle(); 
            }
        }
        else
        {
            Debug.LogWarning($"[Manager] ���յ��P�������ت��ͪ�: {id}");
        }
    }
}
