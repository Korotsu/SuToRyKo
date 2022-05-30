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
        None
    };

    public int nbLight = 0, nbHeavy = 0;
    public int nbLightInProgress = 0, nbHeavyInProgress = 0;
    public float cost = 0f;
    public Type taskType = Type.None;

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

    [SerializeField] private GameObject tacticianPrefab = null;

    private List<Tactician> tacticians = new List<Tactician>();
    private List<Tactician> waitingTacticians = new List<Tactician>();
    private List<Tactician> unusedTacticians = new List<Tactician>();

    private List<Task> runningTasks = new List<Task>();
    private List<Task> waitingTasks = new List<Task>();

    private List<Unit> unusedUnits = new List<Unit>();

    private Factory lightFactory = null;
    private Factory heavyFactory = null;

    private List<TargetBuilding> targetBuildings = new List<TargetBuilding>();

    bool isStarted = false;

    private float timer = 0;
    [SerializeField] private float timerDuration = 1f;



    private void Awake()
    {
        //GetLightAndHeavyFactory(out lightFactory, out heavyFactory);
    }
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

        GetLightAndHeavyFactory(out lightFactory, out heavyFactory);
    }

    void Update()
    {
        base.Update();

        timer += Time.deltaTime;
        
        if (!isStarted || timer >= timerDuration)
        {
            TaskInit();
            isStarted = true;
            
            timer = 0f;
        }

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
                    tactician.AddSoldier(_unit);
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
            int cost = CheckTroupCost(task);

            if (_TotalBuildPoints >= cost)
            {
                for (int i = 0; i < task.nbLight; i++)
                {
                    if (!lightFactory.RequestUnitBuild(0))
                        return false;
                }

                for(int i = 0; i < task.nbHeavy; i++)
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
                    tactician.AddSoldier(_unit);
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
        foreach (Task task in waitingTasks)
        {
            unusedTacticians.AddRange(task.tacticians);
        }

        waitingTasks.Clear();
        List<Task> sortedTaskList = new List<Task>();

        foreach (Base entity in FindObjectsOfType<Base>())
        {
            if (entity.GetTeam() != Team)
            {
                Task task = new Task();
                task.target = entity;

                if (entity is Tactician || entity is Factory)
                    task.taskType = Task.Type.Attack;
                else if (entity is TargetBuilding)
                    task.taskType = Task.Type.Capture;

                sortedTaskList.Add(task);
            }
        }
        sortedTaskList.Sort();


        int cost = 0;

        foreach (Task task in sortedTaskList)
        {
            int currentCost = 0;

            if (task.target is Tactician)
            {
                task.formationData = attackFormation;
                currentCost = CheckTroupCost(task);
                cost += currentCost;
            }
            else if (task.target is TargetBuilding)
            {
                task.formationData = captureFormation;
                currentCost = CheckTroupCost(task);
                cost += currentCost;
            }

            if (TotalBuildPoints < cost)
                break;
            
            else
            {
                task.cost = currentCost;
                waitingTasks.Add(task);
            }
        }
    }

    private void TaskUpdate()
    {
        int count = waitingTasks.Count;

        for (int i = 0; i < count; i++)
        {
            Task task = waitingTasks[i];

            if ((task.nbLight == 0 && task.nbHeavy == 0))
            {
                task.cost = 0;
                waitingTasks.Remove(task);
                runningTasks.Add(task);

                i--;
                count = waitingTasks.Count;
            }

        }

        count = unusedTacticians.Count;

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

        foreach (Task task in waitingTasks)
        {
            CreateTroup(task);
        }
    }

    void LaunchTask(Task task)
    {
        switch (task.taskType)
        {
            case Task.Type.Attack:
                foreach (Tactician tactician in task.tacticians)
                {
                    tactician.SetState(new TacticianAttackState(tactician));
                }
                break;
            case Task.Type.Capture:
                foreach (Tactician tactician in task.tacticians)
                {
                    tactician.SetState(new IdleTactician(tactician));//TODO : set in capture
                }
                break;
            case Task.Type.None:
                break;
            default:
                break;
        }
    }

    private bool BuildingTaskUpdate(Task task)
    { 
        if(task.target is TargetBuilding)
            return CreateCaptureTroup(task);

        return false;
    }

    #endregion

    #region Utility Methods

    private int CheckTroupCost(Task task)
    {
        float influence = task.target.Influence;


        if (influence <= 0)
            return int.MaxValue;


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

        if (!bestTactician && tacticianPrefab)
        {
            bestTactician = GameObject.Instantiate(tacticianPrefab).GetComponent<Tactician>();
            bestTactician.SetTeam(Team);
        }
        else if (!bestTactician && !tacticianPrefab)
        {
            Debug.LogError("There is no tacticianPrefab!");
            return int.MaxValue;
        }
        int lightUnitCost = lightFactory.GetUnitCost(0);
        int heavyUnitCost = heavyFactory.GetUnitCost(0);

        float lightUnitInfluance = lightFactory.GetBuildableUnitInfluence(0);
        float heavyUnitInfluance = heavyFactory.GetBuildableUnitInfluence(0);

        float newInfluence = Mathf.Clamp(influence - bestTactician.Influence, 0, float.MaxValue);

        
        float lightUnitsInfluence = task.formationData.lightUnit * newInfluence;
        float heavyUnitsInfluence = task.formationData.heavyUnit * newInfluence;


        int count = unusedUnits.Count;

        for (int i = 0; i < count; i++)
        {
            Unit unit = unusedUnits[i];

            if (unit.GetUnitData.type == EntityDataScriptable.Type.Light && lightUnitsInfluence > 0f)
                lightUnitsInfluence -= unit.Influence;
            
            else if (unit.GetUnitData.type == EntityDataScriptable.Type.Heavy && heavyUnitsInfluence > 0f)
                heavyUnitsInfluence -= unit.Influence;
                
            bestTactician.AddSoldier(unit);
            unusedUnits.Remove(unit);

            i--;
            count = unusedUnits.Count;

            if (lightUnitsInfluence <= 0f && heavyUnitsInfluence <= 0f)
                break;
        }

        int nbLightUnitToProduce = 0, nbHeavyUnitToProduce = 0;

        if (lightUnitsInfluence >= 0f)
            nbLightUnitToProduce = Mathf.RoundToInt(lightUnitsInfluence / lightUnitInfluance);
        
        if (nbHeavyUnitToProduce >= 0f)
            nbHeavyUnitToProduce = Mathf.RoundToInt(heavyUnitsInfluence / heavyUnitInfluance);
        
        
        //cost = lightUnitsInfluence;

        //Debug.Log($"Capture Troup: Total cost require = {cost} points.");

        unusedTacticians.Remove(bestTactician);
        waitingTacticians.Add(bestTactician);

        task.nbLightInProgress = bestTactician.nbLightInCreation;
        task.nbHeavyInProgress = bestTactician.nbHeavyInCreation;

        task.nbLight = nbLightUnitToProduce;
        task.nbHeavy = nbHeavyUnitToProduce;

        task.tacticians.Add(bestTactician);

        return nbLightUnitToProduce * lightUnitCost + nbHeavyUnitToProduce * heavyUnitCost;
    }

    private bool RequestCreationTroup()
    {
        return false;
    }

    private void GetLightAndHeavyFactory(out Factory light, out Factory heavy)
    {
        light = null;
        heavy = null;

        foreach (Factory factory in FactoryList)
        {
            if (!light && factory.GetFactoryData.type == EntityDataScriptable.Type.Light)
                light = factory;

            else if (!heavy && factory.GetFactoryData.type == EntityDataScriptable.Type.Heavy)
                heavy = factory;

            if (light && heavy)
                break;
        }
    }

    private bool CreateTroup(Task task)
    {
        if (_TotalBuildPoints >= task.cost)
        {
            while (task.nbLight - task.nbLightInProgress > 0)
            {
                if (!lightFactory.RequestUnitBuild(0, task.tacticians[0]))
                    break;
                else
                    task.nbLight--;
            }

            while (task.nbHeavy - task.nbHeavyInProgress > 0)
            {
                if (!heavyFactory.RequestUnitBuild(0, task.tacticians[0]))
                    break;
                else
                    task.nbHeavy--;
            }

            return true;
        }
        
        return false;
    }
    #endregion
}
