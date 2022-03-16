using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlanetMesher : MonoBehaviour
{
    public ComputeShader heightMapCompute;
    public ComputeShader perturbCompute;

    public continentsSettings continentsSettings;
    public MountainsSettings mountainsSettings;
    public MaskSettings maskSettings;

    public Material PlanetMaterial;

    ComputeBuffer heightBuffer;
    ComputeBuffer vertexBuffer;

    public bool perturbMesh;

    void Awake()
    {
        if (!Application.isPlaying)//Equivilent to inEditor
        {
            CreatePlanetMesh();
        }
    }

    public Mesh CreatePlanetFace(Vector3[] plane, Vector3 direction, int radius)
    {
        int diameter = radius * 2;
        List<Vector3> verticies = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        Vector3 offset = (plane[0] * 0.5f * diameter) + (plane[1] * 0.5f * diameter); //The offset to center the face at 0,0
        Vector3 planetOffset = direction * radius; //How far from the center of the planet the face is

        //Vertex generation
        for (int x = 0; x < diameter + 1; x++)
        {
            for (int y = 0; y < diameter + 1; y++)
            {
                Vector3 pos = ((plane[0] * x) + (plane[1] * y) - offset + planetOffset) / radius; //Calculate point on plane with offsets applied
                pos = PointOnCubeToPointOnSphere(pos) * radius;
                verticies.Add(pos);
                normals.Add(pos.normalized);
            }
        }

        Vector3[] vertices = verticies.ToArray();
        ComputeHelper.CreateStructuredBuffer<Vector3>(ref vertexBuffer, verticies.Count);
        vertexBuffer.SetData(vertices);

        //Calculate heights
        heightMapCompute.SetInt("numVertices", vertexBuffer.count);
        heightMapCompute.SetBuffer(0, "vertices", vertexBuffer);
        ComputeHelper.CreateAndSetBuffer<float>(ref heightBuffer, vertexBuffer.count, heightMapCompute, "heights");

        heightMapCompute.SetFloat("oceanDepthMultiplier", 5);
        heightMapCompute.SetFloat("oceanFloorDepth", 1.5f);
        heightMapCompute.SetFloat("oceanFloorSmoothing", 0.5f);
        heightMapCompute.SetFloat("mountainBlend", 1.2f);


        heightMapCompute.SetFloats("noiseParams_continents", continentsSettings.GetSettingArray());
        heightMapCompute.SetFloats("noiseParams_mask", maskSettings.GetSettingArray());
        heightMapCompute.SetFloats("noiseParams_mountains", mountainsSettings.GetSettingArray());

        ComputeHelper.Run(heightMapCompute, vertexBuffer.count); //Applies noise to verticies and returns an array of a float per height value
        var heights = new float[vertexBuffer.count];
        heightBuffer.GetData(heights);

        //Perturb vertices
        if (perturbMesh)
        {
            perturbCompute.SetBuffer(0, "points", vertexBuffer);
            perturbCompute.SetInt("numPoints", verticies.Count);
            perturbCompute.SetFloat("maxStrength", 0.1f);

            ComputeHelper.Run(perturbCompute, verticies.Count);
        }

        vertexBuffer.GetData(vertices);

        vertexBuffer.Release();
        heightBuffer.Release();

        //Triangle generation
        for (int x = 1; x < diameter + 1; x++)
        {
            for (int y = 1; y < diameter + 1; y++)
            {
                //0-1
                //|\|
                //2-3
                int[] relevantVertexes = new int[4]
                {
                    ((y - 1) * (diameter + 1)) + x - 1,
                    ((y - 1) * (diameter + 1)) + x - 0,
                    ((y - 0) * (diameter + 1)) + x - 1,
                    ((y - 0) * (diameter + 1)) + x - 0,
                };
                //0-1
                // \|
                //  3
                triangles.AddRange(new int[] { relevantVertexes[0], relevantVertexes[3], relevantVertexes[1] });
                //0
                //|\
                //2-3
                triangles.AddRange(new int[] { relevantVertexes[0], relevantVertexes[2], relevantVertexes[3] });
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    public static Vector3 PointOnCubeToPointOnSphere(Vector3 p)
    {
        float x2 = p.x * p.x;
        float y2 = p.y * p.y;
        float z2 = p.z * p.z;
        float x = p.x * Mathf.Sqrt(1 - (y2 + z2) / 2 + (y2 * z2) / 3);
        float y = p.y * Mathf.Sqrt(1 - (z2 + x2) / 2 + (z2 * x2) / 3);
        float z = p.z * Mathf.Sqrt(1 - (x2 + y2) / 2 + (x2 * y2) / 3);
        return new Vector3(x, y, z);
    }

    private void DestroyChildren()
    {
        //Destroy results of previous generation
        Transform[] children = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != transform)
            {
                DestroyImmediate(children[i].gameObject);
            }
        }
    }

    public void CreatePlanetMesh()
    {
        DestroyChildren();

        //The conditions for all the faces
        Vector3[][] conditions = new Vector3[][]
        {
            new Vector3[]{ Vector3.forward, Vector3.up, Vector3.left },
            new Vector3[]{ Vector3.right, Vector3.up, Vector3.forward },
            new Vector3[]{ Vector3.left, Vector3.up, Vector3.back },
            new Vector3[]{ Vector3.back, Vector3.up, Vector3.right },
            new Vector3[]{ Vector3.left, Vector3.forward, Vector3.up },
            new Vector3[]{ Vector3.left, Vector3.back, Vector3.down },
        };

        //Add new generation in to world
        foreach (Vector3[] condition in conditions)
        {
            GameObject curFace = GameObject.CreatePrimitive(PrimitiveType.Quad);
            curFace.GetComponent<MeshFilter>().mesh = CreatePlanetFace(new Vector3[2] { condition[0], condition[1] }, condition[2], 100);
            curFace.transform.SetParent(transform);
            curFace.GetComponent<MeshRenderer>().material = PlanetMaterial;
        }
    }
}
