using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AI.BehaviorStates;
using AI.ScriptableObjects;


public class Task
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
    public Base target = null;

    public bool isRunning = false;
}

[System.Serializable]
public class Strategist : UnitController
{

    [SerializeField] private FormationData attackFormation = null;
    [SerializeField] private FormationData captureFormation = null;

    private List<Tactician> tacticians = new List<Tactician>();
    private List<Tactician> waitingTacticians = new List<Tactician>();

    private List<Task> runningTasks = new List<Task>();
    private List<Task> waitingTasks = new List<Task>();

    private List<Unit> waitingUnits = new List<Unit>();

    private List<TargetBuilding> targetBuildings = new List<TargetBuilding>();

    bool isStarted = false;
    private int previousUnitsCount = 0;

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
                waitingUnits.Add(unit);
            };
        }

        previousUnitsCount = UnitList.Count;
    }

    void Update()
    {
        base.Update();

        if (!isStarted)
        {
            //TakeDecision();
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
        if (waitingUnits.Count < 10)
            return false;

        int nbLight = 7, nbHeavy = 3, currentNbLight = 0, currentNbHeavy = 0;
        List<Unit> units = new List<Unit>();

        foreach (Unit unit in waitingUnits)
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

        foreach (Tactician tactician in waitingTacticians)
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
            bestTactician.SetState(new AI.StateMachine.IdleTactician(bestTactician));//TODO: Set in attack
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
            int cost = CheckTroupCost(ref bestTactician, captureFormation, task.target.Influence, out nbLightUnits, out nbHeavyUnits);

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
        if (waitingUnits.Count < 5)
            return false;

        int nbUnit = 5;
        List<Unit> units = new List<Unit>();

        foreach (Unit unit in waitingUnits)
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

    private void TaskUpdate()
    {
        waitingTasks.Clear();

        foreach (Base entity in FindObjectsOfType<Base>())
        {
            if (entity.GetTeam() != Team)
            {
                Task task = new Task();
                task.target = entity;
                waitingTasks.Add(task);
            }
        }

        bool canCreateTroup = true;

        float bestInfluence = float.MinValue;
        Task priorityTask = null;

        while (canCreateTroup)
        {
            foreach (Task task in waitingTasks)
            {
                float influence = task.target.Influence;
                if (bestInfluence < influence)
                {
                    bestInfluence = influence;
                    priorityTask = task;
                }
            }

            if (priorityTask != null)
            {
                if (priorityTask.target.GetType() == typeof(Tactician))
                {
                    canCreateTroup = TacticianTaskUpdate(priorityTask);
                }
                else if (priorityTask.target.GetType() == typeof(TargetBuilding))
                {
                    canCreateTroup = BuildingTaskUpdate(priorityTask);
                }
                else
                    canCreateTroup = false;
            }
            else
                canCreateTroup = false;
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

    private int CheckTroupCost(ref Tactician tactician, FormationData formation, float influence, out int nbLightUnits, out int nbHeavyUnits)
    {
        nbLightUnits = 0;
        nbHeavyUnits = 0;

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

        int lightUnitCost = lightFactory.GetUnitCost(0);
        int heavyUnitCost = heavyFactory.GetUnitCost(0);

        float lightUnitInfluance = lightFactory.GetBuildableUnitInfluence(0);
        float heavyUnitInfluance = heavyFactory.GetBuildableUnitInfluence(0);

        float newInfluence = influence - tactician.Influence;

        nbLightUnits = (int)((formation.lightUnit * newInfluence) / lightUnitInfluance);
        nbHeavyUnits = (int)((formation.heavyUnit * newInfluence) / heavyUnitInfluance);

        foreach (Unit unit in waitingUnits)
        {
            if (unit.GetUnitData.type == EntityDataScriptable.Type.Light && nbLightUnits > 0)
                nbLightUnits--;
            else if (unit.GetUnitData.type == EntityDataScriptable.Type.Heavy && nbHeavyUnits > 0)
                nbHeavyUnits--;

            if (nbLightUnits == 0 && nbHeavyUnits == 0)
                break;
        }

        cost = nbLightUnits * lightUnitCost + nbHeavyUnits * heavyUnitCost;

        Debug.Log($"Capture Troup: Total cost require = {cost} points.");

        return cost;
    }

    #endregion
}
