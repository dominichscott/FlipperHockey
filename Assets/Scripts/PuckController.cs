using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace FlipperHockey
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    partial struct PuckController : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {}

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach ((
                        RefRW<LocalTransform> localTransform,
                        RefRW<Puck> puck,
                        Entity entity)
                    in SystemAPI.Query<
                        RefRW<LocalTransform>,
                        RefRW<Puck>>().WithEntityAccess().WithAll<Simulate>())
            {
                
                if (state.World.IsServer())
                {
                    // Reflect if collision happened
                    if (puck.ValueRO.ShouldReflect)
                    {
                        puck.ValueRW.Direction = math.reflect(puck.ValueRO.Direction, puck.ValueRO.SurfaceNormal);
                        puck.ValueRW.ShouldReflect = false; // reset after bounce
                    }

                    //Debug.Log("Moving Puck " + puck.ValueRO.Direction + ", " + puck.ValueRO.Speed + ", " + SystemAPI.Time.DeltaTime);
                    // Move puck in its current direction
                    localTransform.ValueRW.Position += puck.ValueRO.Speed * puck.ValueRO.Direction * SystemAPI.Time.DeltaTime;
                }
            }

            ecb.Playback(state.EntityManager);
        }
    }
}
