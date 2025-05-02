using System.Diagnostics;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.LightTransport;
using UnityEngine.Rendering.Universal;

namespace FlipperHockey
{
    /// <summary>
    /// Updates the <see cref="MainGameObjectCamera"/> postion to match the current player <see cref="MainCamera"/> component position if it exists.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class MainCameraSystem : SystemBase
    {
        protected override void OnCreate()
        {
            if (MainGameObjectCamera.Instance == null)
            {
                MainGameObjectCamera.Instance = Camera.main;
                RequireForUpdate<MainCamera>();
            }

        }

        protected override void OnUpdate()
        {


          
        }
    }
}
