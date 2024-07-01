using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GalaxyInfoReader : MonoBehaviour
{
    public string csvFilePath;

    private Dictionary<string, Dictionary<string, string>> galaxyData;

    [SerializeField] private List<GalaxyInfos> _galaxyInfosList = new List<GalaxyInfos>();
    public List<GalaxyInfos> GetGalaxyInfosList { get { return _galaxyInfosList; } }

    public GalaxyInfos GetGalaxyInfo(string galaxyName)
    {
        return _galaxyInfosList.Find(galaxy => galaxy.GalaxyUI.IndexOf(galaxyName, StringComparison.OrdinalIgnoreCase) != -1);
    }

    private void Awake()
    {
        ReadGalaxyData();
        Debug.Log("number of galaxies in CSV file : " + _galaxyInfosList.Count);
    }

    private void ReadGalaxyData()
    {
        galaxyData = new Dictionary<string, Dictionary<string, string>>();

        // Read the .csv file
        string[] lines = File.ReadAllLines(csvFilePath);

        // Get the keys from the first line
        string[] keys = lines[0].Split(',');

        // Parse each line of the .csv file (excluding the first line)
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            if (values.Length > 1)
            {
                string galaxyName = values[1];
                Dictionary<string, string> galaxyInfo = new Dictionary<string, string>();

                for (int j = 1; j < values.Length; j++)
                {
                    string key = keys[j].Trim();
                    string value = values[j].Trim();
                    galaxyInfo.Add(key, value);
                }

                galaxyData.Add(galaxyName, galaxyInfo);
                _galaxyInfosList.Add(SetGalaxiesInfos(galaxyName));
            }
        }
    }

    private GalaxyInfos SetGalaxiesInfos(string galaxyName)
    {
        GalaxyInfos info = null;

        if(galaxyData != null)
        {
            foreach (var kvp in galaxyData)
            {
                string keyWithoutSpaces = kvp.Key.Replace(" ", string.Empty).ToLower();
                string nameWithoutSpaces = galaxyName.Replace(" ", string.Empty).ToLower();

                if (keyWithoutSpaces.Contains(nameWithoutSpaces))
                {
                    Dictionary<string, string> galaxyInfo = kvp.Value;

                    Debug.Log("Galaxy Name: " + kvp.Key);
                    Debug.Log("UI Name: " + galaxyInfo["Name_UI"]);
                    Debug.Log("u-band: " + galaxyInfo["uband"]);
                    Debug.Log("g-band: " + galaxyInfo["gband"]);
                    Debug.Log("r-band: " + galaxyInfo["rband"]);
                    Debug.Log("i-band: " + galaxyInfo["iband"]);
                    Debug.Log("z-band: " + galaxyInfo["zband"]);
                    Debug.Log("Redshift: " + galaxyInfo["Redshift"]);
                    Debug.Log("Radial Velocity: " + galaxyInfo["radialVelocity_UI"]);
                    Debug.Log("Luminosity: " + galaxyInfo["luminosity_UI"]);
                    Debug.Log("Angular Size: " + galaxyInfo["angularSize_UI"]);
                    Debug.Log("Camera: " + galaxyInfo["Camera"]);

                    info = new GalaxyInfos()
                    {
                        GalaxyName = kvp.Key,
                        GalaxyUI = galaxyInfo["Name_UI"],
                        UBand = galaxyInfo["uband"],
                        GBand = galaxyInfo["gband"],
                        RBand = galaxyInfo["rband"],
                        IBand = galaxyInfo["iband"],
                        ZBand = galaxyInfo["zband"],
                        GalaxyRedshift = galaxyInfo["Redshift"],
                        GalaxyRadialVelocity = galaxyInfo["radialVelocity_UI"],
                        GalaxyLuminosity = galaxyInfo["luminosity_UI"],
                        GalaxyAngularSize = galaxyInfo["angularSize_UI"],
                        GalaxyCamera = galaxyInfo["Camera"],
                    };

                    break; // Exit the loop after finding the first partial match
                }
            }
        }        

        if (info == null)
        {
            Debug.Log("Galaxy not found in the data: " + galaxyName);

            info = new GalaxyInfos()
            {
                GalaxyName = "!GalaxyName",
                GalaxyUI = "!GalaxyUI",
                UBand = "!uband",
                GBand = "!gband",
                RBand = "!rband",
                IBand = "!iband",
                ZBand = "!zband",
                GalaxyRedshift = "!GalaxyRedshift",
                GalaxyRadialVelocity = "!GalaxyRadialVelocity",
                GalaxyLuminosity = "!GalaxyLuminosity",
                GalaxyAngularSize = "!GalaxyAngularSize",
                GalaxyCamera = "!GalaxyCamera",
            };
        }

        return info;
    }
}

public class GalaxyInfos
{
    private string _galaxyName;
    public string GalaxyName
    {
        get { return _galaxyName; }
        set { _galaxyName = "Galaxy Name: " + value; }
    }

    private string _galaxyUI;
    public string GalaxyUI
    {
        get { return _galaxyUI; }
        set { _galaxyUI = "Galaxy UI: " + value; }
    }

    private string _uBand;
    public string UBand
    {
        get { return _uBand; }
        set { _uBand = value; }
    }

    private string _gBand;
    public string GBand
    {
        get { return _gBand; }
        set { _gBand = value; }
    }

    private string _rBand;
    public string RBand
    {
        get { return _rBand; }
        set { _rBand = value; }
    }

    private string _iBand;
    public string IBand
    {
        get { return _iBand; }
        set { _iBand = value; }
    }

    private string _zBand;
    public string ZBand
    {
        get { return _zBand; }
        set { _zBand = value; }
    }

    private string _galaxyRedshift;
    public string GalaxyRedshift
    {
        get { return _galaxyRedshift; }
        set { _galaxyRedshift = "Galaxy Redshift: " + value; }
    }
    private string _galaxyRadialVelocity;
    public string GalaxyRadialVelocity
    {
        get { return _galaxyRadialVelocity; }
        set { _galaxyRadialVelocity = "Galaxy Radial Velocity: " + value; }
    }

    private string _galaxyLuminosity;
    public string GalaxyLuminosity
    {
        get { return _galaxyLuminosity; }
        set { _galaxyLuminosity = "Galaxy Luminosity: " + value; }
    }

    private string _galaxyAngularSize;
    public string GalaxyAngularSize
    {
        get { return _galaxyAngularSize; }
        set { _galaxyAngularSize = "Galaxy Angular Size: " + value; }
    }

    private string _galaxyCamera;
    public string GalaxyCamera
    {
        get { return _galaxyCamera; }
        set { _galaxyCamera = "Galaxy Camera: " + value; }
    }
}
