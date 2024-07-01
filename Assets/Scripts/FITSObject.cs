using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FitsReader;
using nom.tam.fits;
using System;
using UnityEngine.Networking;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using basirua;
using ICSharpCode.SharpZipLib.BZip2;


public class FITSObject : MonoBehaviour,ITextureHandler
{
    public bool isLoadOnStart = true;
    public FITSImage[] ImageData;
    private FITSImage[] imageDataBackup;
    public List<Vector3> rgbMultiplierBackups;
    [Header("Texture settings")]
    public TextureWrapMode WrapMode;
    public GameObject objectToInstatiate;
    public float scale;
    public Texture2D fitsTexture;
    RenderTexture renderTexture;
    public ComputeShader Colorize;
    //public ComputeShader Offset;
    //public ComputeShader Combine;
    public ComputeShader remap;
    //public ComputeShader DarkenShader;
    public bool loading = false;
    public bool firstTimeLoad = true;
    private bool isImageDataBackup = false;

    [Header("Data Loader")]
    public Texture2D loadTexture;
    public string fitsPath;
    internal bool loaded = false;

    public string GameObjectName { get { return gameObject.name.Replace("(Clone)", ""); } }

    public FITSImage[] ImageDataBackup { get => imageDataBackup; set => imageDataBackup = value; }

    private void Awake()
    {
        fitsPath = Application.persistentDataPath + "/" + GameObjectName + "/" + "fits";
        if (!Directory.Exists(fitsPath))
        {
            Directory.CreateDirectory(fitsPath);
        }
    }

    private void LoadComputeShaders()
    {
        if (Colorize == null)
        {
            Colorize = Resources.Load<ComputeShader>("Shaders/Colorize");
            if (Colorize == null)
                Debug.LogError("Failed to load <Colorize> Compute Shader");
        }

        //if (Offset == null)
        //{
        //    Offset = Resources.Load<ComputeShader>("Shaders/ImageProccessing");
        //    if (Offset == null)
        //        Debug.LogError("Failed to load <ImageProccessing> Compute Shader");
        //}

        //if (Combine == null)
        //{
        //    Combine = Resources.Load<ComputeShader>("Shaders/MaximumAndMinimum");
        //    if (Combine == null)
        //        Debug.LogError("Failed to load <MaximumAndMinimum> Compute Shader");
        //}

        if (remap == null)
        {
            remap = Resources.Load<ComputeShader>("Shaders/Remap");
            if (remap == null)
                Debug.LogError("Failed to load <Remap> Compute Shader");
        }

        //if (DarkenShader == null)
        //{
        //    DarkenShader = Resources.Load<ComputeShader>("Shaders/Darken");
        //    if (DarkenShader == null)
        //        Debug.LogError("Failed to load <Darken> Compute Shader");
        //}
    }

    public void Start()
    {
        LoadComputeShaders();

        isImageDataBackup = false;

        if (isLoadOnStart)
        {
            Texture2D tex = LoadFromPath(1024, 1536);
            if (tex != null)
            {
                fitsTexture = tex;

                if (ImageData[0].Settings.SourceMode == FITSSourceMode.Web && ImageData[0].Path.ToLower().Contains("http"))
                    ImageData[0].Path = fitsPath + "/" + GameObjectName + "_fits_0.fits";

                if (ImageData[1].Settings.SourceMode == FITSSourceMode.Web && ImageData[1].Path.ToLower().Contains("http"))
                    ImageData[1].Path = fitsPath + "/" + GameObjectName + "_fits_1.fits";

                if (ImageData[2].Settings.SourceMode == FITSSourceMode.Web && ImageData[2].Path.ToLower().Contains("http"))
                    ImageData[2].Path = fitsPath + "/" + GameObjectName + "_fits_2.fits";

                FITSReader.loadObjectFromFits(ImageData, gameObject, scale);
                CutNPlaceOnCenter(tex, true);
            }
            else
            {
                LoadFITS(false);
            }
        }        
    }

    private void BackupImageData()
    {
        if (!isImageDataBackup)
        {
            ImageDataBackup = ImageData;
            isImageDataBackup = true;
            Debug.Log(ImageDataBackup[0].Settings.RGBMultiplier);
            rgbMultiplierBackups = new List<Vector3>();
            foreach (FITSImage i in ImageDataBackup)
            {
                rgbMultiplierBackups.Add(i.Settings.RGBMultiplier);
            }
        }
    }

    private void LoadFITS(bool webPass = false)
    {
        loading = true;
        if (ImageData.Length == 0)
        {
            Debug.LogError("ImageData length on object: " + GameObjectName + " is 0, FITSObject must have at least 1 fits file to load");
            return; // if there are no images for us to load then exit the load function so that game doesn't scrash.
        }

        if (GalaxyManager.Instance.LoadFromCSV && !webPass)
        {
            GalaxyInfos galaxyInfos = GetComponent<Galaxy>().GetGalaxyInfos;
            if (galaxyInfos == null)
                return;

            for (int i = 0; i < ImageData.Length; i++)
            {
                ImageData[i].Settings.SourceMode = FITSSourceMode.Web;
                switch (ImageData[i].colorBand)
                {
                    case ColorBand.Red:
                        ImageData[i].Path = galaxyInfos.RBand;
                        break;
                    case ColorBand.Green:
                        ImageData[i].Path = galaxyInfos.GBand;
                        break;
                    case ColorBand.Blue:
                        ImageData[i].Path = galaxyInfos.IBand;
                        break;
                    case ColorBand.UV:
                        ImageData[i].Path = galaxyInfos.UBand;
                        break;
                    case ColorBand.XRay:
                        ImageData[i].Path = galaxyInfos.ZBand;
                        break;
                }
            }
        }

        if (!webPass)
        {
            for (int i = 0; i < ImageData.Length; i++)
            {
                if (ImageData[i].Settings.SourceMode == FITSSourceMode.Web)
                {
                    StartCoroutine(PreloadFITS(ImageData));
                    return;
                }
            }
        }
        UpdateFITS(ImageData);
    }

    /// <summary>
    /// Async UpdateFITS
    /// </summary>
    /// <param name="imageData"></param>
    async public void UpdateFITS(FITSImage[] imageData, int length = -1, bool recut = true)
    {
        renderTexture = null;
        length = length == -1 ? imageData.Length : length;

        for (int i = 0; i < length; i++)
        {
            Fits FITSObject = FITSReader.GetFitsFromPath(ImageData[i].Path); // this  loads the image from the path

            Debug.Log($"this is the path of {GameObjectName},{ImageData[i].Path}");

            ImageHDU HDU = (ImageHDU)FITSObject.readHDU();
            ImageData HDUIData = (ImageData)HDU.Data;

            Array[] dataArray = (Array[])HDUIData.DataArray;
            Array[] result = await LoopOverData(dataArray);
            float[] intList = (float[])result[0];
            float min = (float)result[1].GetValue(0);
            float max = (float)result[1].GetValue(1);
            int XDimension = dataArray[0].Length;
            int YDimension = dataArray.Length;

            float[] doublePixels = intList;

            if (i == 0)
            {
                renderTexture = new RenderTexture(XDimension, YDimension, 0);
                renderTexture.enableRandomWrite = true;
            }

            ComputeBuffer pixelsRemapBuffer = new ComputeBuffer(doublePixels.Length, sizeof(float));

            float[] remapResult = (float[])doublePixels.Clone();

            pixelsRemapBuffer.SetData(remapResult);
            remap.SetBuffer(0, "pixels", pixelsRemapBuffer);
            remap.SetFloat("min", min);
            remap.SetFloat("max", max);
            remap.Dispatch(0, 65535, 1, 1);

            pixelsRemapBuffer.GetData(remapResult);
            RenderTexture minTex = new RenderTexture(1536, 1024, 0);
            pixelsRemapBuffer.Release();
            minTex.enableRandomWrite = true;

            RenderTexture temp = renderTexture;
            temp.enableRandomWrite = true;
            ComputeBuffer pixelsColorizeBuffer = new ComputeBuffer(remapResult.Length, sizeof(float));
            ComputeBuffer blackBuffer = new ComputeBuffer(1, sizeof(float));
            pixelsColorizeBuffer.SetData(remapResult);
            float[] blacks = new float[1] { 0 };
            blackBuffer.SetData(blacks);
            Colorize.SetTexture(0, "Result", temp);
            Colorize.SetBuffer(0, "pixels", pixelsColorizeBuffer);
            Colorize.SetBuffer(0, "blacks", blackBuffer);
            Colorize.SetFloat("width", XDimension);
            Colorize.SetFloat("boost", ImageData[i].Settings.LowBoost);
            Colorize.SetFloat("threshold", ImageData[i].Settings.ThresholdSettings.Threshold);
            Colorize.SetFloat("XOffset", ImageData[i].Settings.Offset.x);
            Colorize.SetFloat("YOffset", ImageData[i].Settings.Offset.y);
            Colorize.SetVector("RGBMultipliers", ImageData[i].Settings.RGBMultiplier);
            Colorize.Dispatch(0, 512, 512, 1);
            pixelsColorizeBuffer.GetData(remapResult);
            blackBuffer.GetData(blacks);
            pixelsColorizeBuffer.Release();
            blackBuffer.Release();
        }

        FITSReader.loadObjectFromFits(ImageData, gameObject, scale);

        if (renderTexture != null)
            fitsTexture = ToTexture2D(renderTexture);

        if (fitsTexture != null)
        {
            fitsTexture.wrapMode = WrapMode;
            fitsTexture.Apply();

            CutNPlaceOnCenter(fitsTexture, recut);
            BackupImageData();
        }
    }

    private void CutNPlaceOnCenter(Texture2D tex, bool recut = true)
    {
        // -- CUT AND PLACE GALAXY ON CENTER !!!!!!!! --
        Renderer R = GetComponent<Renderer>();
        float bfScale = 360f;//This value seems to be stable for all the galaxies I tried, well i let the BrightestFinder method to get the scaling, and if the value is different then the galaxy isn't centered

        BrightestFinder bf = new BrightestFinder();
        //if (recut)
        Vector2 brightestPoint = bf.FindBrightestManually(tex, 0.6f);//0.6f
        tex = bf.CalculateTexture(1024, 1024, bfScale, brightestPoint.x, brightestPoint.y, tex);
        SaveToPath(tex);
        if (recut)
            SetGalaxyScaling(tex, brightestPoint);

        Shader shader = Shader.Find("Unlit/GalaxyCutShader");
        if (shader != null)
        {
            Debug.Log("<Unlit/GalaxyCutShader> has loaded correctly");
            // Shader is correctly loaded
            Material mat = new Material(shader);
            // Use the material for rendering
            mat.SetTexture("_MainTex", tex);
            mat.SetFloat("_Scale", bfScale);
            R.material = mat;
        }
        else
        {
            Debug.LogError("<Unlit/GalaxyCutShader> has not been found or loaded correctly");
        }
        
        loaded = true;
    }

    private void SetGalaxyScaling(Texture2D tex, Vector2 center)
    {
        float scaleRatio = GalaxyRadialProfile.CalculateRadialProfile(tex, center);
        float scaling = 20 * scaleRatio;
        float convertedScale = Mathf.Clamp(scaling, 0.25f, 1.5f);//prevent galaxies from being to small or big
        transform.localScale = new Vector3(transform.localScale.x * convertedScale, transform.localScale.y * convertedScale, 1);

        GetComponent<GalaxiesFaceCamera>().FaceCamera();
    }

    private Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    static async Task<Array[]> LoopOverData(Array[] dataArray)
    {
        // we're still on unity thread here
        return await Task.Run(
            () =>
            {
                float[] intList = new float[dataArray.Length * dataArray[0].Length];
                float max = 0;
                float min = 0;
                int index = 0;
                for (int u = 0; u < dataArray.Length; u++)
                {

                    foreach (var item in dataArray[u])
                    {
                        float castItem = Convert.ToSingle(item);

                        if (castItem < min) min = castItem;
                        if (castItem > max) max = castItem;

                        intList[index] = castItem;
                        index++;
                    }
                }
                return new Array[] { intList, new float[] { min, max } };
            }
        );
    }

    public IEnumerator PreloadFITS(FITSImage[] images)
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].Settings.SourceMode == FITSSourceMode.Web)
            {
                Debug.Log("Downloading FITS");
                Debug.Log(images[i].Path);
                UnityWebRequest www = UnityWebRequest.Get(images[i].Path);
                var dh = new DownloadHandlerFile(Application.dataPath + "/" + www.GetHashCode());
                dh.removeFileOnAbort = true;
                www.downloadHandler = dh;

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    Debug.Log("Download finished");
                    byte[] data = File.ReadAllBytes(Application.dataPath + "/" + www.GetHashCode());

                    Debug.Log(data);
                    Debug.Log("Above is the url fit downloaded");

                    if (images[i].Path.EndsWith(".gz"))  // this is whgen the fits file is stored as .gz , thats it zipped file, but it does not work lol
                    {
                        MemoryStream compressed = new MemoryStream(data);
                        using var decompressor = new GZipStream(compressed, CompressionMode.Decompress);

                        FileStream file = File.Create(fitsPath + "/" + GameObjectName + "_fits_" + i + ".fits");
                        decompressor.CopyTo(file);
                        file.Flush();
                        file.Close();

                        images[i].Path = "Assets/" + www.downloadHandler.GetHashCode() + ".fits";
                    }

                    else if (images[i].Path.EndsWith(".bz2"))  // this is whgen the fits file is stored as .gz , thats it zipped file, but it does not work lol
                    {
                        string bzPath = this.fitsPath + "/" + GameObjectName + "_fits_" + i + ".bz2";
                        string fitsPath = this.fitsPath + "/" + GameObjectName + "_fits_" + i + ".fits";

                        Debug.Log("Iam bz written");
                        FileStream file = File.Create(bzPath);
                        file.Flush();
                        file.Close();

                        File.WriteAllBytes(bzPath, data);  // write all byte that is data into the .bz file

                        FileStream fitsfile = File.Create(fitsPath);

                        BZip2.Decompress(File.OpenRead(bzPath), fitsfile, true);

                        images[i].Path = fitsPath;
                    }
                    else
                    {
                        images[i].Path = this.fitsPath + "/" + GameObjectName + "_fits_" + i + ".fits";
                    }
                }
            }
        }
        LoadFITS(true);
    }

    public void SaveToPath(Texture2D texture)
    {
        loadTexture = texture;
        byte[] getdata = texture.EncodeToPNG();
        var dirPath = Application.persistentDataPath + "/" + GameObjectName;
        if (Directory.Exists(dirPath))
        {
            SaveToPng(getdata, dirPath);
        }
        else
        {
            Directory.CreateDirectory(dirPath);
            SaveToPng(getdata, dirPath);
        }
    }

    public void SaveToPng(byte[] bytes, string path)
    {
        string fullPath = path + "/" + GameObjectName + ".png";
        File.WriteAllBytes(fullPath, bytes);
        Debug.Log("Texture saved as png to, " + fullPath);
    }

    public Texture2D LoadFromPath(int texH, int texW)
    {
        try
        {
            string filePath = Application.persistentDataPath + "/" + GameObjectName + "/" + GameObjectName + ".png";
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture2D = new Texture2D(texW, texH);
            texture2D.LoadImage(bytes);
            texture2D.name = GameObjectName.ToString();
            loadTexture = texture2D;
            return texture2D;
        }
        catch
        {
            Debug.Log("Textures not found reloading fits");
            return null;
        }        
    }    

    public void LoadFitsImages()
    {
        LoadFITS(false);
    }
}

public interface ITextureHandler
{
    public void SaveToPath(Texture2D texture);

    public void SaveToPng(byte[] bytes, string path);

    public Texture2D LoadFromPath(int texH,int texW);
}

public enum ColorBand
{
    Red, Green, Blue, UV, XRay
}