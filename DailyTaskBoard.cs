using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DailyTaskBoard : MonoBehaviour
{
    public static DailyTaskBoard Instance { get; private set; }

    [Serializable]
    public class EmployeeConfig
    {
        public string employeeId;
        public string displayName;
        [Tooltip("该员工今天需要去 Fab 维修的机台ID（如 NCECU21）")]
        public string targetMachineId; // 🌟 区分开来：这是去干活的机台，不是办公桌
    }

    [Serializable]
    public class DailyTask
    {
        public string employeeId;
        public string employeeName;
        public string targetMachineId; // 🌟
        public string description;
        public bool completed;
        public DateTime? completedAt;
    }

    [Header("员工配置（包含玩家和NPC）")]
    public List<EmployeeConfig> employeeRoster = new List<EmployeeConfig>();

    [Header("UI（可选）")]
    public TextMeshProUGUI taskListText;
    public TextMeshProUGUI statusText;

    [Header("时间设置")]
    [Tooltip("是否使用系统时间。关闭后可用调试时间验证流程。")]
    public bool useSystemClock = true;
    [Range(0, 23)] public int debugHour = 8;
    [Range(0, 59)] public int debugMinute = 30;

    [Header("会议结束时间")]
    [Tooltip("早会结束后发布任务（默认 09:00）。")]
    public int morningReleaseHour = 9;
    public int morningReleaseMinute = 0;

    [Tooltip("晚会结束后汇总当天完成状况（默认 19:00）。")]
    public int eveningSummaryHour = 19;
    public int eveningSummaryMinute = 0;

    public IReadOnlyList<DailyTask> CurrentTasks => currentTasks;

    private readonly List<DailyTask> currentTasks = new List<DailyTask>();
    private int currentDay = -1;
    private int lastMorningReleaseDay = -1;
    private int lastEveningSummaryDay = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        DateTime now = GetCurrentTime();

        RotateDayIfNeeded(now);

        if (IsAfterOrEqual(now, morningReleaseHour, morningReleaseMinute) && lastMorningReleaseDay != now.DayOfYear)
        {
            GenerateTasksAfterMorningMeeting(now);
            lastMorningReleaseDay = now.DayOfYear;
        }

        if (IsAfterOrEqual(now, eveningSummaryHour, eveningSummaryMinute) && lastEveningSummaryDay != now.DayOfYear)
        {
            RunEveningSummary(now);
            lastEveningSummaryDay = now.DayOfYear;
        }
    }

    // 🌟 传入的参数改为 machineId
    public void RecordPMCompletion(string machineId, string employeeId = null)
    {
        DateTime now = GetCurrentTime();
        
        string resolvedEmployeeId = string.IsNullOrWhiteSpace(employeeId) 
            ? ResolveCurrentPlayerEmployeeId() 
            : employeeId.Trim();

        if (string.IsNullOrWhiteSpace(resolvedEmployeeId))
        {
            SetStatus("PM 记录失败：未识别到员工账号，请先在电脑登录。");
            return;
        }

        if (currentTasks.Count == 0)
        {
            SetStatus("PM 已执行，但今日任务尚未发布（9:00 后可查看）。");
            return;
        }

        for (int i = 0; i < currentTasks.Count; i++)
        {
            DailyTask task = currentTasks[i];

            if (task.completed) continue;
            if (!string.Equals(task.employeeId, resolvedEmployeeId, StringComparison.OrdinalIgnoreCase)) continue;
            // 🌟 匹配目标机台 ID
            if (!string.Equals(task.targetMachineId, machineId, StringComparison.OrdinalIgnoreCase)) continue;

            task.completed = true;
            task.completedAt = now;
            SetStatus($"✅ PM完成：{task.employeeName} - {task.targetMachineId}（{now:HH:mm}）");
            RefreshTaskUI();
            return;
        }

        SetStatus($"未找到可匹配任务：员工[{resolvedEmployeeId}] / 机台[{machineId}]。");
        RefreshTaskUI();
    }

    public void RefreshTaskUI()
    {
        if (taskListText == null) return;

        if (currentTasks.Count == 0)
        {
            taskListText.text = "今日任务尚未发布（早会结束 09:00 后可查看）";
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("今日工作任务");
        sb.AppendLine("------------------------");

        for (int i = 0; i < currentTasks.Count; i++)
        {
            DailyTask task = currentTasks[i];
            string flag = task.completed ? "[已完成]" : "[进行中]";
            string finishTime = task.completedAt.HasValue ? $" ({task.completedAt.Value:HH:mm})" : string.Empty;
            sb.AppendLine($"{flag} {task.employeeName}：对机台 {task.targetMachineId} 进行 PM{finishTime}");
        }

        taskListText.text = sb.ToString();
    }

    private void RotateDayIfNeeded(DateTime now)
    {
        if (currentDay == now.DayOfYear) return;

        currentDay = now.DayOfYear;
        currentTasks.Clear();
        RefreshTaskUI();
        SetStatus($"{now:MM/dd} 新的一天：08:30-09:00 早会，09:00 发布任务。18:30-19:00 晚会，19:00 汇总。");
    }

    private void GenerateTasksAfterMorningMeeting(DateTime now)
    {
        currentTasks.Clear();

        for (int i = 0; i < employeeRoster.Count; i++)
        {
            EmployeeConfig employee = employeeRoster[i];

            if (string.IsNullOrWhiteSpace(employee.employeeId) || string.IsNullOrWhiteSpace(employee.targetMachineId)) continue;

            currentTasks.Add(new DailyTask
            {
                employeeId = employee.employeeId.Trim(),
                employeeName = string.IsNullOrWhiteSpace(employee.displayName) 
                    ? employee.employeeId.Trim() 
                    : employee.displayName.Trim(),
                targetMachineId = employee.targetMachineId.Trim(),
                description = $"对机台 {employee.targetMachineId.Trim()} 进行 PM",
                completed = false,
                completedAt = null
            });
        }

        SetStatus($"📋 {now:HH:mm} 早会结束，今日任务已发布（可在任意办公室电脑查看）。");
        RefreshTaskUI();
    }

    private void RunEveningSummary(DateTime now)
    {
        int completed = 0;
        for (int i = 0; i < currentTasks.Count; i++)
        {
            if (currentTasks[i].completed) completed++;
        }

        SetStatus($"🕖 {now:HH:mm} 晚会结束汇总：今日完成 {completed}/{currentTasks.Count} 项。");
        RefreshTaskUI();
    }

    private string ResolveCurrentPlayerEmployeeId()
    {
        return ComputerLogin.ActiveEmployeeId;
    }

    private DateTime GetCurrentTime()
    {
        if (useSystemClock) return DateTime.Now;

        DateTime today = DateTime.Today;
        return new DateTime(today.Year, today.Month, today.Day, debugHour, debugMinute, 0);
    }

    private bool IsAfterOrEqual(DateTime now, int hour, int minute)
    {
        int cur = now.Hour * 60 + now.Minute;
        int target = hour * 60 + minute;
        return cur >= target;
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log($"[DailyTaskBoard] {msg}");
    }
}