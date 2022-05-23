using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    
    private Mesh m_mesh;

    private Vector3[] m_vert;
    private Color[] m_color;
    public LayerMask m_fogLayer;
    
    [SerializeField]
    private float m_rad_Sqr = 5;
    // Use this for initialization
    void Start () {
        Initialize();
    }
	
    // Update is called once per frame
    void Update () {
        /*Ray r = new Ray(camTransform.position, m_player.position - camTransform.position);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, 1000, m_fogLayer, QueryTriggerInteraction.Collide)) {
            for (int i=0; i< m_vertices.Length; i++) {
                Vector3 v = m_fogOfWarPlane.transform.TransformPoint(m_vertices[i]);
                float dist = Vector3.SqrMagnitude(v - hit.point);
                if (dist < m_radiusSqr) {
                    float alpha = Mathf.Min(m_colors[i].a, dist/m_radiusSqr);
                    m_colors[i].a = alpha;
                }
            }
            UpdateColor();
        }*/
    }

    public void UpdatePoint(Vector3 pos)
    {
        Vector3 pos2 = Camera.main.transform.position;
        Vector3 dir = (pos - pos2).normalized;
        Ray r = new Ray(pos2, dir);
        Debug.DrawLine(pos2, dir *1000, Color.red, 1, false );
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, 1000, m_fogLayer, QueryTriggerInteraction.Collide)) 
        {
            for (int i=0; i< m_vert.Length; i++) 
            {
                Vector3 v = transform.TransformPoint(m_vert[i]);
                float dist = Vector3.SqrMagnitude(v - hit.point);
                if (dist < m_rad_Sqr)
                {
                    float alpha = Mathf.Min(m_color[i].a, dist/m_rad_Sqr);
                    m_color[i].a = alpha;
                }
            }
            UpdateColor();
        }
    }

    public void ResetColor()
    {
        for (int i=0; i < m_color.Length; i++) {
            m_color[i] = Color.black;
        } 
    }
	
    void Initialize() {
        m_mesh = gameObject.GetComponent<MeshFilter>().mesh;
        m_vert = m_mesh.vertices;
        m_color = new Color[m_vert.Length];
        
        ResetColor();
        UpdateColor();
    }
	
    void UpdateColor() {
        m_mesh.colors = m_color;
    }
}
