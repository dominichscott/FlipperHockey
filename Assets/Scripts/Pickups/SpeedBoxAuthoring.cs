using Unity.Entities;
using UnityEngine;

namespace FlipperHockey
{
    public struct SpeedBox : IComponentData
    {
        public float timer;
        public byte hasbeenPickedUp;
        public bool touchable;
        internal bool destroy;
    }

    [DisallowMultipleComponent]
    public class SpeedBoxAuthoring : MonoBehaviour
    {


        class SpawnerBaker : Baker<SpeedBoxAuthoring>
        {
            public override void Bake(SpeedBoxAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<SpeedBox>(entity, new SpeedBox
                {
                    timer = 5f,
                    hasbeenPickedUp = 0,
                    touchable = false
                });
            }
        }
    }
}
