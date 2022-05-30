using UnityEngine;
using UnityEngine.UI;
public class TargetBuilding : BaseEntity
{
    [SerializeField] private float CaptureGaugeStart = 100f;
    [SerializeField] private float CaptureGaugeSpeed = 1f;
    
    [SerializeField] private int ImmediateBuildPoints = 5;
    
    [SerializeField] private float PointsPerSecond = 0.5f;
    private float OngoingPoint = 0f;
    
    
    [SerializeField] private Material BlueTeamMaterial = null;
    [SerializeField] private Material RedTeamMaterial = null;

    private Material NeutralMaterial = null;
    private MeshRenderer BuildingMeshRenderer = null;
    private Image GaugeImage;
    private int[] TeamScore;
    private float CaptureGaugeValue;
    private ETeam OwningTeam = ETeam.Neutral;
    private ETeam CapturingTeam = ETeam.Neutral;
    
    public new ETeam GetTeam() => OwningTeam;
    
    protected override float GetInfluence()
    {
        float p = 30;
        p += 30 *(1f - CaptureGaugeValue / CaptureGaugeStart);
        return p;
    }


    #region MonoBehaviour methods

    private void Start()
    {
        BuildingMeshRenderer = GetComponentInChildren<MeshRenderer>();
        NeutralMaterial = BuildingMeshRenderer.material;

        GaugeImage = GetComponentInChildren<Image>();
        if (GaugeImage)
            GaugeImage.fillAmount = 0f;
        CaptureGaugeValue = CaptureGaugeStart;
        TeamScore = new int[2];
        TeamScore[0] = 0;
        TeamScore[1] = 0;
    }

    private void Update()
    {
        if (OwningTeam != ETeam.Neutral && CapturingTeam == ETeam.Neutral)
        {
            OngoingPoint += PointsPerSecond * Time.deltaTime;

            if (OngoingPoint >= 1f)
            {
                OngoingPoint -= 1f;
                ++GameServices.GetControllerByTeam(OwningTeam).TotalBuildPoints;
            }
        }
        else if (CapturingTeam == OwningTeam || CapturingTeam == ETeam.Neutral)
            return;

        CaptureGaugeValue -= TeamScore[(int)CapturingTeam] * CaptureGaugeSpeed * Time.deltaTime;

        GaugeImage.fillAmount = 1f - CaptureGaugeValue / CaptureGaugeStart;

        if (CaptureGaugeValue <= 0f)
        {
            CaptureGaugeValue = 0f;
            OnCaptured(CapturingTeam);
        }
    }
    #endregion

    #region Capture methods
    public void StartCapture(Unit unit)
    {
        if (unit == null)
            return;

        TeamScore[(int)unit.GetTeam()] += unit.Cost;

        if (CapturingTeam == ETeam.Neutral)
        {
            if (TeamScore[(int)GameServices.GetOpponent(unit.GetTeam())] == 0)
            {
                CapturingTeam = unit.GetTeam();
                GaugeImage.color = GameServices.GetTeamColor(CapturingTeam);
            }
        }
        else
        {
            if (TeamScore[(int)GameServices.GetOpponent(unit.GetTeam())] > 0)
                ResetCapture();
        }
    }
    public void StopCapture(Unit unit)
    {
        if (unit == null)
            return;

        TeamScore[(int)unit.GetTeam()] -= unit.Cost;
        if (TeamScore[(int)unit.GetTeam()] == 0)
        {
            ETeam opponentTeam = GameServices.GetOpponent(unit.GetTeam());
            if (TeamScore[(int)opponentTeam] == 0)
            {
                ResetCapture();
            }
            else
            {
                CapturingTeam = opponentTeam;
                GaugeImage.color = GameServices.GetTeamColor(CapturingTeam);
            }
        }
    }

    private void ResetCapture()
    {
        CaptureGaugeValue = CaptureGaugeStart;
        CapturingTeam = ETeam.Neutral;
        GaugeImage.fillAmount = 0f;
    }

    private void OnCaptured(ETeam newTeam)
    {
        Debug.Log("target captured by " + newTeam.ToString());
        if (OwningTeam != newTeam)
        {
            UnitController teamController = GameServices.GetControllerByTeam(newTeam);
            if (teamController != null)
                teamController.CaptureTarget(ImmediateBuildPoints);

            if (OwningTeam != ETeam.Neutral)
            {
                // remove points to previously owning team
                teamController = GameServices.GetControllerByTeam(OwningTeam);
                if (teamController != null)
                    teamController.LoseTarget(ImmediateBuildPoints);
            }
        }

        ResetCapture();
        OwningTeam = newTeam;
        BuildingMeshRenderer.material = newTeam == ETeam.Blue ? BlueTeamMaterial : RedTeamMaterial;
    }
    #endregion
}
