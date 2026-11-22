using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SimulationUIManager : MonoBehaviour
{
    public static SimulationUIManager Instance;

    [Header("Top Panel: Clock & Status Header")]
    public TextMeshProUGUI topHeaderText;

    [Header("Left Panel: UI Text References")]
    public TextMeshProUGUI populationText;
    public TextMeshProUGUI workingText;
    public TextMeshProUGUI hungryText;
    public TextMeshProUGUI logsText;
    public TextMeshProUGUI firewoodText;
    public TextMeshProUGUI oreText;
    public TextMeshProUGUI toolsText;

    [Header("Right Panel: Live Activity Stream")]
    public TextMeshProUGUI actionLogText;
    public int maxLogEntries = 8;
    private static List<string> eventLog = new List<string>();

    [Header("Simulation Timing & Settings")]
    public float refreshInterval = 0.25f;
    private float timer = 0f;
    private float totalSimTime = 0f;
    private bool isFastSpeed = false;

    // Status tracking for Top UI Header
    private string currentStatusMessage = "NORMAL";
    private string statusColorHex = "#00FF00"; // Green
    private float statusResetTimer = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        // Track elapsed simulation time
        totalSimTime += Time.deltaTime;

        // Reset temporary status messages (like RESTOCK) back to NORMAL after 4 seconds
        if (statusResetTimer > 0f)
        {
            statusResetTimer -= Time.deltaTime;
            if (statusResetTimer <= 0f && currentStatusMessage != "LUNCH BREAK")
            {
                SetStatus("NORMAL", "#00FF00");
            }
        }

        timer += Time.deltaTime;
        if (timer >= refreshInterval)
        {
            timer = 0f;
            UpdateTopHeader();
            UpdateLeftDashboard();
            UpdateRightLogUI();
        }
    }

    public static string GetFormattedSimTime()
    {
        if (Instance == null) return "Day 1 (00:00)";
        int day = Mathf.FloorToInt(Instance.totalSimTime / 120f) + 1; // 1 Day = 2 minutes
        int minutes = Mathf.FloorToInt((Instance.totalSimTime % 120f) / 2f);
        int seconds = Mathf.FloorToInt(Instance.totalSimTime % 2f * 30f);
        return $"Day {day} ({minutes:00}:{seconds:00})";
    }

    public static void LogAction(string message)
    {
        string simTimestamp = GetFormattedSimTime();
        eventLog.Insert(0, $"[{simTimestamp}] {message}");

        int limit = (Instance != null) ? Instance.maxLogEntries : 8;
        if (eventLog.Count > limit)
        {
            eventLog.RemoveAt(eventLog.Count - 1);
        }
    }

    private void SetStatus(string statusText, string colorHex, float duration = 0f)
    {
        currentStatusMessage = statusText;
        statusColorHex = colorHex;
        statusResetTimer = duration;
    }

    void UpdateTopHeader()
    {
        if (topHeaderText == null) return;

        string speedLabel = isFastSpeed ? "2.0x" : "1.0x";
        topHeaderText.text = $"<b>VILLAGE GOAP</b>  |  {GetFormattedSimTime()}  |  Status: <b><color={statusColorHex}>{currentStatusMessage}</color></b>  |  Speed: {speedLabel}";
    }

    void UpdateLeftDashboard()
    {
        Labourer[] agents = Object.FindObjectsByType<Labourer>(FindObjectsSortMode.None);
        int totalPopulation = agents.Length;
        int workingCount = 0;
        int hungryCount = 0;

        foreach (Labourer agent in agents)
        {
            if (agent.hunger >= 50f) hungryCount++;
            else workingCount++;
        }

        if (populationText != null) populationText.text = "Population: " + totalPopulation;
        if (workingText != null) workingText.text = "Working: " + workingCount;
        if (hungryText != null) hungryText.text = "Seeking Food: " + hungryCount;

        SupplyPileComponent[] supplyPiles = Object.FindObjectsByType<SupplyPileComponent>(FindObjectsSortMode.None);
        int totalLogs = 0, totalFirewood = 0, totalOre = 0, totalTools = 0;

        foreach (SupplyPileComponent pile in supplyPiles)
        {
            totalLogs += pile.numLogs;
            totalFirewood += pile.numFirewood;
            totalOre += pile.numOre;
            totalTools += pile.numTools;
        }

        if (logsText != null) logsText.text = "Logs: " + totalLogs;
        if (firewoodText != null) firewoodText.text = "Firewood: " + totalFirewood;
        if (oreText != null) oreText.text = "Ore: " + totalOre;
        if (toolsText != null) toolsText.text = "Tools: " + totalTools;
    }

    void UpdateRightLogUI()
    {
        if (actionLogText != null)
        {
            actionLogText.text = string.Join("\n", eventLog);
        }
    }

    // Force Lunch Break
    public void OnClick_ForceLunchBreak()
    {
        Labourer[] agents = Object.FindObjectsByType<Labourer>(FindObjectsSortMode.None);
        foreach (Labourer agent in agents)
        {
            agent.hunger = 60f; 
        }

        SetStatus("LUNCH BREAK", "#FFFF00", 5f); 
        LogAction("<color=yellow><b>EVENT:</b> Lunch Break forced! All workers replanning to eat.</color>");
    }

    // Toggle Speed (1.0x / 2.0x)
    public void OnClick_ToggleSpeed()
    {
        isFastSpeed = !isFastSpeed;
        Time.timeScale = isFastSpeed ? 2.0f : 1.0f;

        string speedLabel = isFastSpeed ? "2.0x" : "1.0x";
        LogAction($"<color=green><b>EVENT:</b> Simulation speed set to {speedLabel}</color>");
    }

    // Restock Supplies
    public void OnClick_RestockSupplies()
    {
        SupplyPileComponent[] supplyPiles = Object.FindObjectsByType<SupplyPileComponent>(FindObjectsSortMode.None);
        foreach (SupplyPileComponent pile in supplyPiles)
        {
            pile.numLogs += 20;
            pile.numOre += 20;
            pile.numTools += 10;
        }

        SetStatus("SUPPLIES RESTOCKED", "#00FFFF", 3f); 
        LogAction("<color=cyan><b>EVENT:</b> Restocked +20 Logs, +20 Ore, +10 Tools to Supply Piles!</color>");
    }
}