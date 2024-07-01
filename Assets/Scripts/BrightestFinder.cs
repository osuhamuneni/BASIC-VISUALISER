using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace basirua
{
    public struct BrightestPoint
    {
        public Vector2 position;
        public float brightness;
        public int taken;
    }

    public struct BrightnessCount
    {
        public Vector2 positionStart;
        public Vector2 positionEnd;
        public int brightnessCount;
    }

    public class BrightestFinder
    {
        private ComputeShader shader;
        private ComputeBuffer buffer;
        private ComputeBuffer takenBuffer;

        private BrightestPoint[] brightestPoints = new BrightestPoint[20];
        private Vector2[] takenData = new Vector2[20];
        private RenderTexture renderTex;

        public BrightestFinder()
        {
            shader = Resources.Load<ComputeShader>("Shaders/BrightestCompute");
            if (shader == null)
                Debug.LogError("Failed to load <BrightestCompute> Compute Shader");

            InitializeBuffers();
        }

        private void InitializeBuffers()
        {
            for (int i = 0; i < 20; i++)
            {
                brightestPoints[i] = new BrightestPoint() { brightness = -50, position = new Vector2(-900, -900), taken = 0 };
                takenData[i] = new Vector2(999, 999);
            }

            buffer = new ComputeBuffer(20, sizeof(float) * 3 + sizeof(int));
            buffer.SetData(brightestPoints);

            takenBuffer = new ComputeBuffer(20, sizeof(float) * 2);
            takenBuffer.SetData(takenData);

            shader.SetBuffer(0, "takenBuffer", takenBuffer);
            shader.SetBuffer(1, "takenBuffer", takenBuffer);
            shader.SetBuffer(0, "_BrightestPoint", buffer);
            shader.SetBuffer(1, "_BrightestPoint", buffer);
        }

        public Texture2D CalculateTexture(int h, int w, float r, float cx, float cy, Texture2D sourceTex)
        {
            Texture2D b = new Texture2D(h, w);
            Color[] pixels = new Color[h * w];
            Color black = Color.black;

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = black;

            for (int i = (int)(cx - r); i < cx + r; i++)
            {
                for (int j = (int)(cy - r); j < cy + r; j++)
                {
                    float dx = i - cx;
                    float dy = j - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    int pixelIndex = (i - (int)(cx - r)) * w + (j - (int)(cy - r));

                    if (d <= r)
                        pixels[pixelIndex] = sourceTex.GetPixel(i, j);
                }
            }

            b.SetPixels(pixels);
            b.Apply();
            return b;
        }

        public Vector2 FindBrightestManually(Texture2D textureToScan, float cutoff)
        {
            int width = textureToScan.width;
            int height = textureToScan.height;
            Color[] colorPixels = textureToScan.GetPixels();

            Vector2 brightestPointStart = Vector2.zero;
            Vector2 brightestPointEnd = Vector2.zero;
            int brightestSpotsCount = 0;
            List<BrightnessCount> brightnessList = new List<BrightnessCount>();

            for (int i = 0; i < colorPixels.Length; i++)
            {
                int pixelX = i % width;
                int pixelY = i / width;

                if (colorPixels[i].r > cutoff && colorPixels[i].g > cutoff && colorPixels[i].b > cutoff)
                {
                    if (brightestPointStart == Vector2.zero)
                    {
                        brightestPointStart = new Vector2(pixelX, pixelY);
                        brightestSpotsCount++;
                    }
                    else if (pixelY == brightestPointStart.y && pixelX == brightestPointStart.x + brightestSpotsCount)
                    {
                        brightestPointEnd = new Vector2(pixelX, pixelY);
                        brightestSpotsCount++;
                    }
                }
                else
                {
                    if (brightestPointStart != Vector2.zero && brightestPointEnd != Vector2.zero && brightestSpotsCount > 10)
                    {
                        BrightnessCount brightness = new BrightnessCount();
                        brightness.brightnessCount = brightestSpotsCount;
                        brightness.positionStart = brightestPointStart;
                        brightness.positionEnd = brightestPointEnd;
                        brightnessList.Add(brightness);
                    }

                    brightestSpotsCount = 0;
                    brightestPointStart = Vector2.zero;
                    brightestPointEnd = Vector2.zero;
                }
            }

            brightnessList = brightnessList.OrderByDescending(o => o.brightnessCount).ToList();

            if (brightnessList.Count > 0)
            {
                Vector2 result = Vector2.zero;
                float xMidPoint = (brightnessList[0].positionEnd.x - brightnessList[0].positionStart.x) / 2f;
                result = new Vector2(brightnessList[0].positionEnd.x - xMidPoint, brightnessList[0].positionStart.y);
                return result;
            }

            return Vector2.zero;
        }
    }
}