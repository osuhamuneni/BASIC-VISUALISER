using UnityEngine;

public static class GalaxyRadialProfile
{
    // Function to calculate the radial profile from the galaxy texture
    public static float CalculateRadialProfile(Texture2D galaxyTexture, Vector2? center = null)
    {
        int centerX = galaxyTexture.width / 2;
        int centerY = galaxyTexture.height / 2;
        if(center.HasValue)
        {
            centerX = (int)center.Value.x;
            centerY = (int)center.Value.y;
        }
        int maxRadius = Mathf.Min(centerX, centerY);

        float sumIntensity = 0;
        int count = 0;

        for (int i = 0; i < galaxyTexture.width; i++)
        {
            for (int j = 0; j < galaxyTexture.height; j++)
            {
                float distance = Vector2.Distance(new Vector2(centerX, centerY), new Vector2(i, j));

                if (distance <= maxRadius)
                {
                    Color pixelColor = galaxyTexture.GetPixel(i, j);
                    float pixelIntensity = (pixelColor.r + pixelColor.g + pixelColor.b) / 3;
                    sumIntensity += pixelIntensity;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            return sumIntensity / count;
        }

        return 0;
    }
}
