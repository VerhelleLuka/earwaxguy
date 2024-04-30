using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider2D))]
public class MarchingSquares : MonoBehaviour
{
    [SerializeField][Range(0, 200)] int size = 5;
    [SerializeField][Range(0.01f, .2f)] float noiseResolution = .1f;
    [SerializeField][Range(0.05f, 1f)] float resolution = 1f;
    [SerializeField][Range(0f, 1f)] float heightTreshold = .5f;
    [SerializeField][Range(0, 10)] float drawRange = 1f;

    [SerializeField][Range(0, 1)] float defaultHeight = 0f;
    [SerializeField] bool useDefaultHeight = false;

    private float[,] heights;

    public Transform circleParent;
    public Transform circlePrefab;

    private MeshFilter _MeshFilter;
    private BoxCollider2D Collider;

    private List<Vector3> Vertices = new List<Vector3>();
    private List<int> Triangles = new List<int>();


    void Start()
    {
        _MeshFilter = GetComponent<MeshFilter>();
        Collider = GetComponent<BoxCollider2D>();
        SetHeights();

        StartCoroutine(UpdateAll());
    }

    void Update()
    {
        Draw();

    }
    private IEnumerator UpdateAll()
    {
        while (true)
        {
            MarchSquares();
            CreateMesh();
            CreateGrid();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SetHeights()
    {
        heights = new float[size + 1, size + 1];

        for (int i = 0; i <= size; ++i)
        {
            for (int j = 0; j <= size; ++j)
            {
                if (!useDefaultHeight)
                    heights[i, j] = Mathf.PerlinNoise(i * noiseResolution, j * noiseResolution);

                else
                    heights[i, j] = defaultHeight;
            }
        }

    }

    private void CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = Vertices.ToArray();
        mesh.triangles = Triangles.ToArray();
        mesh.RecalculateNormals();

        List<Vector2> verticesAsVector2 = new List<Vector2>();
        for (int i = 0; i < Vertices.Count; ++i)
        {
            verticesAsVector2.Add(Vertices[i]);
        }

        _MeshFilter.mesh = mesh;
        Collider.size = new Vector2(size * resolution, size * resolution);
        Collider.offset = new Vector2((size / 2f) * resolution, (size / 2f) * resolution);
    }

    private void MarchSquares()
    {
        Vertices.Clear();
        Triangles.Clear();
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                float botLeftVertex = heights[i, j];
                float botRightVertex = heights[i + 1, j];
                float topRightVertex = heights[i + 1, j + 1];
                float topLeftVertex = heights[i, j + 1];

                MarchSquare(botLeftVertex, botRightVertex, topRightVertex, topLeftVertex, i, j);
            }
        }
    }

    private void MarchSquare(float botLeft, float botRight, float topRight, float topLeft, float offsetX, float offsetY)
    {
        int value = GetHeight(botLeft) * 8 + GetHeight(botRight) * 4 + GetHeight(topRight) * 2 + GetHeight(topLeft) * 1;

        Vector3[] verticesLocal = new Vector3[6];
        int[] trianglesLocal = new int[6];

        int vertexCount = Vertices.Count;


        Vector3 pointA = new Vector3(0, 0);
        Vector3 pointB = new Vector3(1, 0);
        Vector3 pointC = new Vector3(1, 1);
        Vector3 pointD = new Vector3(0, 1);


        switch (value)
        {
            case 0:
                return;
            case 1:
                verticesLocal = new Vector3[]
                { new Vector3(0, 1f), new Vector3(Lerp(topLeft, topRight, pointD, pointC).x, 1), new Vector3(0, Lerp(topLeft, botLeft, pointD, pointA).y) };

                trianglesLocal = new int[]
                { 0, 1, 2};
                break;
            case 2:
                verticesLocal = new Vector3[]
                { new Vector3(1, 1), new Vector3(1, Lerp(topRight, botRight, pointC, pointB).y), new Vector3(Lerp(topRight, topLeft, pointC, pointD).x, 1) };

                trianglesLocal = new int[]
                { 0, 1, 2};
                break;
            case 3:
                verticesLocal = new Vector3[]
                { new Vector3(0, Lerp(topLeft, botLeft, pointD, pointA).y), new Vector3(0, 1), new Vector3(1, 1), new Vector3(1, Lerp(topRight, botRight, pointC, pointB).y) };

                trianglesLocal = new int[]
                { 0, 1, 2, 0, 2, 3};
                break;
            case 4:
                verticesLocal = new Vector3[]
                { new Vector3(1, 0), new Vector3(Lerp(botRight, botLeft, pointB, pointA).x, 0), new Vector3(1, Lerp(botRight, topRight, pointB, pointC).y) };

                trianglesLocal = new int[]
                { 0, 1, 2};
                break;
            case 5:
                verticesLocal = new Vector3[]
                { new Vector3(0, Lerp(topLeft, botLeft, pointD, pointA).y), new Vector3(0, 1), new Vector3(Lerp(topLeft, topRight, pointD, pointC).x, 1), new Vector3(1, 0), new Vector3(Lerp(botRight, botLeft, pointB, pointA).x, 0), new Vector3(1, Lerp(botRight, topRight, pointB, pointC).y) };

                trianglesLocal = new int[]
                { 0, 1, 2, 3, 4, 5};
                break;
            case 6:
                verticesLocal = new Vector3[]
                { new Vector3(Lerp(botRight, botLeft, pointB, pointA).x, 0), new Vector3(Lerp(topRight, topLeft, pointC, pointD).x, 1), new Vector3(1, 1), new Vector3(1, 0) };

                trianglesLocal = new int[]
                { 0, 1, 2, 0, 2, 3};
                break;
            case 7:
                verticesLocal = new Vector3[]
                { new Vector3(0, 1), new Vector3(1, 1), new Vector3(1, 0), new Vector3(Lerp(botRight, botLeft, pointB, pointA).x, 0), new Vector3(0, Lerp(topLeft, botLeft, pointD, pointA).y) };

                trianglesLocal = new int[]
                { 2, 3, 1, 3, 4, 1, 4, 0, 1};
                break;
            case 8:
                verticesLocal = new Vector3[]
                { new Vector3(0, Lerp(botLeft, topLeft, pointA, pointD).y), new Vector3(0, 0), new Vector3(Lerp(botLeft, botRight, pointA, pointB).x, 0) };

                trianglesLocal = new int[]
                { 2, 1, 0};
                break;
            case 9:
                verticesLocal = new Vector3[]
                { new Vector3(0, 0), new Vector3(Lerp(botLeft, botRight, pointA, pointB).x, 0), new Vector3(Lerp(topLeft, topRight, pointD, pointC).x, 1), new Vector3(0, 1) };

                trianglesLocal = new int[]
                { 1, 0, 2, 0, 3, 2};
                break;
            case 10:
                verticesLocal = new Vector3[]
                { new Vector3(0, 0), new Vector3(0, Lerp(botLeft, topLeft, pointA, pointD).y), new Vector3(Lerp(botLeft, botRight, pointA, pointB).x, 0), new Vector3(1, 1), new Vector3(Lerp(topRight, topLeft, pointC, pointD).x, 1), new Vector3(1, Lerp(topRight, botRight, pointC, pointB).y) };

                trianglesLocal = new int[]
                { 0, 1, 2, 5, 4, 3};
                break;
            case 11:
                verticesLocal = new Vector3[]
                { new Vector3(0, 0), new Vector3(0, 1), new Vector3(1, 1), new Vector3(1, Lerp(topRight, botRight, pointC, pointB).y), new Vector3(Lerp(botLeft, botRight, pointA, pointB).x, 0) };

                trianglesLocal = new int[]
                { 0, 1, 2, 0, 2, 3, 4, 0, 3};
                break;
            case 12:
                verticesLocal = new Vector3[]
                { new Vector3(0, 0), new Vector3(1, 0), new Vector3(1, Lerp(botRight, topRight, pointB, pointC).y), new Vector3(0, Lerp(botLeft, topLeft, pointA, pointD).y) };

                trianglesLocal = new int[]
                { 0, 3, 2, 0, 2, 1};
                break;
            case 13:
                verticesLocal = new Vector3[]
                { new Vector3(0, 0), new Vector3(0, 1), new Vector3(Lerp(topLeft, topRight, pointD, pointC).x, 1), new Vector3(1, Lerp(botRight, topRight, pointB, pointC).y), new Vector3(1, 0) };

                trianglesLocal = new int[]
                { 0, 1, 2, 0, 2, 3, 0, 3, 4};
                break;
            case 14:
                verticesLocal = new Vector3[]
                { new Vector3(1, 1), new Vector3(1, 0), new Vector3(0, 0), new Vector3(0, Lerp(botLeft, topLeft, pointA, pointD).y), new Vector3(Lerp(topRight, topLeft, pointC, pointD).x, 1) };

                trianglesLocal = new int[]
                { 0, 1, 4, 1, 3, 4, 1, 2, 3};
                break;
            case 15:
                verticesLocal = new Vector3[]
                { new Vector3(0, 0), new Vector3(0, 1), new Vector3(1, 1), new Vector3(1, 0) };

                trianglesLocal = new int[]
                { 0, 1, 2, 0, 2, 3};
                break;


        }

        foreach (Vector3 vert in verticesLocal)
        {
            Vector3 newVert = new Vector3((vert.x + offsetX) * resolution, (vert.y + offsetY) * resolution, 0);
            Vertices.Add(newVert);
        }

        foreach (int triangle in trianglesLocal)
        {
            Triangles.Add(triangle + vertexCount);
        }
    }

    private int GetHeight(float value)//If the Perlin value is lower than the height threshold
    {
        return value < heightTreshold ? 0 : 1;
    }

    private void CreateGrid()
    {
        foreach (Transform child in circleParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i <= size; ++i)
        {
            for (int j = 0; j <= size; ++j)
            {
                Vector2 pos = transform.TransformPoint(new Vector2(i * resolution, j * resolution));
                Transform newCirlce = Instantiate(circlePrefab, pos, new Quaternion(), circleParent);
                newCirlce.localScale = Vector2.one * resolution / 2;
                newCirlce.GetComponent<SpriteRenderer>().color = new Color(heights[i, j], heights[i, j], heights[i, j], 1f);
            }
        }
    }

    private Vector3 Lerp(float edgeStart, float edgeEnd, Vector3 edgeStartVector, Vector3 edgeEndVector)
    {
        return Vector3.Lerp(edgeStartVector, edgeEndVector, (heightTreshold - edgeStart) / (edgeEnd - edgeStart));
    }

    public Rigidbody2D playerRb;

    private void Draw()
    {

        Vector3 playerPos = new Vector3(playerRb.position.x, playerRb.position.y, 0) - transform.position;
        float finalDrawPower = 0f;
        float drawPower = 3f;

        //Negative to lower the values
        finalDrawPower = -1f * drawPower;

        //Divide by resolution to match correct mesh coordinates
        for (float i = (playerPos.x - drawRange) / resolution; i < (playerPos.x + drawRange) / resolution; ++i)
        {


            for (float j = (playerPos.y - drawRange) / resolution; j < (playerPos.y + drawRange)/resolution; ++j)
            {

                int x = Mathf.FloorToInt(i);
                int y = Mathf.FloorToInt(j);
                if (x >= 0 && x <= size && y >= 0 && y <= size)
                {
                    if (resolution < 1)
                        Debug.Log("i = " + i);
                    float distanceToMouse = Vector2.Distance(new Vector2(i * resolution, j * resolution), new Vector2(playerPos.x, playerPos.y));
                    //Clamp between 0 and 1 in fuction of time, drawPower and distance to mouse
                    heights[x, y] = Mathf.Clamp(heights[x, y] + Time.deltaTime * finalDrawPower * Mathf.Clamp(drawRange - distanceToMouse, 0, 1000), 0, 1);
                }

            }
        }
        
    }
}
