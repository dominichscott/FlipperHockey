using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Windows;
namespace FlipperHockey
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial struct PickUpController : ISystem
    {
        public float yDegrees;
        public float rSpeed;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            yDegrees = 0f;
            rSpeed = 1f;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            //cleanup SpeedBox
            foreach ((
                RefRW<LocalTransform> localTransform,
                RefRW<SpeedBox> speedBox,
                Entity entity)
                in SystemAPI.Query<
                    RefRW<LocalTransform>,
                    RefRW<SpeedBox>>().WithEntityAccess().WithAll<Simulate>())
            {
                //rotate object             
                yDegrees += rSpeed;
                Quaternion rRot = Quaternion.Euler(0, yDegrees, 0);
                localTransform.ValueRW.Rotation = rRot;

                if (state.World.IsServer())
                {
                    if (speedBox.ValueRO.destroy)
                    {
                        Debug.Log("Destroying SpeedPickup");
                        ecb.DestroyEntity(entity);
                    }
                }
            }
            //cleanup PowerBox
            foreach ((
                RefRW<LocalTransform> localTransform,
                RefRW<PowerBox> powerBox,
                Entity entity)
                in SystemAPI.Query<
                    RefRW<LocalTransform>,
                    RefRW<PowerBox>>().WithEntityAccess().WithAll<Simulate>())
            {
                //rotate object
                yDegrees -= rSpeed;
                Quaternion rRot = Quaternion.Euler(0, yDegrees, 0);
                localTransform.ValueRW.Rotation = rRot;
                if (state.World.IsServer())
                {
                    if (powerBox.ValueRO.destroy)
                    {
                        Debug.Log("Destroying Power");
                        ecb.DestroyEntity(entity);
                    }
                }
            }

            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }

}