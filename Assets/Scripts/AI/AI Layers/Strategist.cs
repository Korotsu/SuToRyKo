using System.Collections.Generic;
using UnityEngine;
using AI.ScriptableObjects;

[System.Serializable]
public class Strategist : UnitController
{

    [SerializeField] private FormationData attackFormation = null;
    [SerializeField] private FormationData captureFormation = null;

    private List<TacticianState> orderlist = new List<TacticianState>();

    private List<Tactician> tacticians = new List<Tactician>();
    private List<Tactician> unusedTacticians = new List<Tactician>();

    private List<Unit> unusedUnits = new List<Unit>();

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
                unusedUnits.Add(unit);
            };
        }

        previousUnitsCount = UnitList.Count;
    }

    void Update()
    {
        base.Update();
        TakeDecision();
    }

    private void TakeDecision()
    {
        float influence = 30f;
        if (!isStarted)
        {
            if (CheckAttackTroupCost(influence) <= TotalBuildPoints)
                CreateAttackTroup(influence);
            else if (CheckCaptureTroupCost() <= TotalBuildPoints)
                CreateCaptureTroup();
            
            isStarted = true;
        }

        int currentCount = UnitList.Count;

        if (CheckAttackTroupCost(influence) <= TotalBuildPoints)
            CreateAttackTroup(influence);

        if (previousUnitsCount != currentCount)
        {
            previousUnitsCount = currentCount;
            CreateAttackTactician();
        }
    }

    #region Test

    #region Attack
    private int CheckAttackTroupCost(float influance)
    {
        if (!attackFormation)
        {
            Debug.LogError("AttackFormation is not set.");
            return int.MaxValue;
        }
        

        float lightInfluance = attackFormation.lightUnit * influance;
        float heavyInfluance = attackFormation.HeavyUnit * influance;

        Factory lightFactory = null;
        Factory heavyFactory = null;

        int cost = 0;

        foreach (Factory factory in FactoryList)
        {
            if (!lightFactory && factory.GetFactoryData.type == EntityDataScriptable.Type.Light)
            {
                lightFactory = factory;
            }
            else if (!heavyFactory && factory.GetFactoryData.type == EntityDataScriptable.Type.Heavy)
            {
                heavyFactory = factory;
            }
        }

        int lightUnitCost = lightFactory.GetUnitCost(0);
        int heavyUnitCost = heavyFactory.GetUnitCost(0);

        int currentLightInfluance = 0;
        int currentHeavyInfluance = 0;
       
        
        while(currentLightInfluance <= lightInfluance)
        {
            cost += lightUnitCost;
            currentLightInfluance += lightUnitCost;
        }

        while(currentHeavyInfluance <= heavyInfluance)
        {
            cost += heavyUnitCost;
            currentHeavyInfluance += heavyUnitCost;
        }

        Debug.Log($"Attack Troup: Total cost require = {cost} points.");
        return cost;
    }

    private void CreateAttackTroup(float influance)
    {
        float lightInfluance = attackFormation.lightUnit * influance;
        float heavyInfluance = attackFormation.HeavyUnit * influance;

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
    
                tactician.SetState(new AI.StateMachine.IdleTactician(tactician));//TODO: Set in AttackState
                return true;
            }
    }

        return false;
    }
    #endregion

    #region Capture
    private void CreateCaptureTroup()
    {
        int nbUnit = 5;

        Factory lightFactory = null;

        foreach (Factory factory in FactoryList)
        {
            if (!lightFactory && factory.GetFactoryData.TypeId == 0)
            {
                lightFactory = factory;
                break;
            }
        }

        for (int i = 0; i < nbUnit; i++)
        {
            lightFactory.RequestUnitBuild(0);
        }
    }
    private int CheckCaptureTroupCost()
    {
        //This is a test
        int nbUnit = 5;

        Factory lightFactory = null;

        int cost = 0;

        foreach (Factory factory in FactoryList)
        {
            if (!lightFactory && factory.GetFactoryData.TypeId == 0)
            {
                lightFactory = factory;
                break;
            }
        }

        for (int i = 0; i < nbUnit; i++)
        {
            cost += lightFactory.GetUnitCost(0);
        }


        Debug.Log($"Capture Troup: Total cost require = {cost} points.");

        return cost;
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

                tactician.SetState(new AI.StateMachine.IdleTactician(tactician));//TODO: Set in CaptureState
                return true;
            }
        }

        return false;
    }
    #endregion

    #endregion
}