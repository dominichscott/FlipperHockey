using FlipperHockey;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
partial struct PickupManager : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PickupSpawner>();
        //state.RequireForUpdate<PickupSpawnerPowerUp>();
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var speedPrefab = SystemAPI.GetSingleton<PickupSpawner>().PickupObjAsEnt;
        //var powerPrefab = SystemAPI.GetSingleton<PickupSpawnerPowerUp>().PickupObjAsEnt;
        
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();

        foreach ((
            var pickupSpawner,
            var localTransform)
            in SystemAPI.Query<
            RefRW<PickupSpawner>,
            RefRO<LocalTransform>>().WithAll<Simulate>())
        {

            if (networkTime.IsFirstTimeFullyPredictingTick)
            {
                if (pickupSpawner.ValueRO.hasObject != 1)
                {
                    
                    Debug.LogWarning("Spawn Speed Power Up Obj");
                    Entity hpBoxEntity = ecb.Instantiate(speedPrefab);


                    ecb.SetComponent(hpBoxEntity, LocalTransform.FromPositionRotation(localTransform.ValueRO.Position, localTransform.ValueRO.Rotation));

                    pickupSpawner.ValueRW.hasObject = 1;
                    pickupSpawner.ValueRW.timer = 10f;
                }
            }
        }
        /*
        foreach ((
                     var pickupSpawnerPowerUp,
                     var localTransform)
                 in SystemAPI.Query<
                     RefRW<PickupSpawnerPowerUp>,
                     RefRO<LocalTransform>>().WithAll<Simulate>())
        {

            if (networkTime.IsFirstTimeFullyPredictingTick)
            {
                if (pickupSpawnerPowerUp.ValueRO.hasObject != 1)
                {
                    
                    Debug.LogWarning("Spawn Power Up Obj");
                    Entity hpBoxEntity = ecb.Instantiate(powerPrefab);


                    ecb.SetComponent(hpBoxEntity, LocalTransform.FromPositionRotation(localTransform.ValueRO.Position, localTransform.ValueRO.Rotation));

                    pickupSpawnerPowerUp.ValueRW.hasObject = 1;
                    pickupSpawnerPowerUp.ValueRW.timer = 10f;
                }
            }
        }
        */
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
