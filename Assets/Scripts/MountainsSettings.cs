using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MountainsSettings
{
    public int numLayers = 5;
    public float lacunarity = 2;
    public float persistence = 0.5f;
    public float scale = 1;
    public float power = 2;
    public float elevation = 1;
    public float gain = 1;
    public float verticalShift = 0;
    public float peakSmoothing = 0;
    public Vector3 offset;

    public float[] GetSettingArray()
    {
		Vector3 seededOffset = new Vector3(0f, 0f, 0f) * 0f * 10000;

		float[] noiseParams = {
			// [0]
			seededOffset.x + offset.x,
			seededOffset.y + offset.y,
			seededOffset.z + offset.z,
			numLayers,
			// [1]
			persistence,
			lacunarity,
			scale,
			elevation,
			// [2]
			power,
			gain,
			verticalShift,
			peakSmoothing
		};

		return noiseParams;
	}
}
