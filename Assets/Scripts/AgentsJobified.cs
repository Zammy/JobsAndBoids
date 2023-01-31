using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Jobs;

[BurstCompile]
public static class AgentsJobified
{
    const int START_CAPACITY = 1024;

    static AgentsJobified()
    {
        _kdTree = new KDTree(START_CAPACITY, Allocator.Persistent);
        _agentsTransformAccess = new TransformAccessArray(START_CAPACITY, -1);
        _positions = new NativeList<float3>(START_CAPACITY, Allocator.Persistent);
        _rotations = new NativeList<quaternion>(START_CAPACITY, Allocator.Persistent);
        _leadership = new NativeList<float>(START_CAPACITY, Allocator.Persistent);
    }

    public static void NewAgentSpawned(Agent agent)
    {
        _agentsTransformAccess.Add(agent.transform);
        _positions.Add(agent.transform.position);
        _rotations.Add(agent.transform.localRotation);
        _leadership.Add(agent.Leadership);
    }

    public static void Tick(float dt)
    {
        var settings = Playground.Instance.AgentSettings;

        for (int i = 0; i < _positions.Length; i++)
        {
            _positions[i] = _agentsTransformAccess[i].localPosition;
            _rotations[i] = _agentsTransformAccess[i].localRotation;
        }

        var positions = _positions.AsArray();
        var rotations = _rotations.AsArray();
        var leaderships = _leadership.AsArray();

        var buildTreeJobHandle = _kdTree.BuildTree(positions);

        var calcForcesJob = new CalcSeparationContainmentFlockCenterJob()
        {
            Positions = positions,
            Tree = _kdTree,
            NeighbourhoodRange = settings.NeighbourhoodRange,
            PlaygroundBounds = Playground.Instance.Size,

            SeparationForces = new NativeArray<float3>(positions.Length, Allocator.TempJob),
            FlockCenters = new NativeArray<float3>(positions.Length, Allocator.TempJob),
            ContainmentForces = new NativeArray<float3>(positions.Length, Allocator.TempJob),
        };
        _job1 = calcForcesJob;
        var calcForcesJobHandle = calcForcesJob.Schedule(positions.Length, 16, buildTreeJobHandle);

        var leaderJob = new ChooseLeaderAndCalcCohesionForceJob()
        {
            Positions = positions,
            Rotations = rotations,
            Tree = _kdTree,
            NeighbourhoodRange = settings.NeighbourhoodRange,
            Leaderships = leaderships,
            FlockCenters = calcForcesJob.FlockCenters,
            ChoosenLeader = new NativeArray<int>(positions.Length, Allocator.TempJob),
            CohesionForces = new NativeArray<float3>(positions.Length, Allocator.TempJob),
            AlignmentForces = new NativeArray<float3>(positions.Length, Allocator.TempJob),
        };
        _job2 = leaderJob;
        var leaderJobHandle = leaderJob.Schedule(positions.Length, 16, calcForcesJobHandle);

        _moveJobHandle = new MoveJob()
        {
            Leaderships = leaderships,

            SeparationForces = calcForcesJob.SeparationForces,
            CohesionForces = leaderJob.CohesionForces,
            AlignmentForces = leaderJob.AlignmentForces,
            ContainmentForces = calcForcesJob.ContainmentForces,

            ForwardWeight = settings.ForwardWeight,
            SeparationWeight = settings.SeparationWeight,
            CohesionWeight = settings.CohesionWeight,
            AlignmentWeight = settings.AlignmentWeight,
            ContainmentWeight = settings.ContainmentWeight,
            AgentSpeed = settings.Speed,
            DeltaTime = dt,
            RotationCoefficient = settings.RotationCoefficient
        }.Schedule(_agentsTransformAccess, leaderJobHandle);
    }

    public static void LateTick()
    {
        _moveJobHandle.Complete();

        _job1.SeparationForces.Dispose();
        _job1.FlockCenters.Dispose();
        _job1.ContainmentForces.Dispose();

        _job2.Leaderships.Dispose();
        _job2.ChoosenLeader.Dispose();
        _job2.CohesionForces.Dispose();
        _job2.AlignmentForces.Dispose();
    }

    static KDTree _kdTree;
    static NativeList<float3> _positions;
    static NativeList<quaternion> _rotations;
    static NativeList<float> _leadership;
    static TransformAccessArray _agentsTransformAccess;
    static JobHandle _moveJobHandle;

    static CalcSeparationContainmentFlockCenterJob _job1;
    static ChooseLeaderAndCalcCohesionForceJob _job2;

    [BurstCompile]
    struct CalcSeparationContainmentFlockCenterJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> Positions;
        [ReadOnly] public KDTree Tree;
        [ReadOnly] public float NeighbourhoodRange;
        [ReadOnly] public float2 PlaygroundBounds;

        [NativeSetThreadIndex] private int ThreadIndex;


        public NativeArray<float3> SeparationForces;
        public NativeArray<float3> FlockCenters;
        public NativeArray<float3> ContainmentForces;

        public void Execute(int index)
        {
            var neighbours = new NativeArray<KDTree.Neighbour>(16, Allocator.Temp);
            var selfPos = Positions[index];
            int neighbourhoodSize = Tree.GetEntriesInRange(index, in selfPos, NeighbourhoodRange, ref neighbours, ThreadIndex);

            var flockCenter = selfPos;
            for (int i = 0; i < neighbourhoodSize; i++)
            {
                var neighbour = neighbours[i];
                if (neighbour.index == index) //This should not be needed but it seems queryingIndex is not being used properly from GetEntriesInRange()
                    continue;

                int otherIndex = neighbour.index;
                float3 otherPos = neighbour.position;
                float distanceSqrd = neighbour.distSq;

                float3 diff = selfPos - otherPos;
                // float distance = math.length(diff);

                SeparationForces[index] += diff * 1 / distanceSqrd;

                flockCenter += otherPos;
            }
            flockCenter /= neighbourhoodSize + 1;
            FlockCenters[index] = flockCenter;

            var containmentForce = float3.zero;
            if (selfPos.x > PlaygroundBounds.x)
            {
                containmentForce += -MathEx.float3_Right;
            }
            if (selfPos.x < -PlaygroundBounds.x)
            {
                containmentForce += MathEx.float3_Right;
            }
            if (selfPos.z > PlaygroundBounds.y)
            {
                containmentForce += -MathEx.float3_Forward;
            }
            if (selfPos.z < -PlaygroundBounds.y)
            {
                containmentForce += MathEx.float3_Forward;
            }
            ContainmentForces[index] = containmentForce;

            neighbours.Dispose();
        }
    }

    [BurstCompile]
    struct ChooseLeaderAndCalcCohesionForceJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> Positions;
        [ReadOnly] public NativeArray<quaternion> Rotations;
        [ReadOnly] public KDTree Tree;
        [ReadOnly] public float NeighbourhoodRange;
        [ReadOnly] public NativeArray<float> Leaderships;
        [ReadOnly] public NativeArray<float3> FlockCenters;

        [NativeSetThreadIndex] private int ThreadIndex;

        public NativeArray<int> ChoosenLeader;
        public NativeArray<float3> CohesionForces;
        public NativeArray<float3> AlignmentForces;

        public void Execute(int index)
        {
            var neighbours = new NativeArray<KDTree.Neighbour>(16, Allocator.Temp);
            var selfPos = Positions[index];
            int neighbourhoodSize = Tree.GetEntriesInRange(index, in selfPos, NeighbourhoodRange, ref neighbours, ThreadIndex);

            bool leaderFound = false;
            float highestLeadership = 0f;
            for (int i = 0; i < neighbourhoodSize; i++)
            {
                var neighbour = neighbours[i];
                if (neighbour.index == index) //This should not be needed but it seems queryingIndex is not being used properly from GetEntriesInRange()
                    continue;

                int otherIndex = neighbour.index;
                float3 otherPos = neighbour.position;
                float distanceSqrd = neighbour.distSq;

                float3 diff = selfPos - otherPos;
                float distance = math.length(diff);

                float leadership = Leaderships[otherIndex] / math.lerp(1f, 5f, distance / NeighbourhoodRange);
                if (leadership > highestLeadership)
                {
                    highestLeadership = leadership;
                    ChoosenLeader[index] = otherIndex;
                    leaderFound = true;
                }
            }

            CohesionForces[index] = FlockCenters[ChoosenLeader[index]] - selfPos;
            if (leaderFound)
                AlignmentForces[index] = math.rotate(Rotations[ChoosenLeader[index]], MathEx.float3_Forward);

            neighbours.Dispose();
        }
    }

    [BurstCompile]
    struct MoveJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<float> Leaderships;

        [ReadOnly] public NativeArray<float3> SeparationForces;
        [ReadOnly] public NativeArray<float3> CohesionForces;
        [ReadOnly] public NativeArray<float3> AlignmentForces;
        [ReadOnly] public NativeArray<float3> ContainmentForces;

        [ReadOnly] public float ForwardWeight;
        [ReadOnly] public float SeparationWeight;
        [ReadOnly] public float CohesionWeight;
        [ReadOnly] public float AlignmentWeight;
        [ReadOnly] public float ContainmentWeight;
        [ReadOnly] public float AgentSpeed;
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public float RotationCoefficient;

        public void Execute(int index, TransformAccess transform)
        {
            float reverseLeadership = (1f - Leaderships[index]);
            float3 forward = transform.localRotation * MathEx.float3_Forward;
            float3 desiredDirection = forward * ForwardWeight
                + SeparationForces[index] * SeparationWeight
                + CohesionForces[index] * CohesionWeight * reverseLeadership
                + AlignmentForces[index] * AlignmentWeight * reverseLeadership
                + ContainmentForces[index] * ContainmentWeight;

            float3 normalizedDirection = math.normalize(desiredDirection);

            float3 velocity = normalizedDirection * AgentSpeed;
            transform.localPosition = (float3)transform.localPosition + velocity * DeltaTime;

            float3 newForward = math.lerp(forward, normalizedDirection, RotationCoefficient);
            transform.localRotation = quaternion.LookRotation(newForward, MathEx.float3_Up);
        }
    }

}