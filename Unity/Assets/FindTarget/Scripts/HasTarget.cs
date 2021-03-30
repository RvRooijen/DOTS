using Unity.Entities;
using Unity.Mathematics;

namespace FindTarget
{
    public struct HasTarget : IComponentData
    {
        public Entity TargetEntity;
        public float3 TargetPosition;
    }
}