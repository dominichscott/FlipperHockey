using FlipperHockey;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.UIElements;
using UnityEngine;

partial class HealthBarUISystem : SystemBase
{
    private EntityQuery ghostQuery;
    private UIDocument uiDocument;
    private ProgressBar healthSlider;
    private GameObject uiObject;
    protected override void OnCreate()
    {
        ghostQuery = GetEntityQuery(typeof(GhostInstance), typeof(HealthComponent));

        // Find UIDocument in scene
        uiObject = GameObject.FindWithTag("UIManager");
        if (uiObject != null)
        {
            Debug.Log("Found UIManager");
            uiDocument = uiObject.GetComponent<UIDocument>();
            healthSlider = uiDocument.rootVisualElement.Q<ProgressBar>("HealthBar");
        }
        else
        {
            Debug.Log("No UIManager");
        }
    }

    protected override void OnUpdate()
    {
       
        if (uiObject == null)
        {
            //Debug.Log("No UIManager, searching");
            uiObject = GameObject.FindWithTag("UIManager");
           
        }
        else if(uiObject != null && uiDocument == null)
        {
            //Debug.Log("UIManager found, searching for UIdoc");
            uiDocument = uiObject.GetComponent<UIDocument>();
            
        }
        else if(uiObject != null && uiDocument != null && healthSlider == null )
        {
            //Debug.Log("Found UIDoc, searching for HealthSlider!");
            healthSlider = uiDocument.rootVisualElement.Q<ProgressBar>("HealthBar");
            
            return;
        }
        else
        {
            //Debug.Log("Lets go!");
        }


        Entities.WithAll<GhostOwnerIsLocal>().ForEach((ref HealthComponent health, ref GhostOwner ghostOwner) =>
            //.WithAll<GhostInstance>().ForEach((ref HealthComponent health, ref GhostOwner ghostOwner, ref GhostOwnerIsLocal gol) =>
        {

            // Debug.Log("Ghost inst me: ");
            if (health.ownerNetworkID == ghostOwner.NetworkId && !World.IsServer())
            {
                healthSlider.value = health.CurrentHealth;
            }
    
        }).WithoutBurst().Run();
    }
}