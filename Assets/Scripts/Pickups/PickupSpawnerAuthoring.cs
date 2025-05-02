using Unity.Entities;
using UnityEngine;

namespace FlipperHockey
{
    public struct PickupSpawner : IComponentData
    {
        public Entity PickupObjAsEnt;
        public byte hasObject;
        public float timer;
    }

    [DisallowMultipleComponent]
    public class PickupSpawnerAuthoring : MonoBehaviour
    {
        public GameObject PickupObj;

        class SpawnerBaker : Baker<PickupSpawnerAuthoring>
        {
            public override void Bake(PickupSpawnerAuthoring authoring)
            {
                PickupSpawner component = default(PickupSpawner);
                component.PickupObjAsEnt = GetEntity(authoring.PickupObj, TransformUsageFlags.Dynamic);
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, component);
            }
        }
    }
}
