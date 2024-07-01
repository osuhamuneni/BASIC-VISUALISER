using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class GalaxyManager : MonoBehaviour
{
    [HideInInspector] public bool autoAddGalaxyPerPerformancesValue = false;
    [HideInInspector] public float memoryThreshold = 500f;
    [HideInInspector] public float cpuThreshold = 80f;
    [HideInInspector] public float fpsThreshold = 50f;
    public GameObject[] fObjectArray;
    public GameObject uiPrefab;
    public Mesh mesh;
    public Material material;

    [SerializeField] private Toggle _cvsToggle;

    private float lastUpdateTime;
    private int fITSObjectIndex = 0;
    private FITSObject fITSObject;
    private FITSObject previousFITSObject;
    private Galaxy _galaxy;
    private GalaxyInfoReader _galaxyInfoReader;

    private List<FITSObject> fitsObjectsPrefabList = new List<FITSObject>();
    private List<FITSObject> fitsObjectsCSVList = new List<FITSObject>();

    private bool _isRunning = true;
    public bool IsRunning
    {
        get { return _isRunning; }
        set { _isRunning = value; }
    }

    private bool _loadFromCSV = false;
    public bool LoadFromCSV
    {
        get
        {
            return _loadFromCSV;
        }
        set
        {
            _loadFromCSV = value;
        }
    }

    private bool _isGalaxySelected = false;
    public bool IsGalaxySelected
    {
        get
        {
            return _isGalaxySelected;
        }
        set
        {
            _isGalaxySelected = value;
            _galaxy.ShowHideGalaxyInfo(value);
            if (value == false)
                _galaxy = null;
        }
    }
    public List<FITSObject> GetFITSObjectsList { get { return fitsObjectsPrefabList; } }
    public static GalaxyManager Instance { get; private set; }

    private void Awake()
    {
        if (GalaxyManager.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        GalaxyManager.Instance = this;

        _galaxyInfoReader = FindObjectOfType<GalaxyInfoReader>();
    }

    private void Start()
    {
        _cvsToggle.onValueChanged.AddListener(OnToggleValueChanged);
        if (LoadFromCSV)
        {
            CreateGalaxy();
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        LoadFromCSV = isOn;
    }

    private void Update()
    {
        if (LoadFromCSV)
        {
            if(fitsObjectsCSVList.Count == 0)
                CreateGalaxy();
            return;
        }

        if (!_isRunning || fObjectArray.Length == 0)
            return;

        bool isLoaded = previousFITSObject != null ? previousFITSObject.loaded : true;

        if (isLoaded && fObjectArray.Length > fITSObjectIndex)
        {
            if (autoAddGalaxyPerPerformancesValue && ShouldInstantiateGalaxy())
            {
                InstantiateGalaxy();
            }
            else if (!autoAddGalaxyPerPerformancesValue)
            {
                InstantiateGalaxy();
            }
        }
    }

    private bool ShouldInstantiateGalaxy()
    {
        if (Time.time - lastUpdateTime < 1f)
        {
            return false;
        }

        float currentMemory = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        float maxMemory = Profiler.GetMonoHeapSizeLong() / (1024f * 1024f);
        float cpuUsage = GetCPUUsage();
        float fps = 1f / Time.deltaTime;

        bool isMemoryPassed = currentMemory < maxMemory - memoryThreshold;
        bool isCPUPassed = cpuUsage < cpuThreshold;
        bool isFPSPassed = fps < fpsThreshold;

        return isMemoryPassed && isCPUPassed && isFPSPassed;
    }

    private float GetCPUUsage()
    {
        PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        float rawCpuUsage = cpuCounter.NextValue();
        System.Threading.Thread.Sleep(1000);
        rawCpuUsage = cpuCounter.NextValue();
        float cpuUsagePercentage = rawCpuUsage / System.Environment.ProcessorCount;
        UnityEngine.Debug.Log("CPU Usage: " + cpuUsagePercentage.ToString("0.00") + "%");
        return cpuUsagePercentage;
    }

    private void InstantiateGalaxy()
    {
        IsRunning = !(fITSObjectIndex == fObjectArray.Length - 1);
        if (!IsRunning)
            return;

        GameObject go = Instantiate(fObjectArray[fITSObjectIndex], transform.position, transform.rotation);
        go.name = fObjectArray[fITSObjectIndex].name;
        fITSObject = go.GetComponent<FITSObject>();
        Galaxy galaxy = go.AddComponent<Galaxy>();
        GalaxyInfos gInfos = _galaxyInfoReader.GetGalaxyInfo(go.name);
        GalaxyInfoDisplay galaxyInfoDisplay = AddUIObject(go);
        galaxyInfoDisplay.InitGalaxyInfo(gInfos);
        galaxy.Init(this, galaxyInfoDisplay, gInfos);

        fitsObjectsPrefabList.Add(fITSObject);
        previousFITSObject = fITSObject;

        fITSObjectIndex++;
        lastUpdateTime = Time.time;
    }

    private void CreateGalaxy()
    {
        foreach (var galaxyInfos in _galaxyInfoReader.GetGalaxyInfosList)
        {
            string galaxyName = galaxyInfos.GalaxyName.Replace("Galaxy Name: ", "");

            GameObject galaxyGo = new GameObject(galaxyName);
            galaxyGo.AddComponent<Transform>();
            galaxyGo.AddComponent<MeshFilter>().sharedMesh = mesh;
            galaxyGo.AddComponent<MeshRenderer>().material = material;
            galaxyGo.AddComponent<SphereCollider>();
            galaxyGo.AddComponent<Galaxy>();
            galaxyGo.AddComponent<FITSObject>();
            galaxyGo.AddComponent<GalaxiesFaceCamera>();

            var fitsObject = galaxyGo.GetComponent<FITSObject>();
            fitsObject.scale = 20;
            fitsObject.ImageData = new FitsReader.FITSImage[3];

            for (int i = 0; i < 3; i++)
            {
                fitsObject.ImageData[i] = new FitsReader.FITSImage
                {
                    Path = GetColorBandPath(galaxyInfos, i),
                    isEnabled = true,
                    colorBand = GetColorBand(i),
                    Settings = new FitsReader.FITSReadSettings
                    {
                        SourceMode = FitsReader.FITSSourceMode.Web,
                        RGBMultiplier = GetRGBMultiplier(i),
                        LowBoost = 10,
                        BoostMode = FitsReader.BoostMode.Difference
                    }
                };
            }

            GalaxyInfos gInfos = _galaxyInfoReader.GetGalaxyInfo(galaxyGo.name);
            GalaxyInfoDisplay galaxyInfoDisplay = AddUIObject(galaxyGo);
            galaxyInfoDisplay.InitGalaxyInfo(gInfos);
            Galaxy galaxy = galaxyGo.GetComponent<Galaxy>();
            galaxy.Init(this, galaxyInfoDisplay, gInfos);

            fitsObjectsCSVList.Add(fitsObject);
        }
    }

    private string GetColorBandPath(GalaxyInfos galaxyInfos, int index)
    {
        switch (index)
        {
            case 0:
                return galaxyInfos.RBand;
            case 1:
                return galaxyInfos.GBand;
            case 2:
                return galaxyInfos.IBand;
            default:
                return string.Empty;
        }
    }

    private static ColorBand GetColorBand(int index)
    {
        switch (index)
        {
            case 0: return ColorBand.Red;
            case 1: return ColorBand.Green;
            case 2: return ColorBand.Blue;
            default: return ColorBand.UV;
        }
    }

    private static Vector3 GetRGBMultiplier(int index)
    {
        switch (index)
        {
            case 0: return new Vector3(1, 0, 0);
            case 1: return new Vector3(0, 1, 0);
            case 2: return new Vector3(0, 0, 1);
            default: return Vector3.one;
        }
    }

    private GalaxyInfoDisplay AddUIObject(GameObject go)
    {
        GameObject prefab = Instantiate(uiPrefab, go.transform.position, Quaternion.identity, go.transform);
        return prefab.GetComponent<GalaxyInfoDisplay>();
    }

    public void SelectGalaxy(Galaxy galaxy)
    {
        if (_galaxy != null)
            IsGalaxySelected = false;

        if (galaxy != null)
        {
            _galaxy = galaxy;
            IsGalaxySelected = true;
        }
    }

    public FITSObject GetSelectectGalaxy { get { return _galaxy.gameObject.GetComponent<FITSObject>(); } }
}
