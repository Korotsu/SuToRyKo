using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AI.BehaviorStates;
using AI.ScriptableObjects;

public class Task : IEqualityComparer<Task>, IComparable<Task>
{
    public enum Type
    {
        Attack,
        Capture,
        Count
    };

    public float cost = 0f;
    public int taskType = -1;

    public List<Tactician> tacticians = new List<Tactician>();
    public FormationData formationData = null;

    public Base target = null;

    public bool isRunning = false;

    public bool Equals(Task x, Task y)
    {
        if (!x.target || !y.target)
            return false;

        return x.target.Influence == y.target.Influence;
    }

    public int GetHashCode(Task obj)
    {
        return (int)obj.target.Influence;
    }

    public int CompareTo(Task other)
    {
        if (!target)
            return 1;
        else if (!other.target)
            return -1;

        return -target.Influence.CompareTo(other.target.Influence);
    }
}

[System.Serializable]
public class Strategist : UnitController
{

    [SerializeField] private FormationData attackFormation = null;
    [SerializeField] private FormationData captureFormation = null;

    private List<Tactician> tacticians = new List<Tactician>();
    private List<Tactician> waitingTacticians = new List<Tactician>();
    private List<Tactician> unusedTacticians = new List<Tactician>();

    private List<Task> runningTasks = new List<Task>();
    private List<Task> waitingTasks = new List<Task>();

    private List<Unit> unusedUnits = new List<Unit>();

    private List<TargetBuilding> targetBuildings = new List<TargetBuilding>();

    bool isStarted = false;
    private int previousUnitsCount = 0;
    private int totalAllocatedCost = 0;

    void Start()
    {
        base.Start();

        targetBuildings.Clear();

        foreach (TargetBuilding targetBuilding in FindObjectsOfType<TargetBuilding>())
        {
            targetBuildings.Add(targetBuilding);
        }

        foreach (Factory factory in FactoryList)
        {
            factory.OnUnitBuilt += (Unit unit) =>
            {
                unusedUnits.Add(unit);
            };
        }

        previousUnitsCount = UnitList.Count;
    }

    void Update()
    {
        base.Update();

        if (!isStarted)
        {
            TaskInit();
            isStarted = true;
        }

        TakeDecision();

    }

    private void TakeDecision()
    {
        TaskUpdate();
    }  

    #region Attack

    private void CreateAttackTroup(float influance)
    {
        float lightInfluance = attackFormation.lightUnit * influance;
        float heavyInfluance = attackFormation.heavyUnit * influance;

        Factory lightFactory = null;
        Factory heavyFactory = null;

        foreach (Factory factory in FactoryList)
        {
            if (!lightFactory && factory.GetFactoryData.TypeId == 0)
            {
                lightFactory = factory;
            }
            else if (!heavyFactory && factory.GetFactoryData.TypeId == 1)
            {
                heavyFactory = factory;
            }
        }

        int lightUnitCost = lightFactory.GetUnitCost(0);
        int heavyUnitCost = heavyFactory.GetUnitCost(0);

        int currentLightInfluance = 0;
        int currentHeavyInfluance = 0;

        while (currentLightInfluance <= lightInfluance)
        {
            lightFactory.RequestUnitBuild(0);
            currentLightInfluance += lightUnitCost;
        }

        while (currentHeavyInfluance <= heavyInfluance)
        {
            heavyFactory.RequestUnitBuild(0);
            currentHeavyInfluance += heavyUnitCost;
        }
    }
    public bool CreateAttackTactician()
    {
        if (unusedUnits.Count < 10)
            return false;

        int nbLight = 7, nbHeavy = 3, currentNbLight = 0, currentNbHeavy = 0;
        List<Unit> units = new List<Unit>();

        foreach (Unit unit in unusedUnits)
        {
            units.Add(unit);

            if (unit.GetUnitData.type == UnitDataScriptable.Type.Light && currentNbLight < nbLight)
                currentNbLight++;
            else if (unit.GetUnitData.type == UnitDataScriptable.Type.Heavy && currentNbHeavy < nbHeavy)
                currentNbHeavy++;

            if (currentNbLight == nbLight && currentNbHeavy == nbLight)
            {
                Tactician tactician = new Tactician();
                foreach (Unit _unit in units)
                {
                    tactician.AddSoldier(_unit.UnitLogic);
                }    
    
                tactician.SetState(new IdleTactician(tactician));//TODO: Set in AttackState
                return true;
            }
    }

        return false;
    }
    #endregion

    #region Capture

    private bool CreateCaptureTroup(Task task)
    {
        Tactician bestTactician = null;
        float bestInfluence = float.MinValue;

        foreach (Tactician tactician in unusedTacticians)
        {
            float influence = tactician.Influence;

            if (bestInfluence < influence)
            {
                bestTactician = tactician;
                bestInfluence = influence;
            }
        }

        if (bestTactician == null)
            bestTactician = new Tactician();

        if (bestTactician.Influence > task.target.Influence)
        {
            bestTactician.SetState(new IdleTactician(bestTactician));//TODO: Create a CaptureTactician
            task.tacticians.Add(bestTactician);
            waitingTacticians.Remove(bestTactician);
            task.isRunning = true;

            runningTasks.Add(task);
            waitingTasks.Remove(task);

            return true;
        }
        else
        {
            int nbLightUnits = 0, nbHeavyUnits = 0;
            int cost = CheckTroupCost(task, out nbLightUnits, out nbHeavyUnits);

            if (_TotalBuildPoints >= cost)
            {
                Factory lightFactory = null;
                Factory heavyFactory = null;

                foreach (Factory factory in FactoryList)
                {
                    if (!lightFactory && factory.GetFactoryData.type == EntityDataScriptable.Type.Light)
                        lightFactory = factory;

                    else if (!heavyFactory && factory.GetFactoryData.type == EntityDataScriptable.Type.Heavy)
                        heavyFactory = factory;

                    if (heavyFactory && lightFactory)
                        break;
                }

                for (int i = 0; i < nbLightUnits; i++)
                {
                    if (!lightFactory.RequestUnitBuild(0))
                        return false;
                }

                for(int i = 0; i < nbHeavyUnits; i++)
                {
                    if (!heavyFactory.RequestUnitBuild(0))
                        return false;
                }
            }
        }

        return false;
    }
    

    private bool CreateCaptureTacticien()
    {
        if (unusedUnits.Count < 5)
            return false;

        int nbUnit = 5;
        List<Unit> units = new List<Unit>();

        foreach (Unit unit in unusedUnits)
        {
            if (unit.GetUnitData.type == UnitDataScriptable.Type.Light && units.Count < nbUnit)
                units.Add(unit);

            if (units.Count == nbUnit)
            {
                Tactician tactician = new Tactician();
                foreach (Unit _unit in units)
                {
                    tactician.AddSoldier(_unit.UnitLogic);
                }

                tactician.SetState(new IdleTactician(tactician));//TODO: Set in CaptureState
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Task Methods
    private void TaskInit()
    {
        waitingTasks.Clear();
        List<Task> sortedTaskList = new List<Task>();

        foreach (Base entity in FindObjectsOfType<Base>())
        {
            if (entity.GetTeam() != Team)
            {
                Task task = new Task();
                task.target = entity;
                sortedTaskList.Add(task);
            }
        }
        sortedTaskList.Sort();


        int cost = 0;
        bool hasAddedTasks = false;

        foreach (Task task in sortedTaskList)
        {
            int nbLight = 0, nbHeavy = 0;
            int currentCost = 0;

            if (task.target is Tactician)
            {
                currentCost = CheckTroupCost(task, out nbLight, out nbHeavy);
                cost += currentCost;
                task.formationData = attackFormation;
            }
            else if (task.target is TargetBuilding)
            {
                currentCost = CheckTroupCost(task, out nbLight, out nbHeavy);
                cost += currentCost;
                task.formationData = captureFormation;
            }

            if (TotalBuildPoints < cost)
                break;
            
            else
            {
                hasAddedTasks = true;
                task.cost = currentCost;
                waitingTasks.Add(task);
            }
        }
    }

    private void TaskUpdate()
    {
        int count = unusedTacticians.Count;

        for (int i = 0; i < count; i++)
        {
            if (unusedTacticians[i].Soldiers.Count == 0)
            {
                Tactician tacticianToRemove = unusedTacticians[i];
                unusedTacticians.RemoveAt(i);
                Destroy(tacticianToRemove.gameObject);

                i--;
                count = unusedTacticians.Count;
            }
        }
    }

    private bool BuildingTaskUpdate(Task task)
    { 
        if(task.target is TargetBuilding)
            return CreateCaptureTroup(task);

        return false;
    }
    private bool TacticianTaskUpdate(Task task)
    {
        //if (TotalBuildPoints < CheckAttackTroupCost(task.target.Influence))
        //    return false;

        //CreateAttackTroup(task.target.Influence);
        return false;
    }

    #endregion

    #region Utility Methods

    private int CheckTroupCost(Task task, out int nbLightUnits, out int nbHeavyUnits)
    {
        nbLightUnits = 0;
        nbHeavyUnits = 0;

        float influence = task.target.Influence;


        if (influence <= 0)
            return int.MaxValue;

        Factory lightFactory = null;
        Factory heavyFactory = null;

        int cost = 0;

        foreach (Factory factory in FactoryList)
        {
            if (!lightFactory && factory.GetFactoryData.type == EntityDataScriptable.Type.Light)
                lightFactory = factory;

            else if (!heavyFactory && factory.GetFactoryData.type == EntityDataScriptable.Type.Heavy)
                heavyFactory = factory;

            if (heavyFactory && lightFactory)
                break;
        }

        Tactician bestTactician = null;
        float bestInfluence = float.MinValue;

        foreach (Tactician tactician in unusedTacticians)
        {
            float currentInfluence = tactician.Influence;

            if (bestInfluence < tactician.Influence)
            {
                bestTactician = tactician;
                bestInfluence = currentInfluence;
            }
        }

        if (!bestTactician)
            bestTactician = new Tactician();

        int lightUnitCost = lightFactory.GetUnitCost(0);
        int heavyUnitCost = heavyFactory.GetUnitCost(0);

        float lightUnitInfluance = lightFactory.GetBuildableUnitInfluence(0);
        float heavyUnitInfluance = heavyFactory.GetBuildableUnitInfluence(0);

        float newInfluence = Mathf.Clamp(influence - bestTactician.Influence, 0, float.MaxValue);

        
        float lightUnitsInfluence = task.formationData.lightUnit * newInfluence;
        float heavyUnitsInfluence = task.formationData.heavyUnit * newInfluence;

        

        foreach (Unit unit in unusedUnits)
        {
            if (unit.GetUnitData.type == EntityDataScriptable.Type.Light && lightUnitsInfluence > 0f)
            {
                lightUnitsInfluence -= unit.Influence;
                //bestTactician.AddSoldier(new UnitLogic(unit));//TODO
            }
            else if (unit.GetUnitData.type == EntityDataScriptable.Type.Heavy && heavyUnitsInfluence > 0f)
            {
                heavyUnitsInfluence -= unit.Influence;
                //bestTactician.AddSoldier(new UnitLogic(unit));//TODO
            }

            if (lightUnitsInfluence <= 0f && heavyUnitsInfluence <= 0f)
                break;
        }

        int nbLightUnitToProduce = 0, nbHeavyUnitToProduce = 0;

        if (lightUnitsInfluence > 0f)
            nbLightUnitToProduce = (int)(lightUnitsInfluence / lightUnitInfluance);
        
        if (nbHeavyUnitToProduce > 0f)
            nbHeavyUnitToProduce = (int)(heavyUnitsInfluence / heavyUnitInfluance);
        
        
        //cost = lightUnitsInfluence;

        //Debug.Log($"Capture Troup: Total cost require = {cost} points.");

        unusedTacticians.Remove(bestTactician);
        waitingTacticians.Add(bestTactician);

        return cost;
    }

    private bool RequestCreationTroup()
    {
        return false;
    }

    #endregion
}
