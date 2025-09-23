using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionConeMesh : MonoBehaviour
{
    public Transform offset;
    public float range = 20f;
    public float angle = 90f;
    public int segments = 48;
    public LayerMask obstacleMask = ~0;   // opcional: recorte con paredes

    Mesh _mesh;

    private void Awake() { _mesh = new Mesh { name = "VisionCone" }; GetComponent<MeshFilter>().mesh = _mesh; }

    void LateUpdate()
    {
        if (!offset) return;

        // 1) Hacé que este objeto siga la posición/rotación de los ojos
        transform.SetPositionAndRotation(
            offset.position,
            Quaternion.LookRotation(Vector3.ProjectOnPlane(offset.forward, Vector3.up).normalized, Vector3.up)
        );

        int vCount = segments + 2;
        var verts = new Vector3[vCount];
        var tris = new int[segments * 3];

        // 2) Centro en local
        verts[0] = Vector3.zero;

        float half = angle * 0.5f;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float yaw = Mathf.Lerp(-half, half, t);

            // 3) Dirección en LOCAL (adelante es Z+)
            Vector3 dirLocal = Quaternion.AngleAxis(yaw, Vector3.up) * Vector3.forward;

            // 4) Para recortar con paredes, hacé el raycast en MUNDO
            Vector3 originW = transform.position;
            Vector3 dirW = transform.TransformDirection(dirLocal);

            Vector3 endW = originW + dirW * range;
            if (Physics.Raycast(originW, dirW, out var hit, range, obstacleMask, QueryTriggerInteraction.Ignore))
                endW = hit.point;

            // 5) Guardá el vértice en LOCAL
            verts[i + 1] = transform.InverseTransformPoint(endW) + Vector3.up * 0.02f; // leve lift opcional
            if (i < segments)
            {
                int tri = i * 3;
                tris[tri + 0] = 0;
                tris[tri + 1] = i + 1;
                tris[tri + 2] = i + 2;
            }
        }

        _mesh.Clear();
        _mesh.vertices = verts;
        _mesh.triangles = tris;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }
}