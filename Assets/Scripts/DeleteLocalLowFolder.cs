using UnityEngine;

public class DeleteLocalLowFolder : MonoBehaviour
{
    public void DeleteFolderContents()
    {
        string companyFolder = "DefaultCompany";
        string projectFolder = "BasicVisualizer";
        string localLowPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)+"Low", companyFolder, projectFolder);

        if (System.IO.Directory.Exists(localLowPath))
        {
            // Delete all files and subdirectories inside the LocalLow folder
            System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(localLowPath);
            foreach (System.IO.FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (System.IO.DirectoryInfo dir in directoryInfo.GetDirectories())
            {
                dir.Delete(true);
            }

            Debug.Log("LocalLow folder contents deleted.");
        }
        else
        {
            Debug.Log("LocalLow folder does not exist.");
        }
    }
}
