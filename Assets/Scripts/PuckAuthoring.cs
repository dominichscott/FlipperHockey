using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

namespace FlipperHockey
{
    
    public struct Puck : IComponentData
    {
        public float3 Direction;
        public bool ShouldReflect;
        public float3 SurfaceNormal;
        public float Speed;
        public float MaxSpeed;
        public bool destroy;
    }

    
    [DisallowMultipleComponent]
    public class PuckAuthoring : MonoBehaviour
    {
        class Baker : Baker<PuckAuthoring>
        {
            public override void Bake(PuckAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Puck>(entity, new Puck
                {
                    Direction = new float3(0, 0, 0),
                    ShouldReflect = false,
                    SurfaceNormal = new float3(0, 0, 0),
                    Speed = 5f,
                    MaxSpeed = 10f,
                    destroy = false,
                });
            }
        }
    }
}