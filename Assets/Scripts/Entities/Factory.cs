using System;
using System.Collections.Generic;
using Entities;
using UnityEngine;
using UnityEngine.UI;



public class UnitBuildOrder
{
    public Tactician t = null;
    public int id = 0;

}
public sealed class Factory : InteractableEntity
{
    [SerializeField]
    FactoryDataScriptable FactoryData = null;

    GameObject[] UnitPrefabs = null;
    GameObject[] FactoryPrefabs = null;
    UnitBuildOrder RequestedEntityBuildIndex =  new UnitBuildOrder{id=-1};
    Image BuildGaugeImage;
    float CurrentBuildDuration = 0f;
    float EndBuildDate = 0f;
    int SpawnCount = 0;
    /* !! max available unit count in menu is set to 9, available factories count to 3 !! */
    const int MaxAvailableUnits = 9;
    const int MaxAvailableFactories = 3;

    UnitController Controller = null;

    [SerializeField]
    int MaxBuildingQueueSize = 5;
    Queue<UnitBuildOrder> BuildingQueue = new Queue<UnitBuildOrder>();
    public enum State
    {
        Available = 0,
        UnderConstruction,
        BuildingUnit,
    }
    public State CurrentState { get; private set; }
    public bool IsUnderConstruction { get { return CurrentState == State.UnderConstruction; } }
    public int Cost { get { return FactoryData.Cost; } }
    public FactoryDataScriptable GetFactoryData { get { return FactoryData; } }
    public int AvailableUnitsCount { get { return Mathf.Min(MaxAvailableUnits, FactoryData.AvailableUnits.Length); } }
    public int AvailableFactoriesCount { get { return Mathf.Min(MaxAvailableFactories, FactoryData.AvailableFactories.Length); } }
    public Action<Unit, Tactician> OnUnitBuilt;
    public Action<Factory> OnFactoryBuilt;
    public Action OnBuildCanceled;
    public bool IsBuildingUnit { get { return CurrentState == State.BuildingUnit; } }

    
    protected override float GetInfluence()
    {
        float p = Cost * 10;
        p += Cost * (1 - (HP / FactoryData.MaxHP));
        return p;
    }

    #region MonoBehaviour methods
    protected override void Awake()
    {
        base.Awake();

        BuildGaugeImage = transform.Find("Canvas/BuildProgressImage").GetComponent<Image>();
        if (BuildGaugeImage)
        {
            BuildGaugeImage.fillAmount = 0f;
            BuildGaugeImage.color = GameServices.GetTeamColor(GetTeam());
        }

        if (FactoryData == null)
        {
            Debug.LogWarning("Missing FactoryData in " + gameObject.name);
        }
        HP = FactoryData.MaxHP;
        OnDeadEvent += Factory_OnDead;

        UnitPrefabs = new GameObject[FactoryData.AvailableUnits.Length];
        FactoryPrefabs = new GameObject[FactoryData.AvailableFactories.Length];

        // Load from resources actual Unit prefabs from template data
        for (int i = 0; i < FactoryData.AvailableUnits.Length; i++)
        {
            GameObject templateUnitPrefab = FactoryData.AvailableUnits[i];
            string path = "Prefabs/Units/" + templateUnitPrefab.name + "_" + Team.ToString();
            UnitPrefabs[i] = Resources.Load<GameObject>(path);
            if (UnitPrefabs[i] == null)
                Debug.LogWarning("could not find Unit Prefab at " + path);
        }

        // Load from resources actual Factory prefabs from template data
        for (int i = 0; i < FactoryData.AvailableFactories.Length; i++)
        {
            GameObject templateFactoryPrefab = FactoryData.AvailableFactories[i];
            string path = "Prefabs/Factories/" + templateFactoryPrefab.name + "_" + Team.ToString();
            FactoryPrefabs[i] = Resources.Load<GameObject>(path);
        }
        
        
        
    }
    protected override void Start()
    {
        base.Start();
        GameServices.GetGameState().IncreaseTeamScore(Team);
        Controller = GameServices.GetControllerByTeam(Team);
    }
    override protected void Update()
    {
        switch (CurrentState)
        {
            case State.Available:
                break;

            case State.UnderConstruction:
                // $$$ TODO : improve construction progress rendering
                if (Time.time > EndBuildDate)
                {
                    CurrentState = State.Available;
                    BuildGaugeImage.fillAmount = 0f;
                }
                else if (BuildGaugeImage)
                    BuildGaugeImage.fillAmount = 1f - (EndBuildDate - Time.time) / FactoryData.BuildDuration;
                break;

            case State.BuildingUnit:
                if (Time.time > EndBuildDate)
                {
                    OnUnitBuilt?.Invoke(BuildUnit(), RequestedEntityBuildIndex.t);
                    OnUnitBuilt = null; // remove registered methods
                    CurrentState = State.Available;

                    // manage build queue : chain with new unit build if necessary
                    if (BuildingQueue.Count != 0)
                    {
                        UnitBuildOrder unitIndex = BuildingQueue.Dequeue();
                        StartBuildUnit(unitIndex.id, unitIndex.t, true);
                    }
                }
                else if (BuildGaugeImage)
                    BuildGaugeImage.fillAmount = 1f - (EndBuildDate - Time.time) / CurrentBuildDuration;
                break;
        }
    }
    #endregion
    void Factory_OnDead()
    {
        if (FactoryData.DeathFXPrefab)
        {
            GameObject fx = Instantiate(FactoryData.DeathFXPrefab, transform);
            fx.transform.parent = null;
        }

        GameServices.GetGameState().DecreaseTeamScore(Team);
        Destroy(gameObject);
    }
    #region IRepairable
    override public bool NeedsRepairing()
    {
        return HP < GetFactoryData.MaxHP;
    }
    override public void Repair(int amount)
    {
        HP = Mathf.Min(HP + amount, GetFactoryData.MaxHP);
        base.Repair(amount);
    }
    override public void FullRepair()
    {
        Repair(GetFactoryData.MaxHP);
    }
    #endregion

    #region Unit building methods
    bool IsUnitIndexValid(int unitIndex)
    {
        if (unitIndex < 0 || unitIndex >= UnitPrefabs.Length)
        {
            Debug.LogWarning("Wrong unitIndex " + unitIndex);
            return false;
        }
        return true;
    }
    public UnitDataScriptable GetBuildableUnitData(int unitIndex)
    {
        if (IsUnitIndexValid(unitIndex) == false)
            return null;

        return UnitPrefabs[unitIndex].GetComponent<Unit>().GetUnitData;
    }

    public float GetBuildableUnitInfluence(int unitIndex)
    {
        if (IsUnitIndexValid(unitIndex) == false)
            return -1;

        return UnitPrefabs[unitIndex].GetComponent<Unit>().GetUnitData.GetInfluence();
    }
    public int GetUnitCost(int unitIndex)
    {
        UnitDataScriptable data = GetBuildableUnitData(unitIndex);
        if (data)
            return data.Cost;

        return 0;
    }
    public int GetQueuedCount(int unitIndex)
    {
        int counter = 0;
        foreach(UnitBuildOrder id in BuildingQueue)
        {
            if (id.id == unitIndex)
                counter++;
        }
        return counter;
    }
    public bool RequestUnitBuild(int unitMenuIndex, Tactician tactician = null)
    {
        int cost = GetUnitCost(unitMenuIndex);
        if (Controller.TotalBuildPoints < cost || BuildingQueue.Count >= MaxBuildingQueueSize)
            return false;

        Controller.TotalBuildPoints -= cost;

        StartBuildUnit(unitMenuIndex, tactician);

        return true;
    }
    void StartBuildUnit(int unitMenuIndex, Tactician tactician = null, bool deque = false)
    {
        if (IsUnitIndexValid(unitMenuIndex) == false)
            return;

        // Factory is being constucted
        if (CurrentState == State.UnderConstruction)
            return;

        
        // Build queue
        if (CurrentState == State.BuildingUnit)
        {
            if (BuildingQueue.Count < MaxBuildingQueueSize)
            {
                BuildingQueue.Enqueue(new UnitBuildOrder{id=unitMenuIndex, t= tactician});
                if (tactician)
                {
                    switch (FactoryData.type)
                    {
                        case EntityDataScriptable.Type.Light:
                            tactician.nbLightInCreation++;
                            break;
                        case EntityDataScriptable.Type.Heavy:
                            tactician.nbHeavyInCreation++;
                            break;
                        default:
                            break;
                    }
                }
            }
            
            return;
        }
        else if (!deque)
        {
            if (tactician)
            {
                switch (FactoryData.type)
                {
                    case EntityDataScriptable.Type.Light:
                        tactician.nbLightInCreation++;
                        break;
                    case EntityDataScriptable.Type.Heavy:
                        tactician.nbHeavyInCreation++;
                        break;
                    default:
                        break;
                }
            }
        }
        
        CurrentBuildDuration = GetBuildableUnitData(unitMenuIndex).BuildDuration;
        //Debug.Log("currentBuildDuration " + CurrentBuildDuration);

        CurrentState = State.BuildingUnit;
        EndBuildDate = Time.time + CurrentBuildDuration;

        RequestedEntityBuildIndex.id = unitMenuIndex;
        RequestedEntityBuildIndex.t = tactician;

        OnUnitBuilt += (Unit unit, Tactician bo) =>
        {
            if (unit != null)
            {
                Controller.AddUnit(unit);
                (Controller as PlayerController)?.UpdateFactoryBuildQueueUI(RequestedEntityBuildIndex.id);

                if (bo)
                {
                    bo.AddSoldier(unit);
                    unit.tempTactician = bo;

                    switch (FactoryData.type)
                    {
                        case EntityDataScriptable.Type.Light:
                            bo.nbLightInCreation--;
                            bo.nbLight++;
                            break;
                        case EntityDataScriptable.Type.Heavy:
                            bo.nbHeavyInCreation--;
                            bo.nbHeavy++;
                            break;
                        default:
                            break;
                    }
                }
            }
        };
    }

    // Finally spawn requested unit
    Unit BuildUnit()
    {
        if (IsUnitIndexValid(RequestedEntityBuildIndex.id) == false)
            return null;

        CurrentState = State.Available;

        GameObject unitPrefab = UnitPrefabs[RequestedEntityBuildIndex.id];

        if (BuildGaugeImage)
            BuildGaugeImage.fillAmount = 0f;

        int slotIndex = SpawnCount % FactoryData.NbSpawnSlots;
        // compute simple spawn position around the factory
        float angle = 2f * Mathf.PI / FactoryData.NbSpawnSlots * slotIndex;
        int offsetIndex = Mathf.FloorToInt(SpawnCount / FactoryData.NbSpawnSlots);
        float radius = FactoryData.SpawnRadius /*+ offsetIndex * FactoryData.RadiusOffset*/;
        Vector3 spawnPos = transform.position + new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle));

        // !! Flying units require a specific layer to be spawned on !!
        bool isFlyingUnit = unitPrefab.GetComponent<Unit>().GetUnitData.IsFlying;
        int layer = isFlyingUnit ? LayerMask.NameToLayer("FlyingZone") : LayerMask.NameToLayer("Floor");

        // cast position on ground
        RaycastHit raycastInfo;
        Ray ray = new Ray(spawnPos, Vector3.down);
        if (Physics.Raycast(ray, out raycastInfo, 10f, 1 << layer))
            spawnPos = raycastInfo.point;

        Transform teamRoot = GameServices.GetControllerByTeam(GetTeam())?.GetTeamRoot();
        GameObject unitInst = Instantiate(unitPrefab, spawnPos, Quaternion.identity, teamRoot);
        unitInst.name = unitInst.name.Replace("(Clone)", "_" + SpawnCount.ToString());
        Unit newUnit = unitInst.GetComponent<Unit>();
        newUnit.Init(GetTeam());

        SpawnCount++;

        // disable build cancelling callback
        OnBuildCanceled = null;

        return newUnit;
    }
    public void CancelCurrentBuild()
    {
        if (CurrentState == State.UnderConstruction || CurrentState == State.Available)
            return;

        CurrentState = State.Available;

        // refund build points
        Controller.TotalBuildPoints += GetUnitCost(RequestedEntityBuildIndex.id);
        foreach(UnitBuildOrder unitIndex in BuildingQueue)
        {
            Controller.TotalBuildPoints += GetUnitCost(unitIndex.id);
        }
        BuildingQueue.Clear();

        BuildGaugeImage.fillAmount = 0f;
        CurrentBuildDuration = 0f;
        RequestedEntityBuildIndex.id = -1;

        OnBuildCanceled?.Invoke();
        OnBuildCanceled = null;
    }
    #endregion

    #region Factory building methods
    public GameObject GetFactoryPrefab(int factoryIndex)
    {
        return IsFactoryIndexValid(factoryIndex) ? FactoryPrefabs[factoryIndex] : null;
    }
    bool IsFactoryIndexValid(int factoryIndex)
    {
        if (factoryIndex < 0 || factoryIndex >= FactoryPrefabs.Length)
        {
            Debug.LogWarning("Wrong factoryIndex " + factoryIndex);
            return false;
        }
        return true;
    }
    public FactoryDataScriptable GetBuildableFactoryData(int factoryIndex)
    {
        if (IsFactoryIndexValid(factoryIndex) == false)
            return null;

        return FactoryPrefabs[factoryIndex].GetComponent<Factory>().GetFactoryData;
    }
    public int GetFactoryCost(int factoryIndex)
    {
        FactoryDataScriptable data = GetBuildableFactoryData(factoryIndex);
        if (data)
            return data.Cost;

        return 0;
    }
    public bool CanPositionFactory(int factoryIndex, Vector3 buildPos)
    {
        if (IsFactoryIndexValid(factoryIndex) == false)
            return false;

        if (GameServices.IsPosInPlayableBounds(buildPos) == false)
            return false;

        GameObject factoryPrefab = FactoryPrefabs[factoryIndex];

        Vector3 extent = factoryPrefab.GetComponent<BoxCollider>().size / 2f;

        float overlapYOffset = 0.1f;
        buildPos += Vector3.up * (extent.y + overlapYOffset);

        if (Physics.CheckBox(buildPos, extent, Quaternion.identity, 
                LayerMask.GetMask("Floor") | LayerMask.GetMask("Factory") | LayerMask.GetMask("Target") ))
        {
            return false;
        }

        return true;
    }
    public Factory StartBuildFactory(int factoryIndex, Vector3 buildPos)
    {
        if (IsFactoryIndexValid(factoryIndex) == false)
            return null;

        if (CurrentState == State.BuildingUnit)
            return null;

        GameObject factoryPrefab = FactoryPrefabs[factoryIndex];
        Transform teamRoot = GameServices.GetControllerByTeam(GetTeam())?.GetTeamRoot();
        GameObject factoryInst = Instantiate(factoryPrefab, buildPos, Quaternion.identity, teamRoot);
        factoryInst.name = factoryInst.name.Replace("(Clone)", "_" + SpawnCount.ToString());
        Factory newFactory = factoryInst.GetComponent<Factory>();
        newFactory.Init(GetTeam());
        newFactory.StartSelfConstruction();

        return newFactory;
    }
    void StartSelfConstruction()
    {
        CurrentState = State.UnderConstruction;

        EndBuildDate = Time.time + FactoryData.BuildDuration;
    }

    #endregion
}
