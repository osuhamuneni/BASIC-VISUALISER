using UnityEngine;
using Unity.PerformanceTesting;
using NUnit.Framework;
using UnityEngine.Profiling;
using System.Diagnostics;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class BasicVisualizerPerformancesTest
{
    [UnityTest, Performance]
    public IEnumerator GlobalPerformanceTestInAppScene()
    {
        // Run the performance test coroutine
        yield return SceneLoaderPerformanceTest();
    }

    private IEnumerator SceneLoaderPerformanceTest()
    {
        // Load the scene asynchronously
        var sceneLoadOperation = SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Single);

        // Wait until the scene is fully loaded
        while (!sceneLoadOperation.isDone)
        {
            yield return null;
        }

        yield return MeasureFramerate();
        yield return MeasureMemory();
        yield return MeasureCPUUsage();

        // Unload the scene
        SceneManager.UnloadSceneAsync("SampleScene");
    }

    [Performance]
    public IEnumerator MeasureFramerate()
    {
        SampleGroup sampleGroup = new SampleGroup("FramesPerSecond", SampleUnit.Undefined, true);

        for (int i = 0; i < 200; i++)
        {
            float framesPerSecond = 1f / Time.deltaTime;
            Measure.Custom(sampleGroup, framesPerSecond);
            yield return new WaitForSeconds(0.1f); // Delay between measurements
        }
    }

    [Performance]
    public IEnumerator MeasureMemory()
    {
        SampleGroup sampleGroup = new SampleGroup("TotalAllocatedMemory", SampleUnit.Megabyte, false);

        for (int i = 0; i < 200; i++)
        {
            float allocatedMemory = Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
            Measure.Custom(sampleGroup, allocatedMemory);

            yield return new WaitForSeconds(0.1f); // Add a 0.1 second delay between each measurement
        }
    }


    [Performance]
    public IEnumerator MeasureCPUUsage()
    {
        SampleGroup sampleGroup = new SampleGroup("CPUUsage", SampleUnit.Undefined, true);

        PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        for (int i = 0; i < 200; i++)
        {
            float cpuUsage = cpuCounter.NextValue();
            Measure.Custom(sampleGroup, cpuUsage);
            yield return new WaitForSeconds(0.1f); // Delay between measurements
        }
    }
}
