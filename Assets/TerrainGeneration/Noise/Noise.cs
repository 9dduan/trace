using System;
using UnityEngine;
using System.Collections;

/** from Sebastian Lague
 */

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight,int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
	{
        float[,] noiseMap = new float[mapWidth, mapHeight];
		Vector2[] octaveOffsets = new Vector2[octaves];

		System.Random prng = new System.Random(seed);

		for (int i=0; i < octaves; i++)
        {
			float offsetX = prng.Next(-100000, 100000) + offset.x;
			float offsetY = prng.Next(-100000, 100000) + offset.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

		if (scale <= 0)
		{
			scale = 0.0001f; //clamp to min value
		}

		float maxNoiseHeight = float.MinValue;
		float minNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;

        for(int y = 0; y < mapHeight; y++)
		{
            for(int x = 0; x < mapWidth; x++)
			{
				float amplitude = 1;
				float frequency = 1;
				float noiseHeight = 0;

                for(int i = 0; i < octaves; i++)
                {
					float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
					float sampleY = (y - halfHeight)/ scale * frequency + octaveOffsets[i].y;

					/**
                        simplex noise 
                        mix octaves of perlin noise 
					    https://www.cnblogs.com/babyrender/archive/2008/10/27/BabyRender.html
                    */  

					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // perlinNoise return value from 0 to 1, we want to have -1 to 1
					noiseHeight += perlinValue * amplitude;

					amplitude *= persistance;
					frequency *= lacunarity; // higher frequency => more separated sampled positions => fuzzier wave
				}

                if(noiseHeight > maxNoiseHeight)
                {
					maxNoiseHeight = noiseHeight;
                }
                else if(noiseHeight < minNoiseHeight)
                {
					minNoiseHeight = noiseHeight;
                }

				noiseMap[x, y] = noiseHeight;
			}
		}

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
				noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

		return noiseMap;
	}
}
