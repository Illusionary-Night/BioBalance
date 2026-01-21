using System.Collections.Generic;
using UnityEngine;

public class FleeAction : ActionBase
{
    // �k�]Ĳ�o���¯ٶZ���H��
    private const float THREAT_DISTANCE_THRESHOLD = 5f;
    // �k�]�ɪ��H���������ס]�Ω��׻١^
    private const float OBSTACLE_AVOIDANCE_ANGLE = 30f;
    // �k�]�Z���]�P���d�򤺪��H���Z���^
    private const float MIN_FLEE_RATE = 0.4f;
    private const float MAX_FLEE_RATE = 0.7f;

    public override ActionType Type => ActionType.Flee;

    /// <summary>
    /// �e�m�����ˬd�G
    /// 1. ���b�Q�����]������V���� None�^
    /// 2. �P���d�򤺦������̡]�Ӯ����̪������W��]�t�ۤv�^�B�Z���C���H��
    /// </summary>
    public override bool IsConditionMet(Creature creature)
    {
        // ���� 1�G���b�Q����
        if (creature.UnderAttack())
        {
            return true;
        }

        // ���� 2�G�P���d�򤺦������̥B�Z���L��
        List<Creature> predators = GetNearbyPredators(creature);
        if (predators.Count > 0)
        {
            // �ˬd�̪񪺮����̶Z���O�_�C���H��
            Creature nearestPredator = predators[0]; // �w���Z���ƦC
            float distance = Vector2.Distance(creature.transform.position, nearestPredator.transform.position);
            return distance < THREAT_DISTANCE_THRESHOLD;
        }

        return false;
    }

    public override float GetWeight(Creature creature)
    {
        // �k�]�u���Ÿ����A�S�O�O�b��������
        if (creature.UnderAttack())
        {
            return 5.0f;
        }
        return 1.3f;
    }

    public override bool IsSuccess(Creature creature)
    {
        // �k�]�欰�l�׹��հ���
        return true;
    }

    public override void Execute(Creature creature, ActionContext context = null)
    {
        // �����m�������A�A�קK������򺡨��ɭP����Ĳ�o
        creature.GetAndResetUnderAttackDirection();

        // �p��¯ٰѦ��I
        Vector2 threatPosition = GetThreatReferencePoint(creature);

        // �p��k�]��V�]�Ϥ�V�^
        Vector2 fleeDirection = CalculateFleeDirection(creature, threatPosition);

        // �p��k�]�ؼЦ�m
        Vector2Int fleeTarget = CalculateFleeTarget(creature, fleeDirection);

        // �ˬd�ؼЬO�_����
        Vector2Int currentPos = creature.GetRoundedPosition();
        if (fleeTarget == currentPos)
        {
            // �L�k�k�]�A��������
            context?.Complete();
            return;
        }

        // �ϥΪ��A�����U���ʦ^��
        var stateMachine = creature.GetStateMachine();

        // �ϥ� flag ����ư���
        bool hasCompleted = false;

        System.Action<Vector2Int> onArrived = (arrivedPosition) =>
        {
            // ����ư���
            if (hasCompleted)
            {
                return;
            }

            // �ˬd Context �O�_���M����
            if (context != null && !context.IsValid)
            {
                return;
            }

            hasCompleted = true;

            // �аO Action ����
            context?.Complete();
        };

        // �z�L���A�����U�^�ա]�۰ʺ޲z�M�z�^
        stateMachine.RegisterMovementCallback(onArrived);
        creature.MoveTo(fleeTarget);
    }

    /// <summary>
    /// ���o���񪺮����̲M��]���Z���Ƨǡ^
    /// </summary>
    private List<Creature> GetNearbyPredators(Creature creature)
    {
        return Perception.Creatures.GetAllTargets(creature, creature.predatorIDList);
    }

    /// <summary>
    /// ���o�¯ٰѦ��I�G
    /// - �Q�h�Ӯ����̰l�v�ɡA��̪ܳ񪺧@���Ѧ�
    /// - ���b�Q�����ɡA�ϥΨ�����V
    /// </summary>
    private Vector2 GetThreatReferencePoint(Creature creature)
    {
        Vector2 creaturePos = creature.transform.position;

        // �B�z�����̫¯١]�u���A�]��������V�w�Q���m�^
        List<Creature> predators = GetNearbyPredators(creature);
        if (predators.Count > 0)
        {
            // ��̪ܳ񪺮����̧@���¯ٰѦ��I
            return predators[0].transform.position;
        }

        // �L���T�¯ٮɡA�H����ܤ@�Ӥ�V�k�]
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return creaturePos + new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
    }

    /// <summary>
    /// �p��k�]��V�]�¯٪��Ϥ�V�^
    /// </summary>
    private Vector2 CalculateFleeDirection(Creature creature, Vector2 threatPosition)
    {
        Vector2 creaturePos = creature.transform.position;
        Vector2 fleeDirection = (creaturePos - threatPosition).normalized;

        // �p�G��V���s�V�q�A�H����ܤ@�Ӥ�V
        if (fleeDirection.sqrMagnitude < 0.001f)
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            fleeDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        }

        return fleeDirection;
    }

    /// <summary>
    /// �p��k�]�ؼЦ�m�A�]�t��ê����������
    /// </summary>
    private Vector2Int CalculateFleeTarget(Creature creature, Vector2 fleeDirection)
    {
        Vector2 creaturePos = creature.transform.position;
        float fleeDistance = Random.Range(creature.perceptionRange * MIN_FLEE_RATE, creature.perceptionRange * MAX_FLEE_RATE);

        // ���ժ����k�]��V
        Vector2Int targetPos = Vector2Int.RoundToInt(creaturePos + fleeDirection * fleeDistance);

        // �Y�e�観��ê�A���հ�������
        if (!IsPositionWalkable(targetPos))
        {
            // ���ե���
            Vector2 leftDirection = RotateVector(fleeDirection, OBSTACLE_AVOIDANCE_ANGLE);
            Vector2Int leftTarget = Vector2Int.RoundToInt(creaturePos + leftDirection * fleeDistance);
            if (IsPositionWalkable(leftTarget))
            {
                return leftTarget;
            }

            // ���եk��
            Vector2 rightDirection = RotateVector(fleeDirection, -OBSTACLE_AVOIDANCE_ANGLE);
            Vector2Int rightTarget = Vector2Int.RoundToInt(creaturePos + rightDirection * fleeDistance);
            if (IsPositionWalkable(rightTarget))
            {
                return rightTarget;
            }

            // �Y�ⰼ�����i��A�Y�u�k�]�Z��
            fleeDistance *= 0.5f;
            targetPos = Vector2Int.RoundToInt(creaturePos + fleeDirection * fleeDistance);
        }

        return targetPos;
    }

    /// <summary>
    /// ����V�q���w����
    /// </summary>
    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    /// <summary>
    /// �ˬd��m�O�_�i�q��
    /// </summary>
    private bool IsPositionWalkable(Vector2Int position)
    {
        // ��X��ڪ��a���ˬd�t��
        if (TerrainGenerator.Instance == null) return true;

        return TerrainGenerator.Instance.GetDefinitionMap().IsWalkable(position);
    }
}