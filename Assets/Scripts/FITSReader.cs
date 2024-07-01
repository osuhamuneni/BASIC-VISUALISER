#define debug
using nom.tam.fits;
using System;
using System.IO;
using UnityEngine;

namespace FitsReader
{
    /// <summary>
    /// This is the main class of the FITSReader. This allows us to get Texture2D from FITS files.
    /// </summary>
    public class FITSReader : MonoBehaviour
    {
        ///HSVData RGB, these are base values for the internal system of the FITS reader containing the three primitive colors.
        public static HSVData HSVRed = new HSVData();
        public static HSVData HSVGreen = new HSVData();
        public static HSVData HSVBlue = new HSVData();
        public static Color SkyboxColor;
        //Checks if the reader has been properly initialized.
        public static bool Initialized;
        public static float RA, dec;

        public static Fits GetFitsFromPath(string path)
        {
            string file = Path.GetTempFileName();
            path = Path.Combine(Application.streamingAssetsPath, path);

            File.Copy(path, file, true);
            Fits f = new Fits(file);
            return f;
        }

        public static void loadObjectFromFits(FITSImage[] imageData, GameObject instance, float scale)
        {
            for (int i = 0; i < imageData.Length; i++)
            {
                Fits FITSObject;
                if (imageData[i].Settings.SourceMode == FITSSourceMode.Disk)
                {
                    FITSObject = imageData[i].preload ?? FITSReader.GetFitsFromPath(imageData[i].Path); //method takes path returns fit 
                }
                else
                {
                    FITSObject = imageData[i].preload ?? FITSReader.GetFitsFromPath(imageData[i].Path);
                }
                ImageHDU h = (ImageHDU)FITSObject.readHDU();

                Vector3 worldPos;

                string[] stringRA = h.Header.FindCard("RA").Value.Split(' ');
                string[] stringDEC = h.Header.FindCard("DEC").Value.Split(' ');
                if (stringRA[0].Contains(":"))
                {
                    string[] stringRAparsed = stringRA[0].Split(':');
                    string[] stringDECparsed = stringDEC[0].Split(':');
                    RA = (float.Parse(stringRAparsed[0]) * 15 + (float.Parse(stringRAparsed[1]) / 4) + (float.Parse(stringRAparsed[2].Replace(".", ",")) / 240));
                    dec = (float.Parse(stringDECparsed[0]) + (float.Parse(stringDECparsed[1]) / 60) + (float.Parse(stringDECparsed[2].Replace(".", ",")) / 3600));
                }
                else if (stringRA[0].Contains("."))
                {
                    RA = float.Parse(stringRA[0].Replace(".", ","));
                    dec = float.Parse(stringDEC[0].Replace(".", ","));
                }
                else if (stringRA[0].Contains(""))
                {
                    RA = 1;
                    dec = 1;
                }

                worldPos = getWorldPosistionFromGalactic(RA, dec);
                Debug.Log("world pos set in fits reader: " + worldPos.x + " : " + worldPos.z);
                Debug.Log("instance object name: " + instance.name);
                //Create the Object
                //instance.transform.parent.transform.position = worldPos * scale;
                instance.transform.position = worldPos * scale;
            }
        }

        public static Vector3 getWorldPosistionFromGalactic(float ra, float dec)
        {
            float x, y, z;
            float x1, y1, z1;

            x = Mathf.Cos(ra * Mathf.Deg2Rad) * Mathf.Cos(dec * Mathf.Deg2Rad);
            y = Mathf.Sin(ra * Mathf.Deg2Rad) * Mathf.Cos(dec * Mathf.Deg2Rad);
            z = Mathf.Sin(dec * Mathf.Deg2Rad);

            x1 = 0.999926f * x - 0.011179f * y - 0.004859f * z;
            y1 = 0.011179f * x + 0.999938f * y - 0.000027f * z;
            z1 = 0.004859f * x + 0.000027f * y + 0.999988f * z;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Denoise and/or colorize the image based on the settings provided
        /// </summary>
        private static Color DenoiseColorize(Color color, FITSReadSettings Settings)
        {
            //If the settings specify that you want the image denoised, remap each color component with Log10 to reduce variation
            if (Settings.Denoise && color.maxColorComponent > 0)
            {
                color.r = Remap(Mathf.Log10(color.r) * color.r, 0, color.maxColorComponent, 0, 1);
                color.g = Remap(Mathf.Log10(color.g) * color.g, 0, color.maxColorComponent, 0, 1);
                color.b = Remap(Mathf.Log10(color.b) * color.b, 0, color.maxColorComponent, 0, 1);
            }

            //If the settings specify that you want the image colorized, process the pixel with the settings provided
            if (Settings.ColorizeMode != ColorizeMode.None)
            {
                //Create anHSVData for each color channel to be processed individually
                HSVData RData = new HSVData(HSVRed.H, 1);
                Color.RGBToHSV(color, out _, out _, out RData.V);

                HSVData GData = new HSVData(HSVGreen.H, 1);
                Color.RGBToHSV(color, out _, out _, out GData.V);

                HSVData BData = new HSVData(HSVBlue.H, 1);
                Color.RGBToHSV(color, out _, out _, out BData.V);

                //Colorize based on the mode selected
                switch (Settings.ColorizeMode)
                {
                    //Screen blending
                    case ColorizeMode.Screen:
                        color = ScreenBlend(ScreenBlend(
                            Color.HSVToRGB(RData.V * Settings.RGBMultiplier.x, RData.S, RData.V),
                            Color.HSVToRGB(GData.V * Settings.RGBMultiplier.y, GData.S, GData.V)),
                            Color.HSVToRGB(BData.V * Settings.RGBMultiplier.z, BData.S, BData.V));
                        break;

                    //Screen blending with Sin function
                    case ColorizeMode.ScreenSin:
                        color = ScreenBlend(ScreenBlend(
                            Color.HSVToRGB(Mathf.Sin(RData.V * Settings.RGBMultiplier.x), RData.S, RData.V),
                            Color.HSVToRGB(Mathf.Sin(GData.V * Settings.RGBMultiplier.y), GData.S, GData.V)),
                            Color.HSVToRGB(Mathf.Sin(BData.V * Settings.RGBMultiplier.z), BData.S, BData.V));
                        break;

                    //Screen blending with Log10 function
                    case ColorizeMode.ScreenLog10:
                        color = ScreenBlend(ScreenBlend(
                            Color.HSVToRGB(Remap(Mathf.Log10(RData.V * Settings.RGBMultiplier.x + Settings.RGBMultiplier.x), 0, RData.V, 0, 0.5f), RData.S, RData.V),
                            Color.HSVToRGB(Remap(Mathf.Log10(GData.V * Settings.RGBMultiplier.y + Settings.RGBMultiplier.y), 0, GData.V, 0, 0.5f), GData.S, GData.V)),
                            Color.HSVToRGB(Remap(Mathf.Log10(BData.V * Settings.RGBMultiplier.z + Settings.RGBMultiplier.z), 0, BData.V, 0, 0.5f), BData.S, BData.V));
                        break;

                    //Colorize based on a preset gradient based on brightness level
                    case ColorizeMode.Gradient:
                        if (Settings.MultiplyWithBrightness)
                            color = Settings.ColorGradient.Evaluate(Mathf.Clamp(color.maxColorComponent * Settings.GradientMultiplier, 0.001f, 0.999f)) * color.maxColorComponent;
                        else
                            color = Settings.ColorGradient.Evaluate(Mathf.Clamp(color.maxColorComponent * Settings.GradientMultiplier, 0.001f, 0.999f));
                        break;

                    //Colorize based on a preset gradient based on brightness with an exponent of 2
                    case ColorizeMode.GradientExp2:
                        if (Settings.MultiplyWithBrightness)
                            color = Settings.ColorGradient.Evaluate(Mathf.Clamp((Mathf.Exp(2) * color.maxColorComponent) * Settings.GradientMultiplier, 0.001f, 0.999f)) * color.maxColorComponent;
                        else
                            color = Settings.ColorGradient.Evaluate(Mathf.Clamp((Mathf.Exp(2) * color.maxColorComponent) * Settings.GradientMultiplier, 0.001f, 0.999f));
                        break;

                    //Colorize based on a preset gradient based on brightness with an exponent of 4
                    case ColorizeMode.GradientExp4:
                        if (Settings.MultiplyWithBrightness)
                            color = Settings.ColorGradient.Evaluate(Mathf.Clamp((Mathf.Exp(4) * color.maxColorComponent) * Settings.GradientMultiplier, 0.001f, 0.999f)) * color.maxColorComponent;
                        else
                            color = Settings.ColorGradient.Evaluate(Mathf.Clamp((Mathf.Exp(4) * color.maxColorComponent) * Settings.GradientMultiplier, 0.001f, 0.999f));
                        break;
                }
            }

            //If the color has any value, we can check and set hue and remaps
            if (color.maxColorComponent > 0)
            {
                //Shift the hue if specified
                if (Settings.Hue != 0)
                {
                    //Create HSVData for shifting
                    HSVData Shifted = new HSVData();
                    //Convert the Color to HSVData
                    Color.RGBToHSV(color, out Shifted.H, out Shifted.S, out Shifted.V);
                    //Shift the hue
                    Shifted.H += Settings.Hue;
                    //Convert the HSVData back to Color
                    color = Color.HSVToRGB(Shifted.H, Shifted.S, Shifted.V);
                }

                //Process the remaps if specified
                if (Settings.Remap.RedMax > 0)
                    color.r = Remap(color.r * Settings.RGBMultiplier.x, 0, 1, Settings.Remap.RedMin, Settings.Remap.RedMax);
                else
                    color.r *= Settings.RGBMultiplier.x;

                if (Settings.Remap.GreenMax > 0)
                    color.g = Remap(color.g * Settings.RGBMultiplier.y, 0, 1, Settings.Remap.GreenMin, Settings.Remap.GreenMax);
                else
                    color.g *= Settings.RGBMultiplier.y;

                if (Settings.Remap.BlueMax > 0)
                    color.b = Remap(color.b * Settings.RGBMultiplier.z, 0, 1, Settings.Remap.BlueMin, Settings.Remap.BlueMax);
                else
                    color.b *= Settings.RGBMultiplier.z;
            }

            //Blend colors based on threshold settings
            if (Settings.ThresholdSettings.ThresholdType != ThresholdType.None)
            {
                if (color.maxColorComponent <= Settings.ThresholdSettings.Threshold)
                {
                    switch (Settings.ThresholdSettings.ThresholdType)
                    {
                        case ThresholdType.Color:
                            color = ThresholdBlend(color, Settings.ThresholdSettings.ThresholdColor, Settings.ThresholdSettings.ThresholdBlendMode);
                            break;
                    }
                }
            }

            //Return the colorized pixel
            return color;
        }

        /// <summary>
        /// Blends two colors based on threshold settings
        /// </summary>
        public static Color ThresholdBlend(Color original, Color blend, ThresholdBlendMode blendMode)
        {
            switch (blendMode)
            {
                case ThresholdBlendMode.None:
                    return blend;

                case ThresholdBlendMode.Lerp:
                    return Color.Lerp(original, blend, 0.925f);
            }

            return original;
        }

        /// <summary>
        /// Boost the value of a decimal number based on the settings provided
        /// </summary>
        public static decimal BoostValue(FITSReadSettings Settings, decimal Value)
        {
            decimal Boost;
            switch (Settings.BoostMode)
            {
                case BoostMode.Difference:
                    Boost = (decimal)Settings.LowBoost - Value;

                    if (Boost > 1)
                        Value *= Boost;
                    break;

                case BoostMode.DifferenceExp2:
                    Boost = ((decimal)Settings.LowBoost - Value) * (decimal)Mathf.Exp(2);

                    if (Boost > 1)
                        Value *= Boost;
                    break;

                case BoostMode.DifferenceExp4:
                    Boost = ((decimal)Settings.LowBoost - Value) * (decimal)Mathf.Exp(4);

                    if (Boost > 1)
                        Value *= Boost;
                    break;

                case BoostMode.DifferenceExp6:
                    Boost = ((decimal)Settings.LowBoost - Value) * (decimal)Mathf.Exp(6);

                    if (Boost > 1)
                        Value *= Boost;
                    break;
            }

            return Value;
        }

        /// <summary>
        /// Screen blending function, provides the math necessary to blend RGB channels of a base and an overlay pixel
        /// </summary>
        public static Color ScreenBlend(Color Base, Color Overlay)
        {
            var R = 1 - (1 - Base.r) * (1 - Overlay.r);
            var G = 1 - (1 - Base.g) * (1 - Overlay.g);
            var B = 1 - (1 - Base.b) * (1 - Overlay.b);

            return new Color(R, G, B);
        }

        /// <summary>
        /// Remaps "from" from the range "fromMin" and "fromMax" to a new range, "toMin" and "toMax"
        /// </summary>
        public static float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }

        /// <summary>
        /// Remaps "from" from the range "fromMin" and "fromMax" to a new range, "toMin" and "toMax"
        /// </summary>
        public static decimal Remap(decimal from, decimal fromMin, decimal fromMax, decimal toMin, decimal toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            if (fromMaxAbs == 0)
                return 0;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }

        /// <summary>
        /// Force parse the image data from the supplied array. Basically a nutcracker type situation. Built to handle many number types and nested arrays
        /// </summary>
        public static void GetImageData(Array Array, ref FITSImageData ImageData)
        {
            foreach (var ArrayItem in Array)
            {
                Type CurrentType = ArrayItem.GetType();
                if (CurrentType == typeof(Array[]))
                {
                    GetImageData((Array[])ArrayItem, ref ImageData);
                    return;
                }
                else if (CurrentType == typeof(Array))
                {
                    GetImageData((Array)ArrayItem, ref ImageData);
                    return;
                }
                else //Parsing
                {
                    bool FoundType = false;

                    //Whole number parsing
                    if (CurrentType == typeof(int[]))
                    {
                        FoundType = true;
                        int[] Data = (int[])ArrayItem;

                        if (Data.Length > ImageData.XDimension)
                            ImageData.XDimension = Data.Length;

                        for (int i = 0; i < Data.Length; i++)
                        {
                            ImageData.Pixels[i + ImageData.ArrayFill] = Mathf.Abs(Data[i]);
                        }
                        ImageData.ArrayFill += Data.Length;
                    }
                    else if (CurrentType == typeof(uint[]))
                    {
                        FoundType = true;
                        uint[] Data = (uint[])ArrayItem;

                        if (Data.Length > ImageData.XDimension)
                            ImageData.XDimension = Data.Length;

                        for (int i = 0; i < Data.Length; i++)
                        {
                            ImageData.Pixels[i + ImageData.ArrayFill] = Data[i];
                        }
                        ImageData.ArrayFill += Data.Length;
                    }
                    else if (CurrentType == typeof(short[]))
                    {
                        FoundType = true;
                        short[] Data = (short[])ArrayItem;

                        if (Data.Length > ImageData.XDimension)
                            ImageData.XDimension = Data.Length;

                        for (int i = 0; i < Data.Length; i++)
                        {
                            ImageData.Pixels[i + ImageData.ArrayFill] = Data[i];
                        }
                        ImageData.ArrayFill += Data.Length;
                    }
                    else if (CurrentType == typeof(ushort[]))
                    {
                        FoundType = true;
                        ushort[] Data = (ushort[])ArrayItem;

                        if (Data.Length > ImageData.XDimension)
                            ImageData.XDimension = Data.Length;

                        for (int i = 0; i < Data.Length; i++)
                        {
                            ImageData.Pixels[i + ImageData.ArrayFill] = Data[i];
                        }
                        ImageData.ArrayFill += Data.Length;
                    }
                    else if (CurrentType == typeof(long[]))
                    {
                        FoundType = true;
                        long[] Data = (long[])ArrayItem;

                        if (Data.Length > ImageData.XDimension)
                            ImageData.XDimension = Data.Length;

                        for (int i = 0; i < Data.Length; i++)
                        {
                            ImageData.Pixels[i + ImageData.ArrayFill] = Data[i];
                        }
                        ImageData.ArrayFill += Data.Length;
                    }
                    else if (CurrentType == typeof(ulong[]))
                    {
                        FoundType = true;
                        ulong[] Data = (ulong[])ArrayItem;

                        if (Data.Length > ImageData.XDimension)
                            ImageData.XDimension = Data.Length;

                        for (int i = 0; i < Data.Length; i++)
                        {
                            ImageData.Pixels[i + ImageData.ArrayFill] = Data[i];
                        }
                        ImageData.ArrayFill += Data.Length;
                    }

                    //Floating point parsing
                    if (CurrentType == typeof(float[]))
                    {
                        FoundType = true;
                        float[] Data = (float[])ArrayItem;

                        if (Data.Length > ImageData.XDimension)
                            ImageData.XDimension = Data.Length;

                        for (int i = 0; i < Data.Length; i++)
                        {
                            ImageData.Pixels[i + ImageData.ArrayFill] = (decimal)Data[i];
                        }
                        ImageData.ArrayFill += Data.Length;
                    }
                    else if (CurrentType == typeof(double[]))
                    {
                        FoundType = true;
                        double[] Data = (double[])ArrayItem;

                        if (Data.Length > ImageData.XDimension)
                            ImageData.XDimension = Data.Length;

                        for (int i = 0; i < Data.Length; i++)
                        {
                            ImageData.Pixels[i + ImageData.ArrayFill] = (decimal)Data[i];
                        }
                        ImageData.ArrayFill += Data.Length;
                    }
                    else if (CurrentType == typeof(decimal[]))
                    {
                        FoundType = true;
                        decimal[] Data = (decimal[])ArrayItem;

                        if (Data.Length > ImageData.XDimension)
                            ImageData.XDimension = Data.Length;

                        for (int i = 0; i < Data.Length; i++)
                        {
                            ImageData.Pixels[i + ImageData.ArrayFill] = Data[i];
                        }
                        ImageData.ArrayFill += Data.Length;
                    }

                    if (!FoundType)
                        Debug.LogError("Failed to parse type: " + CurrentType.ToString());

                    ImageData.YDimension++;
                }
            }
        }
    }

    /// <summary>
    /// Holds the image data of a FITS image.
    /// </summary>
    public class FITSImageData
    {
        public int XDimension;
        public int YDimension;
        public int ArrayFill;
        public decimal[] Pixels = new decimal[10000000];
    }

    /// <summary>
    /// Holds colorized image data
    /// </summary>
    public class ColorImageData
    {
        public int XDimension;
        public int YDimension;
        public Color[] Pixels;

        public ColorImageData(int x = 0, int y = 0, int fill = 0)
        {
            XDimension = x;
            YDimension = y;

            if (fill > 0)
                Pixels = new Color[fill];
        }
    }

    /// <summary>
    /// HSVData, helps with colorization of FITS pixels
    /// </summary>
    public class HSVData
    {
        public HSVData(float HV = 0, float SV = 0, float VV = 0)
        {
            H = HV;
            S = SV;
            V = VV;
        }

        public float H;
        public float S;
        public float V;
    }

    /// <summary>
    /// Remap settings for the FITS image
    /// </summary>
    [Serializable]
    public class ColorizeRemap
    {
        [Header("Red")]
        public float RedMin = 0;
        public float RedMax = 1;

        [Header("Green")]
        public float GreenMin = 0;
        public float GreenMax = 1;

        [Header("Blue")]
        public float BlueMin = 0;
        public float BlueMax = 1;
    }

    /// <summary>
    /// Color range settings for the fITS image
    /// </summary>
    [Serializable]
    public struct ColorRange
    {
        public Color RangeColor;
        public Vector2 MinMax;
    }

    /// <summary>
    /// A settings holder for the FITS path and settings
    /// </summary>
    [Serializable]
    public struct FITSImage
    {
        public string Path;
        public bool isEnabled;
        public ColorBand colorBand;
        public FITSReadSettings Settings;
        [HideInInspector]
        public Fits preload;
    }

    /// <summary>
    /// Colorize modes for FITS settings
    /// </summary>
    public enum ColorizeMode
    {
        None,
        Gradient,
        GradientExp2,
        GradientExp4,
        Screen,
        ScreenSin,
        ScreenLog10,
    }

    /// <summary>
    /// Boost modes for FITS settings
    /// </summary>
    public enum BoostMode
    {
        None,
        Difference,
        DifferenceExp2,
        DifferenceExp4,
        DifferenceExp6
    }

    /// <summary>
    /// Power modes for FITS settings
    /// </summary>
    public enum PowerType
    {
        Automatic,
        High,
        None,
        Low,
        Lowest
    }

    /// <summary>
    /// Blending modes for FITS settings
    /// </summary>
    public enum BlendingMode
    {
        AdditiveEven,
        Additive,
        Multiply,
    }

    public enum FITSSourceMode
    {
        Disk,
        Web
    }

    /// <summary>
    /// Threshold modes for FITS settings
    /// </summary>
    public enum ThresholdType
    {
        None,
        Color
    }

    /// <summary>
    /// Threshold blending modes for FITS settings
    /// </summary>
    public enum ThresholdBlendMode
    {
        None,
        Lerp
    }

    /// <summary>
    /// Threshold settings for FITS settings
    /// </summary>
    [Serializable]
    public struct ThresholdSettings
    {
        public ThresholdType ThresholdType;
        public ThresholdBlendMode ThresholdBlendMode;
        public Color ThresholdColor;
        public float Threshold;
    }

    /// <summary>
    /// The settings for the FITS reader
    /// </summary>
    [Serializable]
    public class FITSReadSettings
    {
        [Header("FITS Source")]
        public FITSSourceMode SourceMode;
        [Header("Blending")]
        public BlendingMode BlendingMode;

        [Header("Offset")]
        public Vector2Int Offset;

        [Header("Brightness")]
        public float Multiplier = 1;
        public PowerType PowerType = PowerType.Automatic;

        [Header("Remove noisy pixels")]
        public bool Denoise;

        [Header("Threshold")]
        public ThresholdSettings ThresholdSettings;

        [Header("Colorization mode")]
        public ColorizeMode ColorizeMode = ColorizeMode.None;

        [Header("Colors multiplier (RGB)")]
        public Vector3 RGBMultiplier = Vector3.one;

        [Header("Colors remap ranges")]
        public ColorizeRemap Remap;

        [Header("Color gradient")]
        public Gradient ColorGradient;
        public bool MultiplyWithBrightness = true;

        [Header("Gradient multiplier")]
        public float GradientMultiplier = 1.0f;

        [Header("Hue shift (0-1)")]
        public float Hue;

        [Header("Boost lower values")]
        public float LowBoost = 2.0f;
        public BoostMode BoostMode;
    }
}