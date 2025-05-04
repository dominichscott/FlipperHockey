using Unity.Entities;
using FlipperHockey;
using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

[GhostComponent]
public struct HealthComponent : IComponentData
{
    [GhostField] public float CurrentHealth;
    [GhostField] public float MaxHealth;
    [GhostField] public float ownerNetworkID;
    [GhostField] public float score;
    [GhostField] public FixedString64Bytes playerName;
}

[DisallowMultipleComponent]
public class HealthComponentAuthoring : MonoBehaviour
{
    public string defaultPlayerName = "Unkown";
    class Baker : Baker<HealthComponentAuthoring>
    {
        public override void Bake(HealthComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var fixedString = new FixedString64Bytes();
            fixedString.CopyFrom(authoring.defaultPlayerName);
            AddComponent<HealthComponent>(entity, new HealthComponent
            {
                CurrentHealth = 100f,
                MaxHealth = 100f,
                ownerNetworkID = -1f,
                score = 0,
                playerName = fixedString
            });


        }

    }


}