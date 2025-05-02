using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
[UpdateInGroup(typeof(SimulationSystemGroup))]
partial struct PlayerNameSystem : ISystem
{
    public string playerName;
    PlayerNameManager pNameManager;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
       pNameManager = PlayerNameManager.Instance;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("Check");
        foreach ((
             RefRW<GhostOwnerIsLocal> ghostOwnerLocal,
             RefRW<HealthComponent> healthComponent,
             Entity entity)
             in SystemAPI.Query<
                 RefRW<GhostOwnerIsLocal>,
                 RefRW<HealthComponent>>().WithEntityAccess().WithAll<Simulate>())
        {
            healthComponent.ValueRW.playerName = "Changed";
            Debug.Log("Player name updated to:"+ healthComponent.ValueRW.playerName);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
