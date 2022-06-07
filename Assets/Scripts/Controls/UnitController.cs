using System;
using System.Collections.Generic;
using UnityEngine;

// points system for units creation (Ex : light units = 1 pt, medium = 2pts, heavy = 3 pts)
// max points can be increased by capturing TargetBuilding entities
public class UnitController : MonoBehaviour
{
    [SerializeField]
    protected ETeam Team;
    public ETeam GetTeam() { return Team; }

    [SerializeField]
    protected int StartingBuildPoints = 15;

    protected int _TotalBuildPoints = 0;
    public int TotalBuildPoints
    {
        get => _TotalBuildPoints;
        set
        {
            _TotalBuildPoints = value;
            OnBuildPointsUpdated?.Invoke();
        }
    }

    protected int _CapturedTargets = 0;
    public int CapturedTargets
    {
        get => _CapturedTargets;
        set
        {
            _CapturedTargets = value;
            OnCaptureTarget?.Invoke();
        }
    }

    protected Transform TeamRoot = null;
    public Transform GetTeamRoot() { return TeamRoot; }

    protected readonly List<Unit> UnitList = new List<Unit>();
    public List<Unit> SelectedUnitList { get; protected set; } = new List<Unit>();
    
    protected readonly List<Factory> FactoryList = new List<Factory>();
    protected Factory SelectedFactory = null;

    [SerializeField]
    protected GameObject tacticianPrefab = null;

    protected List<Tactician> lockedTacticians = new List<Tactician>();
    public Tactician selectedTactician { get; protected set; } = null;

    // events
    protected Action OnBuildPointsUpdated;
    protected Action OnCaptureTarget;

    #region Unit methods
    protected void UnselectAllUnits()
    {
        SelectedUnitList.ForEach(unit => unit.SetSelected(false));
        SelectedUnitList.Clear();

        if (selectedTactician && selectedTactician.GetSoldiers().Count <= 1)
            Destroy(selectedTactician.gameObject);

        selectedTactician = null;
    }
    protected void SelectAllUnits()
    {
        UnselectAllUnits();

        foreach (Unit unit in UnitList)
        {
            if (unit.mainTactician)
                SelectFormation(unit);

            else
                SelectSingleUnit(unit);
        }
    }
    protected void SelectAllUnitsByTypeId(int typeId)
    {
        UnselectCurrentFactory();
        UnselectAllUnits();
        SelectedUnitList = UnitList.FindAll(delegate (Unit unit)
            {
                return unit.GetTypeId == typeId;
            }
        );
        foreach (Unit unit in SelectedUnitList)
        {
            if (unit.mainTactician)
                SelectFormation(unit);

            else
                SelectSingleUnit(unit);
        }
    }
    protected void SelectUnitList(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            if (unit.mainTactician)
                SelectFormation(unit);

            else
                SelectSingleUnit(unit);
        }
    }
    protected void SelectUnitList(Unit[] units)
    {
        foreach (Unit unit in units)
        {
            if (unit.mainTactician)
                SelectFormation(unit);

            else
                SelectSingleUnit(unit);
        }
    }
    protected void SelectSingleUnit(Unit unit)
    {
        if (unit.tempTactician)
            unit.tempTactician.RemoveAndCheck(unit);

        SelectUnit(unit);

        if ((selectedTactician == null && SelectedUnitList.Count > 1) || (selectedTactician && selectedTactician.isFormationLocked))
            CreateTactician(unit.transform.position);

        else if (selectedTactician)
        {
            selectedTactician.Soldiers.Add(unit);
            unit.tempTactician = selectedTactician;
        }
    }

    protected void SelectUnit(Unit unit)
    {
        unit.SetSelected(true);

        if (!SelectedUnitList.Contains(unit))
            SelectedUnitList.Add(unit);
    }

    protected void SelectFormation(Unit unit)
    {
        if (selectedTactician && selectedTactician.isFormationLocked)
            CreateTactician(selectedTactician.transform.position);

        selectedTactician ??= unit.mainTactician;

        if (selectedTactician == unit.mainTactician && unit.tempTactician && unit.tempTactician != unit.mainTactician)
            unit.mainTactician.FormationManager.SwitchFormationType(unit.mainTactician.FormationManager.formationType);

        foreach (Unit soldier in unit.mainTactician.Soldiers)
        {
            if (soldier.tempTactician && soldier.tempTactician != soldier.mainTactician)
                soldier.tempTactician.RemoveAndCheck(soldier);

            SelectUnit(soldier);
            soldier.tempTactician = selectedTactician;
            if (!selectedTactician.Soldiers.Contains(soldier))
            {
                soldier.actions -= soldier.FollowFormation;
                selectedTactician.AddSoldier(soldier);
            }
        }
    }

    protected void UnselectFormation(Unit _unit)
    {
        if (_unit.mainTactician)
        {
            foreach (Unit unit in _unit.mainTactician.Soldiers)
            {
                UnselectUnit(unit);

                if (unit.tempTactician && unit.tempTactician != unit.mainTactician)
                    unit.tempTactician.RemoveAndCheck(unit);
            }
        }
    }

    protected void UnselectUnit(Unit unit)
    {
        unit.SetSelected(false);
        if (SelectedUnitList.Contains(unit))
            SelectedUnitList.Remove(unit);
    }

    protected void UnselectSingleUnit(Unit unit)
    {
        if (unit.tempTactician && unit.tempTactician.RemoveAndCheck(unit))
            selectedTactician = null;

        UnselectUnit(unit);
    }

    virtual public void AddUnit(Unit unit)
    {
        unit.OnDeadEvent += () =>
        {
            TotalBuildPoints += unit.Cost;
            if (unit.IsSelected)
                SelectedUnitList.Remove(unit);
            UnitList.Remove(unit);
        };
        UnitList.Add(unit);
    }
    public void CaptureTarget(int points)
    {
        Debug.Log("CaptureTarget");
        TotalBuildPoints += points;
        CapturedTargets++;
    }
    public void LoseTarget(int points)
    {
        TotalBuildPoints -= points;
        CapturedTargets--;
    }
    #endregion

    #region Formation methods

    protected void CreateFormation(Formations.FormationManager.EFormationTypes newType)
    {
        if (selectedTactician)
            selectedTactician.GetComponent<Formations.FormationManager>().SwitchFormationType(newType);
    }

    protected void CreateTactician(Vector3 position)
    {
        if (tacticianPrefab)
        {
            GameObject tacticianObject = Instantiate(tacticianPrefab, position, Quaternion.identity);
            Tactician tactician = tacticianObject.GetComponent<Tactician>();

            List<Unit> soldiers = tactician.GetSoldiers();

            foreach (Unit unit in SelectedUnitList)
            {
                soldiers.Add(unit);
                unit.actions        -= unit.FollowFormation;
                unit.tempTactician  = tactician;
            }

            selectedTactician = tactician;
        }
    }

    protected void FormationLockToggle()
    {
        if (selectedTactician)
        {
            foreach (Unit unit in SelectedUnitList)
            {
                unit.tempTactician = selectedTactician.isFormationLocked ? selectedTactician : null;

                if (!selectedTactician.isFormationLocked && unit.mainTactician)
                    Destroy(unit.mainTactician.gameObject);

                unit.mainTactician = selectedTactician.isFormationLocked ? null : selectedTactician;
            }

            selectedTactician.isFormationLocked = !selectedTactician.isFormationLocked;
        }
    }

    #endregion

    #region Factory methods
    void AddFactory(Factory factory)
    {
        if (factory == null)
        {
            Debug.LogWarning("Trying to add null factory");
            return;
        }

        factory.OnDeadEvent += () =>
        {
            TotalBuildPoints += factory.Cost;
            if (factory.IsSelected)
                SelectedFactory = null;
            FactoryList.Remove(factory);
        };
        FactoryList.Add(factory);
    }
    virtual protected void SelectFactory(Factory factory)
    {
        if (factory == null || factory.IsUnderConstruction)
            return;

        SelectedFactory = factory;
        SelectedFactory.SetSelected(true);
        UnselectAllUnits();
    }
    virtual protected void UnselectCurrentFactory()
    {
        if (SelectedFactory != null)
            SelectedFactory.SetSelected(false);
        SelectedFactory = null;
    }
    protected bool RequestUnitBuild(int unitMenuIndex)
    {
        if (SelectedFactory == null)
            return false;

        return SelectedFactory.RequestUnitBuild(unitMenuIndex);
    }
    protected bool RequestFactoryBuild(int factoryIndex, Vector3 buildPos)
    {
        if (SelectedFactory == null)
            return false;

        int cost = SelectedFactory.GetFactoryCost(factoryIndex);
        if (TotalBuildPoints < cost)
            return false;

        // Check if positon is valid
        if (SelectedFactory.CanPositionFactory(factoryIndex, buildPos) == false)
            return false;

        Factory newFactory = SelectedFactory.StartBuildFactory(factoryIndex, buildPos);
        if (newFactory != null)
        {
            AddFactory(newFactory);
            TotalBuildPoints -= cost;

            return true;
        }
        return false;
    }
    #endregion

    #region MonoBehaviour methods
    virtual protected void Awake()
    {
        string rootName = Team.ToString() + "Team";
        TeamRoot = GameObject.Find(rootName)?.transform;
        if (TeamRoot)
            Debug.LogFormat("TeamRoot {0} found !", rootName);
    }
    virtual protected void Start()
    {
        CapturedTargets = 0;
        TotalBuildPoints = StartingBuildPoints;

        // get all team factory already in scene
        Factory[] allFactories = FindObjectsOfType<Factory>();
        foreach (Factory factory in allFactories)
        {
            if (factory.GetTeam() == GetTeam())
            {
                AddFactory(factory);
            }
        }

        Debug.Log("found " + FactoryList.Count + " factory for team " + GetTeam().ToString());
    }
    virtual protected void Update()
    {

    }
    #endregion
}