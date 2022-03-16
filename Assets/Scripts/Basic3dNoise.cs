using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Basic3dNoise
{
    public static float PerlinNoise3D(float x, float y, float z)
    {
        x = Mathf.Abs(x);
        y = Mathf.Abs(y);
        z = Mathf.Abs(z);

        y += 1;
        z += 2;
        float xy = _perlin3DFixed(x, y);
        float xz = _perlin3DFixed(x, z);
        float yz = _perlin3DFixed(y, z);
        float yx = _perlin3DFixed(y, x);
        float zx = _perlin3DFixed(z, x);
        float zy = _perlin3DFixed(z, y);
        return xy * xz * yz * yx * zx * zy;
    }
    static float _perlin3DFixed(float a, float b)
    {
        return Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a, b));
    }
}