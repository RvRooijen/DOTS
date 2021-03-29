using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[DisableAutoCreation]
public class FindTargetJobSystem : JobComponentSystem
{
    private struct EntityWithPosition
    {
        public Entity Entity;
        public float3 Position;
    }
    
    private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer.Concurrent concurrentCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

        EntityQuery targetQuery = GetEntityQuery(typeof(Target), ComponentType.Exclude<BeingUsed>(), ComponentType.ReadOnly<Translation>());
        NativeArray<Entity> targetEntityArray = targetQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> targetTranslationArray = targetQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

        NativeArray<EntityWithPosition> targetArray = new NativeArray<EntityWithPosition>(targetEntityArray.Length, Allocator.TempJob);
        
        for (int i = 0; i < targetEntityArray.Length; i++)
        {
            targetArray[i] = new EntityWithPosition
            {
                Entity = targetEntityArray[i],
                Position = targetTranslationArray[i].Value
            };
        }
        
        JobHandle jobHandle = Entities
            .WithAll<Unit>()
            .WithNone<HasTarget>()
            .ForEach((int entityInQueryIndex, Entity entity, ref Translation unitTranslation) => {
            
                Entity closestTargetEntity = Entity.Null;
                float3 unitPosition = unitTranslation.Value;
                float3 closestTargetPosition = float3.zero;

                for (var i = 0; i < targetArray.Length; i++)
                {
                    EntityWithPosition entityWithPosition = targetArray[i];
                    
                    if (closestTargetEntity == Entity.Null)
                    {
                        closestTargetEntity = entityWithPosition.Entity;
                        closestTargetPosition = entityWithPosition.Position;
                    }
                    else
                    {
                        if (math.distance(unitPosition, entityWithPosition.Position) <
                            math.distance(unitPosition, closestTargetPosition))
                        {
                            closestTargetEntity = entityWithPosition.Entity;
                            closestTargetPosition = entityWithPosition.Position;
                        }
                    }
                }

                if (closestTargetEntity != Entity.Null)
                {
                    concurrentCommandBuffer.AddComponent(entityInQueryIndex, entity, new HasTarget
                    {
                        TargetEntity = closestTargetEntity,
                        TargetPosition = closestTargetPosition
                    });
                    
                    concurrentCommandBuffer.AddComponent<BeingUsed>(entityInQueryIndex, closestTargetEntity);
                }
                
            }).Schedule(inputDeps);
        _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        targetEntityArray.Dispose();
        targetTranslationArray.Dispose();
        
        return jobHandle;
    }
}