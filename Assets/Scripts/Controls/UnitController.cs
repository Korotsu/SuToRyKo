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
            Debug.Log("TotalBuildPoints updated");
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
    protected List<Unit> SelectedUnitList = new List<Unit>();
    protected readonly List<Factory> FactoryList = new List<Factory>();
    protected Factory SelectedFactory = null;

    [SerializeField]
    protected GameObject tacticianPrefab = null;

    protected List<Tactician> lockedTacticians = new List<Tactician>();
    protected Tactician selectedTactician = null;

    // events
    protected Action OnBuildPointsUpdated;
    protected Action OnCaptureTarget;

    #region Unit methods
    protected void UnselectAllUnits()
    {
        foreach (Unit unit in SelectedUnitList)
        {
            unit.SetSelected(false);

            if (selectedTactician && unit.tempTactician && selectedTactician.GetTacticianState() != null)
            {
                if (selectedTactician != unit.mainTactician)
                    unit.tempTactician.GetSoldiers().Remove(unit.GetComponent<Soldier>());
                unit.tempTactician = null;
            }
        }

        SelectedUnitList.Clear();

        if (selectedTactician && selectedTactician.isFormationLocked == false && selectedTactician.GetSoldiers().Count <= 1)
            Destroy(selectedTactician.gameObject);
        
        selectedTactician = null;
    }
    protected void SelectAllUnits()
    {
        UnselectAllUnits();

        foreach (Unit unit in UnitList)
        {
            if (unit.tempTactician || unit.mainTactician)
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
            if (unit.tempTactician || unit.mainTactician)
                SelectFormation(unit);

            else
                SelectSingleUnit(unit);
        }
    }
    protected void SelectUnitList(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            if (unit.tempTactician || unit.mainTactician)
                SelectFormation(unit);

            else
                SelectSingleUnit(unit);
        }
    }
    protected void SelectUnitList(Unit [] units)
    {
        foreach (Unit unit in units)
        {
            if (unit.tempTactician || unit.mainTactician)
                SelectFormation(unit);

            else
                SelectSingleUnit(unit);
        }
    }
    protected void SelectSingleUnit(Unit unit)
    {
        SelectUnit(unit);
        
        if ((selectedTactician == null && SelectedUnitList.Count > 1) || (selectedTactician && selectedTactician.isFormationLocked))
            CreateTactician();

        else if(selectedTactician)
        {
            selectedTactician.GetSoldiers().Add(unit.GetComponent<Soldier>());
            unit.tempTactician = selectedTactician;
        }
    }

    protected void SelectUnit(Unit unit)
    {
        unit.SetSelected(true);

        if(!SelectedUnitList.Contains(unit))
            SelectedUnitList.Add(unit);
    }

    protected void SelectFormation(Unit unit)
    {
        selectedTactician ??= unit.tempTactician ?? unit.mainTactician;
        unit.GetAllUnitsInFormation().ForEach(_unit => SelectUnit(_unit));
    }

    protected void UnselectFormation(Unit unit)
    {
        foreach (Unit _unit in unit.GetAllUnitsInFormation())
        {
            UnselectUnit(_unit);

            if (selectedTactician && _unit.tempTactician && selectedTactician.GetTacticianState() != null)
            {
                if(!_unit.mainTactician)
                    _unit.tempTactician.GetSoldiers().Remove(_unit.GetComponent<Soldier>());
                _unit.tempTactician = null;
            }
        }

        if (selectedTactician && selectedTactician.GetSoldiers().Count <= 1)
        {
            selectedTactician = null;

            if (selectedTactician.isFormationLocked == false)
                Destroy(selectedTactician.gameObject);
        }

        
    }

    protected void UnselectUnit(Unit unit)
    {
        unit.SetSelected(false);
        SelectedUnitList.Remove(unit);
    }

    protected void UnselectSingleUnit(Unit unit)
    {
        UnselectUnit(unit);

        if (selectedTactician && selectedTactician.GetSoldiers().Count <= 1)
        {
            Destroy(selectedTactician.gameObject);
            selectedTactician = null;
        }
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

    protected void CreateLinearFormation()
    {
        selectedTactician.GetComponent<Formations.FormationManager>().SwitchFormationType(Formations.FormationManager.EFormationTypes.Linear);
        //if (lockedTacticians.Count > 0)
        //lockedTacticians.ForEach(tactician => tactician.GetComponent<Formations.FormationManager>().SwitchFormationType(Formations.FormationManager.EFormationTypes.Linear));
    }

    protected void KillTactician()
    {
        if (lockedTacticians.Count > 0)
        {
            for (int i = 0; i < lockedTacticians.Count; i++)
            {
                Destroy(lockedTacticians[i].gameObject);
            }

            lockedTacticians.ForEach(tactician => Destroy(tactician.gameObject));
        }
    }

    protected void CreateTactician()
    {
        if (tacticianPrefab)
        {
            GameObject tacticianObject = Instantiate(tacticianPrefab, transform);
            Tactician tactician = tacticianObject.GetComponent<Tactician>();

            List<Soldier> soldiers = tactician.GetSoldiers();

            foreach (Unit unit in SelectedUnitList)
            {
                soldiers.Add(unit.GetComponent<Soldier>());
                unit.tempTactician = tactician;
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
    virtual protected void Start ()
    {
        CapturedTargets = 0;
        TotalBuildPoints = StartingBuildPoints;

        // get all team factory already in scene
        Factory [] allFactories = FindObjectsOfType<Factory>();
        foreach(Factory factory in allFactories)
        {
            if (factory.GetTeam() == GetTeam())
            {
                AddFactory(factory);
            }
        }

        Debug.Log("found " + FactoryList.Count + " factory for team " + GetTeam().ToString());
    }
    virtual protected void Update ()
    {
		
	}
    #endregion
}