using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct HasTarget : IComponentData
{
    public Entity TargetEntity;
    public float3 TargetPosition;
}