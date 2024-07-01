using UnityEngine;

public class Galaxy : MonoBehaviour
{
    private GalaxyManager _galaxyManager;
    private GalaxyInfoDisplay _galaxyInfoDisplay;
    private GalaxyInfos _galaxyInfos;

    public GalaxyInfos GetGalaxyInfos
    { 
        get
        {
            if(_galaxyInfos == null)
                _galaxyInfos = FindObjectOfType<GalaxyInfoReader>().GetGalaxyInfo(gameObject.name);
            return _galaxyInfos; 
        } 
    }

    public void Init(GalaxyManager galaxyManager, GalaxyInfoDisplay galaxyInfoDisplay, GalaxyInfos galaxyInfos)
    {
        _galaxyManager = galaxyManager;
        _galaxyInfoDisplay = galaxyInfoDisplay;
        _galaxyInfoDisplay.gameObject.SetActive(false);
        _galaxyInfos = galaxyInfos;
    }

    private void OnMouseDown()
    {
        _galaxyManager.SelectGalaxy(this);
    }

    public void ShowHideGalaxyInfo(bool show)
    {
        _galaxyInfoDisplay.DisplayUI(show);
    }
}
