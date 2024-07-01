using System;
using System.Collections.Generic;
using UnityEngine;
using FitsReader;

public class d3_Controls : MonoBehaviour
{
    private FITSObject[] Galaxies;
    public int GalaxyIndex = 0;
    public bool Zoom = false;
    public GameObject d3_Origin;
    public float RotationSpeed = 1;
    public int Galaxy_Colour_Index = 0;
    private int Previous_Galaxy_Colour = 0;
    public bool ChangeAll = false;
    public bool showInfo = false;
    [HideInInspector] public bool toggleChannelFormFits = false;

    private Renderer galaxyRenderer; // Cached reference to the Renderer component
    private GalaxyManager galaxyManager => GalaxyManager.Instance;

    // Start is called before the first frame update
    void Start()
    {
        Galaxies = FindObjectsOfType<FITSObject>();
        galaxyRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Galaxies.Length != galaxyManager.GetFITSObjectsList.Count)
        {
            Galaxies = new FITSObject[galaxyManager.GetFITSObjectsList.Count];
            Galaxies = galaxyManager.GetFITSObjectsList.ToArray();
        }

        if (Zoom && GalaxyIndex < Galaxies.Length) // Ensure GalaxyIndex is within bounds
        {
            d3_Origin.transform.position = Galaxies[GalaxyIndex].transform.position;
            d3_Origin.transform.LookAt(Galaxies[GalaxyIndex].transform);
            d3_Origin.transform.Rotate(Vector3.up * (RotationSpeed * Time.deltaTime));
        }
        else
        {
            d3_Origin.transform.position = Vector3.zero;
            d3_Origin.transform.Rotate(Vector3.up * (RotationSpeed * Time.deltaTime));
        }

        if (Galaxy_Colour_Index != Previous_Galaxy_Colour)
        {
            ToggleColours(Galaxy_Colour_Index);
            Previous_Galaxy_Colour = Galaxy_Colour_Index;
        }

        //if (GalaxyIndex < Galaxies.Length) // Ensure GalaxyIndex is within bounds
        //{
        //    Galaxies[GalaxyIndex].gameObject.GetComponent<Galaxy>().ShowHideGalaxyInfo(showInfo);
        //}
    }

    public void ToggleColours(int ColourIndex)
    {
        if (toggleChannelFormFits)
        {
            Vector3 v1;
            Vector3 v2;
            Vector3 v3;
            switch (ColourIndex)
            {
                case 0:
                    v1 = new Vector3(1, 0, 0);
                    v2 = new Vector3(0, 1, 0);
                    v3 = new Vector3(0, 0, 1);
                    break;
                case 1:
                    v1 = Vector3.one;
                    v2 = Vector3.zero;
                    v3 = Vector3.zero;
                    break;
                case 2:
                    v1 = Vector3.zero;
                    v2 = Vector3.one;
                    v3 = Vector3.zero;
                    break;
                case 3:
                    v1 = Vector3.zero;
                    v2 = Vector3.zero;
                    v3 = Vector3.one;
                    break;
                default:
                    v1 = Vector3.one;
                    v2 = Vector3.one;
                    v3 = Vector3.one;
                    break;
            }

            if (ChangeAll)
            {
                foreach (FITSObject fit in Galaxies)
                {
                    if (fit.enabled)
                    {
                        try
                        {
                            fit.ImageData[0].Settings.RGBMultiplier = v1;
                            fit.ImageData[1].Settings.RGBMultiplier = v2;
                            fit.ImageData[2].Settings.RGBMultiplier = v3;
                            fit.UpdateFITS(fit.ImageData, recut: false);
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex, this);
                        }
                    }
                }
            }
            else
            {
                //if (GalaxyIndex < FITSGalaxies.Count) // Ensure GalaxyIndex is within bounds
                {
                    FITSObject galaxy = galaxyManager.GetSelectectGalaxy;
                    galaxy.ImageData[0].Settings.RGBMultiplier = v1;
                    galaxy.ImageData[1].Settings.RGBMultiplier = v2;
                    galaxy.ImageData[2].Settings.RGBMultiplier = v3;
                    galaxy.UpdateFITS(galaxy.ImageData, recut: false);
                }
            }
        }
        else
        {
            if (ChangeAll)
            {
                foreach (FITSObject fit in Galaxies)
                {
                    if (fit.enabled)
                    {
                        SetShaderChannel(fit, ColourIndex);
                    }
                }
            }
            else
            {
                FITSObject galaxy = galaxyManager.GetSelectectGalaxy;

                SetShaderChannel(galaxy, ColourIndex);
            }
        }
    }

    private void SetShaderChannel(FITSObject fit, int channel)
    {
        Material material = fit.GetComponent<MeshRenderer>().material;

        if (material)
        {
            material.SetFloat("_Channel", channel);
        }
    }
}
