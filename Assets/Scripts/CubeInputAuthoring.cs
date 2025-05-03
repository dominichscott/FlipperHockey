using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using FlipperHockey;
#endif

namespace FlipperHockey
{
    /// <summary>
    /// Input for the cube to move in the example setup.
    /// </summary>
    public struct CubeInput : IInputComponentData
    {
        /// <summary>
        /// Horizontal movement (X axis).
        /// </summary>
        public int Horizontal;
        
        /// <summary>
        /// Vertical movement (Z axis).
        /// </summary>
        public int Vertical;

        public int Rotate;

        public InputEvent shoot;

        public InputEvent spawnPuck;
    }

    /// <summary>
    /// The authoring component for the CubeInput.
    /// </summary>
    [DisallowMultipleComponent]
    public class CubeInputAuthoring : MonoBehaviour
    {
        class Baking : Baker<CubeInputAuthoring>
        {
            public override void Bake(CubeInputAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<CubeInput>(entity);
            }
        }
    }

    /// <summary>
    /// System in charge of setting the cube input data based on the pressed keys.
    /// Input: whether keys are pressed, <see cref="CubeInput"/> component data.
    /// Output: modified <see cref="CubeInput"/> component data for which the local client is the owner.
    /// </summary>
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial struct SampleCubeInput : ISystem
    {
        /// <summary>
        /// Sets the cube input data based on the pressed keys.
        /// </summary>
        /// <param name="state">Raw entity system state, unused here.</param>
        public void OnUpdate(ref SystemState state)
        {
#if ENABLE_INPUT_SYSTEM
            var left = Keyboard.current.aKey.isPressed;
            var right = Keyboard.current.dKey.isPressed;
            var down = Keyboard.current.sKey.isPressed;
            var up = Keyboard.current.wKey.isPressed;
            var rLeft = Keyboard.current.qKey.isPressed;
            var rRight = Keyboard.current.eKey.isPressed;
            var spawnPuck = Keyboard.current.gKey.isPressed;

            var shootKey = Input.GetKeyDown(KeyCode.Mouse0);
#else
            var left = UnityEngine.Input.GetKey(KeyCode.A);
            var right = UnityEngine.Input.GetKey(KeyCode.D);
            var down = UnityEngine.Input.GetKey(KeyCode.S);
            var up = UnityEngine.Input.GetKey(KeyCode.W);
            var rLeft = UnityEngine.Input.GetKey(KeyCode.Q);
            var rRight = UnityEngine.Input.GetKey(KeyCode.E);
            
            var shootKey = UnityEngine.Input.GetKeyDown(KeyCode.Space);
#endif

            foreach (var playerInput in SystemAPI.Query<RefRW<CubeInput>>().WithAll<GhostOwnerIsLocal>())
            {
                playerInput.ValueRW = default;
                if (left)
                    playerInput.ValueRW.Horizontal -= 1;
                if (right)
                    playerInput.ValueRW.Horizontal += 1;
                if (down)
                    playerInput.ValueRW.Vertical -= 1;
                if (up)
                    playerInput.ValueRW.Vertical += 1;
                if (rLeft)
                    playerInput.ValueRW.Rotate -= 1;
                if (rRight)
                    playerInput.ValueRW.Rotate += 1;

                if (shootKey)
                {
                    playerInput.ValueRW.shoot.Set();
                }
                else
                {
                    playerInput.ValueRW.shoot = default;
                }

                if (spawnPuck)
                {
                    playerInput.ValueRW.spawnPuck.Set();
                }
                else
                {
                    playerInput.ValueRW.spawnPuck = default;
                }
            }
        }
    }
    
    /// <summary>
    /// System in charge of moving the cube based on the input for entities with the Simulate flag.
    /// Input: <see cref="CubeInput"/> component data, <see cref="LocalTransform"/> component data.
    /// Output: modified <see cref="LocalTransform"/> component data.
    /// </summary>
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [BurstCompile]
    public partial struct CubeMovementSystem : ISystem
    {
        /// <summary>
        /// Modifies the local transforms of the cubes based on the input.
        /// </summary>
        /// <param name="state">Raw entity system state, unused here.</param>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var speed = SystemAPI.Time.DeltaTime * 4;
            var rSpeed = SystemAPI.Time.DeltaTime * 150;
            var backCamOffset = 0;
            var rightCamOffset = 0;
            var upCamOffset = 8f;
            
            foreach (
                var (input, trans) in
                SystemAPI.Query<RefRO<CubeInput>, RefRW<LocalTransform>>()
                         .WithAll<Simulate>()
            )
            {
                //var moveInput = new float2(input.ValueRO.Horizontal, input.ValueRO.Vertical);
                //moveInput = math.normalizesafe(moveInput) * speed;
                //trans.ValueRW.Position += new float3(moveInput.x, 0, moveInput.y);

                var rotateInput = input.ValueRO.Rotate * rSpeed;
                trans.ValueRW.Rotation *= Quaternion.AngleAxis(rotateInput, Vector3.up);
                
                var forwardDir = math.mul(trans.ValueRW.Rotation, Vector3.forward);
                var rightDir = math.mul(trans.ValueRW.Rotation, Vector3.right);

                Vector3 moveVector = (input.ValueRO.Vertical * forwardDir * speed) +
                                     (input.ValueRO.Horizontal * rightDir * speed);
                trans.ValueRW.Position += new float3(moveVector);
            }

            foreach (var trans in 
                     SystemAPI.Query<RefRW<LocalTransform>>()
                         .WithAll<GhostOwner, GhostOwnerIsLocal>()
                         .WithAll<Simulate>())
            {
                if (!state.World.IsServer())
                {
                    // Entity mainEntityCameraEntity = SystemAPI.GetSingletonEntity<MainCamera>();
                    // MainCamera mainCamera = SystemAPI.GetSingleton<MainCamera>();
    
                    var backDir = math.mul(trans.ValueRO.Rotation, Vector3.back);
                    var rightDir = math.mul(trans.ValueRO.Rotation, Vector3.right);
                    var upDir = math.mul(trans.ValueRO.Rotation, Vector3.up);
    
                    float3 camPos = trans.ValueRO.Position +
                                    backDir * backCamOffset +
                                    rightDir * rightCamOffset +
                                    upDir * upCamOffset;
                    
                    
                    quaternion camRot = trans.ValueRO.Rotation * Quaternion.Euler(90f, 0f, 0f);
    
                    MainGameObjectCamera.Instance.transform.SetPositionAndRotation(camPos, camRot);
                    MainGameObjectCamera.Instance.fieldOfView = 75;
                }
                
            }
            //check if dead and respawn
            foreach ((RefRW<HealthComponent> playerHealth,
                         RefRW<LocalTransform> playerPosition)
                     in
                     SystemAPI.Query<
                             RefRW<HealthComponent>,
                             RefRW<LocalTransform>>()
                         .WithAll<HealthComponent>()
                         .WithAll<Simulate>())
            {

                if (playerHealth.ValueRO.CurrentHealth <= 0)
                {
                    List<LocalTransform> localTrans = new List<LocalTransform>();
                    foreach (var spawnTrans in
                             SystemAPI.Query<RefRW<LocalTransform>>()
                                 .WithAll<SpawnPoint>()
                                 .WithAll<Simulate>())
                    {
                        Debug.Log("SPpoint found " + spawnTrans.ValueRW.Position.x);
                        localTrans.Add(spawnTrans.ValueRW);
                    }
                    //adjust y offset
                    LocalTransform tempTransform = localTrans[UnityEngine.Random.Range(0, localTrans.Count - 1)];
                    tempTransform.Position.y = 1.43f;


                    playerPosition.ValueRW.Position = tempTransform.Position;
                    playerHealth.ValueRW.CurrentHealth = playerHealth.ValueRO.MaxHealth;
                }

            }
        }
    }
}
