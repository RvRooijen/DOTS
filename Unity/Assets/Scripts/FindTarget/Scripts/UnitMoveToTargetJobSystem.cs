using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class UnitMoveToTargetJobSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer.Concurrent concurrentCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        float dt = Time.DeltaTime;
        
        JobHandle jobHandle = Entities
            .WithAll<Unit>()
            .ForEach((int entityInQueryIndex, Entity unitEntity, ref HasTarget hasTarget, ref Translation translation) =>
            {
                float3 targetDirection = math.normalize(hasTarget.TargetPosition - translation.Value);
                float movementSpeed = 4;
                translation.Value += targetDirection * movementSpeed * dt;

                if (math.distance(translation.Value, hasTarget.TargetPosition) < .2f)
                {
                    concurrentCommandBuffer.DestroyEntity(entityInQueryIndex, hasTarget.TargetEntity);
                    concurrentCommandBuffer.RemoveComponent<HasTarget>(entityInQueryIndex, unitEntity);
                }
            }).Schedule(inputDeps);
        _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}