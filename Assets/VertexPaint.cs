using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
public class VertexPaint : MonoBehaviour
{
    private MeshRenderer renderer;
    private MeshFilter filter;
    private Mesh mesh;
    private Mesh stream;
    private Color[] colors;
    private bool init = false;

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    void setup()
    {
        renderer = GetComponent<MeshRenderer>();
        filter = GetComponent<MeshFilter>();
        mesh = filter.sharedMesh;

        stream = new Mesh();
        stream.MarkDynamic();

        stream.vertices = mesh.vertices;
        colors = new Color[mesh.vertexCount];
        for (var i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }
        stream.triangles = mesh.triangles;
        stream.colors = colors;
        renderer.additionalVertexStreams = stream;

        Debug.Log("Called VertexPaint.setup");
    }


    void OnSceneGUI(SceneView sceneView)
    {
        RaycastHit hit;
        Vector3 mousePosition = Event.current.mousePosition;

        // ScreenPointToRayで期待しているyの位置を補正する。
        mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y;
        var ray = sceneView.camera.ScreenPointToRay(mousePosition);

        if (Event.current.isMouse && Event.current.type == EventType.mouseDown)
        {
            if (!init)
            {
                setup();
                init = true;
            }

            // 現状meshが一個しかないので判定を省きつつ、meshに対してrayを打つ。
            if (RXLookingGlass.IntersectRayMesh(ray, mesh, transform.localToWorldMatrix, out hit))
            {
                // hit.pointはこの場合meshでのローカル座標になる。
                Debug.LogFormat("hit at {0}(local position)", hit.point);
                // あとは頂点を塗るだけ
                paint(hit.point);
            }
        }
    }

    private void paint(Vector3 position)
    {
        var radius = 1.0f;
        for (var i = 0; i < stream.vertexCount; i++)
        {
            var dist = Vector3.Distance(stream.vertices[i], position);
            if (dist < radius)
            {
                colors[i] = Color.blue;
            }
        }

        // colorsはセットしてあげないと動かないみたい。
        stream.colors = colors;
    }

}
