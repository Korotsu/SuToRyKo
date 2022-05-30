using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseEntity : Base
{
    protected int HP = 0;
    protected Action OnHpUpdated;
    protected GameObject SelectedSprite = null;
    protected Text HPText = null;
    protected bool IsInitialized = false;
    
    Mesh entityVisMesh;
    MeshRenderer entityvisMeshRend;
    [HideInInspector]
    public GameObject entityVisObj;
    public Material VisMat;
    public float VisionMax = 50f;
    public Action OnDeadEvent;
    public bool IsSelected { get; protected set; }
    public bool IsAlive { get; protected set; }
    virtual public void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        Team = _team;

        IsInitialized = true;
    }
    public Color GetColor()
    {
        return GameServices.GetTeamColor(GetTeam());
    }
    
    void UpdateHpUI()
    {
        if (HPText != null)
            HPText.text = "HP : " + HP.ToString();
    }

   

    #region MonoBehaviour methods
    virtual protected void Awake()
    {
        IsAlive = true;

        SelectedSprite = transform.Find("SelectedSprite")?.gameObject;
        SelectedSprite?.SetActive(false);

        Transform hpTransform = transform.Find("Canvas/HPText");
        if (hpTransform)
            HPText = hpTransform.GetComponent<Text>();

        OnHpUpdated += UpdateHpUI;
        entityVisObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        entityVisObj.transform.SetParent(transform, false);
        entityVisObj.transform.localScale = new Vector3(VisionMax, VisionMax,VisionMax);
        entityVisMesh = entityVisObj.GetComponent<MeshFilter>().mesh;
        entityvisMeshRend = entityVisObj.GetComponent<MeshRenderer>();
        entityvisMeshRend.material = VisMat;
        entityVisObj.layer = LayerMask.NameToLayer("UnitView");
    }
    virtual protected void Start()
    {
        UpdateHpUI();
    }
    virtual protected void Update()
    {
    }
    #endregion
}
