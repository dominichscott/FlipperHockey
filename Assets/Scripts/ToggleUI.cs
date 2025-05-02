using UnityEngine;

public class ToggleUI : MonoBehaviour
{
    public GameObject networkUIPanel;
    private bool toggle = true;
    
    public void ToggleUIButtonHandler()
    {
        toggle = !toggle;
        networkUIPanel.SetActive(toggle);
    }
}
