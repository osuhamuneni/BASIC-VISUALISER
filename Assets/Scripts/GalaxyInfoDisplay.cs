using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyInfoDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshPro galaxyName;
    [SerializeField] private TextMeshPro galaxyUI;
    [SerializeField] private TextMeshPro galaxyEquatorialCoordinates;
    [SerializeField] private TextMeshPro galaxyRadialVelocity;
    [SerializeField] private TextMeshPro galaxyLuminosity;
    [SerializeField] private TextMeshPro galaxyAngularSize;
    [SerializeField] private TextMeshPro galaxyCamera;

    [SerializeField] private Text Text_galaxyName;
    [SerializeField] private Text Text_galaxyUI;
    [SerializeField] private Text Text_galaxyEquatorialCoordinates;
    [SerializeField] private Text Text_galaxyRadialVelocity;
    [SerializeField] private Text Text_galaxyLuminosity;
    [SerializeField] private Text Text_galaxyAngularSize;
    [SerializeField] private Text Text_galaxyCamera;

    [SerializeField] private GameObject galaxyInfoPanel;
    [SerializeField] private GameObject galaxyInfoButton;

    private bool textValueHasBeenset = false;
    private GalaxyInfos _galaxyInfos;
    private Galaxy _galaxy;

    private Canvas _canvas;

    //private void OnEnable()
    //{
    //    if(_canvas == null)
    //    {
    //        _canvas = GetComponent<Canvas>();
    //        _canvas.worldCamera = Camera.main;
    //    }
    //    SetGalaxyInfos();
    //}

    public void InitGalaxyInfo(GalaxyInfos galaxyInfos)
    {
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
            _canvas.worldCamera = Camera.main;
        }
        _galaxyInfos = galaxyInfos;
    }

    private void SetGalaxyInfos()
    {
        if (galaxyName != null)
        {
            galaxyName.text = _galaxyInfos.GalaxyName;
            galaxyUI.text = _galaxyInfos.GalaxyUI;
            galaxyEquatorialCoordinates.text = _galaxyInfos.GalaxyRedshift;
            galaxyRadialVelocity.text = _galaxyInfos.GalaxyRadialVelocity;
            galaxyLuminosity.text = _galaxyInfos.GalaxyLuminosity;
            galaxyAngularSize.text = _galaxyInfos.GalaxyAngularSize;
            galaxyCamera.text = _galaxyInfos.GalaxyCamera;
        }

        if(Text_galaxyName != null)
        {
            Text_galaxyName.text = _galaxyInfos.GalaxyName;
            Text_galaxyUI.text = _galaxyInfos.GalaxyUI;
            Text_galaxyEquatorialCoordinates.text = _galaxyInfos.GalaxyRedshift;
            Text_galaxyRadialVelocity.text = _galaxyInfos.GalaxyRadialVelocity;
            Text_galaxyLuminosity.text = _galaxyInfos.GalaxyLuminosity;
            Text_galaxyAngularSize.text = _galaxyInfos.GalaxyAngularSize;
            Text_galaxyCamera.text = _galaxyInfos.GalaxyCamera;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.I))
        {
            DisplayInfos(true);
        }

        if (Input.GetKey(KeyCode.X))
        {
            DisplayInfos(false);
        }

        if(Input.GetKey(KeyCode.F))
        {
            DisplayUI(false);
            DisplayInfos(false);
        }
    }

    public void DisplayUI(bool show)
    {
        gameObject.SetActive(show);
    }

    public void DisplayInfos(bool show)
    {
        galaxyInfoPanel.SetActive(show);
        galaxyInfoButton.SetActive(!show);

        if (!textValueHasBeenset && show)
            SetGalaxyInfos();
    }
}
