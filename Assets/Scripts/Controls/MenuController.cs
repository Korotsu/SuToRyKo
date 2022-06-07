using System.Collections;
using System.Collections.Generic;
using System;
using AI.BehaviorStates;
using Formations;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    Transform FactoryMenuCanvas = null;
    public GraphicRaycaster BuildMenuRaycaster { get; private set; }

    private UnitController Controller = null;
    private GameObject FactoryMenuPanel = null;
    private Text BuildPointsText = null;
    private Text CapturedTargetsText = null;
    private Button[] BuildUnitButtons = null;
    private Button[] BuildFactoryButtons = null;
    private Button CancelBuildButton = null;
    private Button CancelFactoryButton = null;
    private Text[] BuildQueueTexts = null;

    
    [SerializeField] private Transform UnitsMenuCanvas = null;
    
    public GraphicRaycaster UnitsMenuRaycaster { get; private set; }
    
    private GameObject UnitsMenuPanel = null;
    private readonly GameObject[] FormationButtons = new GameObject[4];

    
    public void HideFactoryMenu()
    {
        if (FactoryMenuPanel)
            FactoryMenuPanel.SetActive(false);
    }
    public void ShowFactoryMenu()
    {
        if (FactoryMenuPanel)
            FactoryMenuPanel.SetActive(true);
    }
    
    public void HideUnitsMenu()
    {
        if (UnitsMenuPanel)
            UnitsMenuPanel.SetActive(false);
    }
    public void ShowUnitsMenu()
    {
        if (UnitsMenuPanel)
            UnitsMenuPanel.SetActive(true);
    }
    
    public void UpdateBuildPointsUI()
    {
        if (BuildPointsText != null)
            BuildPointsText.text = "Build Points : " + Controller.TotalBuildPoints;
    }
    public void UpdateCapturedTargetsUI()
    {
        if (CapturedTargetsText != null)
            CapturedTargetsText.text = "Captured Targets : " + Controller.CapturedTargets;
    }
    public void UpdateFactoryBuildQueueUI(int i, Factory selectedFactory)
    {
        if (selectedFactory == null)
            return;
        int queueCount = selectedFactory.GetQueuedCount(i);
        if (queueCount > 0)
        {
            BuildQueueTexts[i].text = "+" + queueCount;
            BuildQueueTexts[i].enabled = true;
        }
        else
        {
            BuildQueueTexts[i].enabled = false;
        }
    }
    public void HideAllFactoryBuildQueue()
    {
        foreach (Text text in BuildQueueTexts)
        {
            if (text)
                text.enabled = false;
        }
    }
    public void UnregisterBuildButtons(int availableUnitsCount, int availableFactoriesCount)
    {
        // unregister build buttons
        for (int i = 0; i < availableUnitsCount; i++)
        {
            BuildUnitButtons[i].onClick.RemoveAllListeners();
        }
        for (int i = 0; i < availableFactoriesCount; i++)
        {
            BuildFactoryButtons[i].onClick.RemoveAllListeners();
        }
    }

    public void UpdateFactoryMenu(Factory selectedFactory, Func<int, bool> requestUnitBuildMethod, Action<int> enterFactoryBuildModeMethod, Action exitFactoryBuildMethod)
    {
        ShowFactoryMenu();

        // Unit build buttons
        // register available buttons
        int i = 0;
        for (; i < selectedFactory.AvailableUnitsCount; i++)
        {
            BuildUnitButtons[i].gameObject.SetActive(true);

            int index = i; // capture index value for event closure
            BuildUnitButtons[i].onClick.AddListener(() =>
            {
                if (requestUnitBuildMethod(index))
                    UpdateFactoryBuildQueueUI(index, selectedFactory);
            });

            Text[] buttonTextArray = BuildUnitButtons[i].GetComponentsInChildren<Text>();
            Text buttonText = buttonTextArray[0];//BuildUnitButtons[i].GetComponentInChildren<Text>();
            UnitDataScriptable data = selectedFactory.GetBuildableUnitData(i);
            buttonText.text = data.Caption + "(" + data.Cost + ")";

            // Update queue count UI
            BuildQueueTexts[i] = buttonTextArray[1];
            UpdateFactoryBuildQueueUI(i, selectedFactory);
        }
        // hide remaining buttons
        for (; i < BuildUnitButtons.Length; i++)
        {
            BuildUnitButtons[i].gameObject.SetActive(false);
        }

        // activate Cancel button
        CancelBuildButton.onClick.AddListener(  () =>
                                                {
                                                    selectedFactory.CancelCurrentBuild();
                                                    HideAllFactoryBuildQueue();
                                                });

        // Factory build buttons
        // register available buttons
        i = 0;
        for (; i < selectedFactory.AvailableFactoriesCount; i++)
        {
            BuildFactoryButtons[i].gameObject.SetActive(true);

            int index = i; // capture index value for event closure
            BuildFactoryButtons[i].onClick.AddListener(() => { enterFactoryBuildModeMethod(index); });

            Text buttonText = BuildFactoryButtons[i].GetComponentInChildren<Text>();
            FactoryDataScriptable data = selectedFactory.GetBuildableFactoryData(i);
            buttonText.text = data.Caption + "(" + data.Cost + ")";
        }

        CancelFactoryButton.onClick.AddListener( exitFactoryBuildMethod.Invoke );

        
        // hide remaining buttons
        for (; i < BuildFactoryButtons.Length; i++)
        {
            BuildFactoryButtons[i].gameObject.SetActive(false);
        }
    }

    public void UpdateUnitsPannel()
    {
        switch (Controller.SelectedUnitList.Count)
        {
            case 0:
                HideUnitsMenu();
                break;
            
            case 1:
            {
                ShowUnitsMenu();

                foreach (GameObject formationButton in FormationButtons)
                {
                    formationButton.SetActive(false);
                }

                break;
            }
            
            default:
            {
                ShowUnitsMenu();
                
                foreach (GameObject formationButton in FormationButtons)
                {
                    formationButton.SetActive(true);
                }

                break;
            }
        }
    }
    
    private void Awake()
    {
        if (!FactoryMenuCanvas ||  !UnitsMenuCanvas)
        {
            Debug.LogWarning("FactoryMenuCanvas or UnitsMenuCanvas was not assigned in inspector");
        }
        else
        {
            Transform FactoryMenuPanelTransform = FactoryMenuCanvas.Find("FactoryMenu_Panel");
            if (FactoryMenuPanelTransform)
            {
                FactoryMenuPanel = FactoryMenuPanelTransform.gameObject;
                FactoryMenuPanel.SetActive(false);
            }
            BuildMenuRaycaster = FactoryMenuCanvas.GetComponent<GraphicRaycaster>();

            Transform BuildPointsTextTransform = FactoryMenuCanvas.Find("BuildPointsText");
            if (BuildPointsTextTransform)
            {
                BuildPointsText = BuildPointsTextTransform.GetComponent<Text>();
            }
            
            Transform CapturedTargetsTextTransform = FactoryMenuCanvas.Find("CapturedTargetsText");
            if (CapturedTargetsTextTransform)
            {
                CapturedTargetsText = CapturedTargetsTextTransform.GetComponent<Text>();
            }


            Transform UnitsMenuPanelTansform = UnitsMenuCanvas.Find("UnitsSelection_Panel");
            if (UnitsMenuPanelTansform)
            {
                UnitsMenuPanel = UnitsMenuPanelTansform.gameObject;
                UnitsMenuPanel.SetActive(false);
            }
            UnitsMenuRaycaster = UnitsMenuCanvas.GetComponent<GraphicRaycaster>();
        }

        Controller = GetComponent<PlayerController>();
    }
    
    private void Start()
    {
        BuildUnitButtons    = FactoryMenuPanel.transform.Find("BuildUnitMenu_Panel").GetComponentsInChildren<Button>();
        BuildFactoryButtons = FactoryMenuPanel.transform.Find("BuildFactoryMenu_Panel").GetComponentsInChildren<Button>();
        CancelBuildButton   = FactoryMenuPanel.transform.Find("UnitsCancel_Button").GetComponent<Button>();
        CancelFactoryButton = FactoryMenuPanel.transform.Find("FactoryCancel_Button").GetComponent<Button>();
        BuildQueueTexts = new Text[BuildUnitButtons.Length];


        UnitsMenuPanel.transform.Find("AttackStanceButton").GetComponent<Button>().onClick.AddListener
            (() =>
            {
                if (Controller.selectedTactician)
                    Controller.selectedTactician.SetState(new TacticianAttackState(Controller.selectedTactician));
                else
                    foreach (Unit unit in Controller.SelectedUnitList)
                        unit.UnitLogic.SetState(new UnitCombatState(unit.UnitLogic));
            } );
        
        UnitsMenuPanel.transform.Find("IdleStanceButton").GetComponent<Button>().onClick.AddListener
            (() =>
            {
                if (Controller.selectedTactician)
                    Controller.selectedTactician.SetState(new IdleTactician(Controller.selectedTactician));
                else
                    foreach (Unit unit in Controller.SelectedUnitList)
                        unit.UnitLogic.SetState(new IdleUnit(unit.UnitLogic));
            } );
        
        
        FormationButtons[0] = UnitsMenuPanel.transform.Find("FormationLineButton").gameObject;
        FormationButtons[1] = UnitsMenuPanel.transform.Find("FormationSpikeButton").gameObject;
        FormationButtons[2] = UnitsMenuPanel.transform.Find("FormationCurveButton").gameObject;
        FormationButtons[3] = UnitsMenuPanel.transform.Find("CustomAngleControlSlider").gameObject;

        
        FormationButtons[0].GetComponent<Button>().onClick.AddListener
            (() => {Controller.selectedTactician.formationManager.SwitchFormationType(FormationManager.EFormationTypes.Linear);} );
        
        FormationButtons[1].GetComponent<Button>().onClick.AddListener
            (() => {Controller.selectedTactician.formationManager.SwitchFormationType(FormationManager.EFormationTypes.Curved);} );

        FormationButtons[2].GetComponent<Button>().onClick.AddListener
            (() => {Controller.selectedTactician.formationManager.SwitchFormationType(FormationManager.EFormationTypes.VShaped);} );
        
        FormationButtons[3].GetComponent<Slider>().onValueChanged.AddListener
            ((value) =>
            {
                Controller.selectedTactician.formationManager.FormationAngle = value;
                Controller.selectedTactician.formationManager.SwitchFormationType(FormationManager.EFormationTypes.Custom);
            } );
    }
}

        
