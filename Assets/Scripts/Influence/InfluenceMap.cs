using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public struct InfluenceData
{
    public float buildingWeight;
    public float unitWeight;
    public float weight => unitWeight + buildingWeight;
}


[Serializable]
public class CellData
{
    public Vector3 center;
    public Vector3 size;
    public Bounds b;
    public InfluenceData[] teamsWeights;

    public CellData()
    {
        teamsWeights = new InfluenceData[(int)ETeam.Count];
        
    }

    public CellData(CellData d)
    {
        center = d.center;
        size = d.size;
        b = d.b;
        teamsWeights = new InfluenceData[(int)ETeam.Count];
    }
    
    public void Reset()
    {
        for (int i = 0; i < teamsWeights.Length; i++)
        {
            teamsWeights[i].buildingWeight = 0;
            teamsWeights[i].unitWeight = 0;
        }
    }

    public void CalcBounds()
    {
        b = new Bounds(center, size);
    }
}

public class cellID
{
    public int x;
    public int y;
}


public class InfluenceMap : MonoBehaviour
{
    private int updateCount = 0;
    private bool isInitialized = false;
    [SerializeField, MinAttribute(1)]
    private int subdivision = 1 ;
    private float Radius = 0f;
    private Vector3 center;
    private float InfluenceCellSize;
    private CellData[,] map;
    
    [SerializeField, MinAttribute(0)]
    private int MaxVisAdd =10;

    private int MaxVisAddCell => (int)(MaxVisAdd * InfluenceCellSize);
    [SerializeField, MinAttribute(0.0001f)] private float Exponent;

    [SerializeField] private bool GizmoBuilding;
    [SerializeField] private bool GizmoUnit;
    //[SerializeField] private bool Bench;
    private Vector3 CornerCalc;
    private Vector3 mapCenter;
    
    void Awake()
    {
        isInitialized = false;
        Init();
    }

    void ApplyLayer(ModifierMap layer)
    {
        
    }

    cellID FindCellAtPos(Vector3 pos)
    {
        cellID id = new cellID();
        //if (Bench)
        {
            CornerCalc.x = pos.x- mapCenter.x +Radius;
            CornerCalc.z = pos.z- mapCenter.z +Radius;
            
            for (int x = (int)(CornerCalc.x/InfluenceCellSize); x < subdivision; x++)
            {
                for (int y = (int)(CornerCalc.z/InfluenceCellSize); y < subdivision; y++)
                {
                    CellData cellData = map[x, y];
                    //Bounds b = new Bounds(cellData.center, cellData.size);
                    if (cellData.b.Contains(pos))
                    {
                        id.x = x;
                        id.y = y;
                        return id;
                    }
                }
            }
        }
        /*else
        {
            
            for (int x = 0; x < subdivision; x++)
            {
                for (int y = 0; y < subdivision; y++)
                {
                    CellData cellData = map[x, y];
                    //Bounds b = new Bounds(cellData.center, cellData.size);
                    if (cellData.b.Contains(pos))
                    {
                        id.x = x;
                        id.y = y;
                        return id;
                    }
                }
            }
        }*/
        

        return id;
    }

    void Spread(cellID id, int teamId, float weight, bool isBuilding, int sizeBeginFalloff = 10, int maxCircleSize = 30)
    {
        if (maxCircleSize < sizeBeginFalloff)
            maxCircleSize = (int)(sizeBeginFalloff * 1.5f);
        int c = maxCircleSize;
        int d = sizeBeginFalloff;
        c = (int)(c / InfluenceCellSize);
        d = (int)(d / InfluenceCellSize);
        float c2 = c * c;
        float d2 = d * d;
        for(int y=-c; y<=c; y++)
            for(int x=-c; x<=c; x++)
                if (x * x + y * y <= c2)
                {
                    bool f = id.x+x < 0 || id.x+x >= subdivision || id.y+y < 0 || id.y+y >= subdivision;
                    if (f) continue;
                    int car = (x * x + y * y);
                    float distance =  car/ d2 ;
                    if (distance < 1)
                    {
                        distance = 0;
                    }
                    else
                    {
                        distance = car /c2;
                    }
                    float e = Mathf.Exp(Exponent - Exponent / distance);
                    float w=weight -  (weight*e) ;
                    if(isBuilding)
                        map[id.x+x, id.y+y].teamsWeights[teamId].buildingWeight += w >= 0 ? w : 0;
                    else
                        map[id.x+x, id.y+y].teamsWeights[teamId].unitWeight += w >= 0 ? w : 0;
                }
    }

    void CleanMap()
    {
        for (int x = 0; x < subdivision; x++)
        {
            for (int y = 0; y < subdivision; y++)
            {
                map[x, y].Reset();
            }
        }

    }

   
    
    public void Init()
    {
        map = new CellData[subdivision, subdivision];

        center = transform.position;
        int count = -1;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.position.x >= center.x && (int)child.position.z == (int)center.z)
                count += 2;
        }

        MeshFilter mf = null;
        Terrain ter = null;
        float localScale = 1;
         mapCenter = new Vector3();
        if (count == -1)
        {
             TryGetComponent(out mf);
             TryGetComponent(out ter);
             localScale = transform.localScale.x;
             count = 1;
        }
        else
        {
            transform.GetChild(0).TryGetComponent(out mf);
            transform.GetChild(0).TryGetComponent(out ter);
            localScale = transform.GetChild(0).localScale.x;

        }

        if (!(mf is null))
        {
            var sharedMesh = mf.sharedMesh;
            Radius = sharedMesh.bounds.extents.x * localScale* count;
            mapCenter = sharedMesh.bounds.center;
        }

        if (!(ter is null))
        {
            var terrainData = ter.terrainData;
            Radius = terrainData.bounds.extents.x * localScale * count;
            mapCenter = terrainData.bounds.center;
        }
        
        InfluenceCellSize = (Radius*2) / subdivision;
        Vector3 corner = new Vector3(mapCenter.x-Radius, 0, mapCenter.z-Radius);
        float InfluenceCellCenter =  InfluenceCellSize/ 2.0f;
        for (int x = 0; x < subdivision; x++)
        {
            for (int y = 0; y < subdivision; y++)
            {
                Vector3 centertoDraw = corner + new Vector3(InfluenceCellCenter, 0, InfluenceCellCenter);
                Vector3 size = new Vector3(InfluenceCellSize, 10, InfluenceCellSize);
                map[x, y] = new CellData
                {
                    center = centertoDraw,
                    size = size
                };
                map[x, y].CalcBounds();
                corner += new Vector3(0, 0 , InfluenceCellSize);
            }
            corner += new Vector3(InfluenceCellSize, 0 , 0);
            corner.z = mapCenter.z-Radius;
        }
       
        
        isInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        updateCount++;
        if (updateCount < 20)
            return;
        updateCount = 0;
        CleanMap();
        GameObject[] units =GameObject.FindGameObjectsWithTag("Unit");
        GameObject[] buildings =GameObject.FindGameObjectsWithTag("Building");
        foreach (GameObject unit in units)
        {
            BaseEntity data = unit.GetComponent<BaseEntity>();
            Vector3 position = unit.transform.position;
            cellID id = FindCellAtPos(position);
            int team = (int)data.GetTeam();
            Spread(id,team, data.Influence, false,(int)(data.VisionMax/2.0f),(int)((data.VisionMax)/2.0f)+MaxVisAddCell);
            
        }
        foreach (GameObject building in buildings)
        {
            BaseEntity data = building.GetComponent<BaseEntity>();
            Vector3 position = building.transform.position;
            cellID id = FindCellAtPos(position);
            int team = (int)data.GetTeam();
            Spread(id,team, data.Influence, true, (int)(data.VisionMax/2.0f),(int)((data.VisionMax)/2.0f)+MaxVisAddCell );
            
        }
    }

    private void OnDrawGizmos()
    {
        if(!isInitialized)
            return;
        Vector3 corner = new Vector3(-Radius, 0, -Radius);
        float InfluenceCellCenter =  InfluenceCellSize/ 2.0f;
        for (int x = 0; x < subdivision; x++)
        {
            for (int y = 0; y < subdivision; y++)
            {
                CellData tileData = map[x, y];
                if(tileData.teamsWeights == null)
                    Gizmos.color = Color.white;
                else
                {
                    Vector4 color = new Vector4();
                    for (int i = 0; i < (int)ETeam.Count; i++)
                    {
                        float weight = 0;
                        if(GizmoBuilding && GizmoUnit)
                         weight =tileData.teamsWeights[i].weight;
                        else if (GizmoUnit)
                            weight = tileData.teamsWeights[i].unitWeight;
                        else if(GizmoBuilding)
                            weight = tileData.teamsWeights[i].buildingWeight;
                        if (i == (int)ETeam.Blue)
                        {
                            Vector4 tocolor = Color.blue;
                            color += tocolor*weight;
                        }
                        if (i == (int)ETeam.Red)
                        {
                            Vector4 tocolor = Color.red;
                            color += tocolor*weight;
                        }
                        if (i == (int)ETeam.Neutral)
                        {
                            Vector4 tocolor = Color.white;
                            color += tocolor*weight;
                        }
                    }

                    float w = color.w;
                    color.w = 0;
                    color.Normalize();
                    color.w = w <= 1.0f ? w : 1;
                    Gizmos.color = color;
                    
                }
                Gizmos.DrawCube(tileData.center, tileData.size);
                
                corner += new Vector3(0, 0 , InfluenceCellSize);
            }
            corner += new Vector3(InfluenceCellSize, 0 , 0);
            corner.z = -Radius;
        }
    }
}
