using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.Assertions;
using FitsReader;

public class ApplicationPerformanceTesting : MonoBehaviour
{
    public FITSObject fitsObject;

    public IEnumerator GalaxyLoadingTest()
    {
        fitsObject.loaded = false;
        yield return LoadGalaxy();

        // Add your assertions here to verify the expected behavior of the galaxy loading
        // For example:
        Assert.IsTrue(fitsObject.loaded, "Galaxy should be loaded");
        Assert.AreEqual(100, fitsObject.ImageData.Length, "Unexpected number of FITS images");
    }

    public void GalaxyLoadingPerformance()
    {
        //Measure.Method(() =>
        //{
        //    LoadGalaxy().MoveNext();
        //})
        //.Definition("Galaxy Loading")
        //.WarmupCount(3)
        //.MeasurementCount(5)
        //.IterationsPerMeasurement(10)
        //.Run();
    }

    private IEnumerator LoadGalaxy()
    {
        // Set up the FITS image data
        FITSImage[] imageData = new FITSImage[100]; // Load 100 FITS images for testing
        for (int i = 0; i < imageData.Length; i++)
        {
            imageData[i] = new FITSImage();
            //imageData[i].Path = "path/to/galaxy" + i + ".fits";
            // Set other properties of the imageData object as needed
        }

        // Assign the imageData to the fitsObject
        fitsObject.ImageData = imageData;

        // Load the FITS data and display it on the galaxy object
        fitsObject.Start();

        // Wait until the FITS data is loaded
        while (fitsObject.loading)
        {
            yield return null;
        }

        // The FITS data is now loaded and displayed on the galaxy object
        Debug.Log("Galaxy loaded!");
    }
}
