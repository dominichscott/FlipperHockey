using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using FlipperHockey;

partial class ScoreBoardSystem : SystemBase
{
    private EntityQuery ghostQuery;
    private UIDocument uiDocument;
    private MultiColumnListView scoreboard;
    private GameObject uiObject;

    PlayerNameManager pNameManager;
    int netId = 999;
    protected override void OnCreate()
    {
        ghostQuery = GetEntityQuery(typeof(GhostInstance), typeof(HealthComponent));

        pNameManager = new PlayerNameManager();
        base.OnCreate();
    }

    private void Instance_OnPlayerNameChanged(string obj)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnUpdate()
    {

        //Find the netID 
        EntityQuery connectionQuery = GetEntityQuery(
            ComponentType.ReadOnly<NetworkStreamInGame>(),
            ComponentType.ReadOnly<NetworkId>()
        );
        if (connectionQuery.IsEmpty)
        {
           // Debug.Log("No Conn");
        }
        else if(netId > 10)
        {
            var connectionEntity = connectionQuery.GetSingletonEntity();
            netId = EntityManager.GetComponentData<NetworkId>(connectionEntity).Value;
            Debug.Log("Conn: " + netId);
        }
        else
        {
            //nothing we have a netid
        }


        if (uiObject == null)
        {
            // Debug.Log("No UIManager, searching");
            uiObject = GameObject.FindWithTag("UIScoreManager");

        }
        if (uiObject != null && uiDocument == null)
        {
            // Debug.Log("UIManager found, searching for UIdoc");
            uiDocument = uiObject.GetComponent<UIDocument>();

        }
        if (uiObject != null && uiDocument != null && scoreboard == null)
        {
            // Debug.Log("Found UIDoc, searching for HealthSlider!");
            scoreboard = uiDocument.rootVisualElement.Q<MultiColumnListView>("ScoreboardMultiColListView");

            scoreboard.columns.Clear();
            scoreboard.columns.Add(new Column()
            {
                title = "PlayerName",
                makeCell = MakeCellLabel,
                bindCell = BindNameToCell,
                stretchable = true,
            });
            scoreboard.columns.Add(new Column()
            {
                title = "Score",
                makeCell = MakeCellLabel,
                bindCell = BindScoreToCell,
                stretchable = true,
            });

            scoreboard.Rebuild();
        }
        else
        {
            //Debug.Log("Lets go!");
        }

        var scores = new List<PlayerScore>();
        int cnt = 0;
        Entities.ForEach((ref HealthComponent healthComp) =>
        //.WithAll<GhostInstance>().ForEach((ref HealthComponent health, ref GhostOwner ghostOwner, ref GhostOwnerIsLocal gol) =>
        {
          //  Debug.Log("NID: " + netId + "HPNID: "+healthComp.ownerNetworkID);
            if (netId == healthComp.ownerNetworkID)
            {
                healthComp.playerName = pNameManager.GetPlayerName();
                //Debug.Log("NID: " + netId + "HPNID: " + healthComp.ownerNetworkID + " hname: " + healthComp.playerName);
            }
           
            cnt++;
            PlayerScore tmp = new PlayerScore(""+ healthComp.playerName, (int)healthComp.score);
            scores.Add(tmp);
            

        }).WithoutBurst().Run();
        if (scores.Count > 0)
        {
            ApplyPersons(scores);
        }
    }
    public void ApplyPersons(List<PlayerScore> scores)
    {
        scoreboard.itemsSource = scores;
    }
    private Label MakeCellLabel() => new();

    private void BindNameToCell(VisualElement element, int index)
    {
        var label = (Label)element;
        var person = (PlayerScore)scoreboard.viewController.GetItemForIndex(index);
        label.text = person.playerName;
    }

    private void BindScoreToCell(VisualElement element, int index)
    {
        var label = (Label)element;
        var playerScore = (PlayerScore)scoreboard.viewController.GetItemForIndex(index);
        label.text = "" + playerScore.score;
    }
}
public class PlayerScore
{
    public string playerName;
    public int score;

    public PlayerScore(string pName, int score)
    {
        playerName = pName;
        this.score = score;
    }
}
