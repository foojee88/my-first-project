using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Drawing;

namespace TaskTrackerApp;

public class TaskItem
{
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public override string ToString()
    {
        string status = IsCompleted ? "[✓ DONE]" : "[PENDING]";
        return $"{status} ({Priority}) - {Title}";
    }
}

public class MainForm : Form
{
    private List<TaskItem> tasks = new();
    private ListBox taskListBox = new();
    private TextBox titleInput = new();
    private ComboBox priorityInput = new();
    private Button addButton = new();
    private Button toggleButton = new();
    private Button deleteButton = new();
    private string filePath = "tasks.json";

    public MainForm()
    {
        Text = "Task Tracker Desktop";
        Width = 520;
        Height = 550;
        StartPosition = FormStartPosition.CenterScreen;

        // Title Label & Box
        Label titleLabel = new() { Text = "Task Title:", Left = 20, Top = 20, Width = 80 };
        titleInput = new() { Left = 110, Top = 18, Width = 230 };

        // Priority Dropdown
        priorityInput = new() { Left = 350, Top = 18, Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
        priorityInput.Items.AddRange(new string[] { "High", "Medium", "Low" });
        priorityInput.SelectedIndex = 1;

        // Add Button
        addButton = new() { Text = "Add Task", Left = 110, Top = 55, Width = 110, Height = 32 };
        addButton.Click += (s, e) => AddTask();

        // Task List View
        taskListBox = new() { Left = 20, Top = 100, Width = 460, Height = 320, Font = new Font("Segoe UI", 10) };

        // Action Buttons
        toggleButton = new() { Text = "Toggle Complete", Left = 20, Top = 440, Width = 140, Height = 35 };
        toggleButton.Click += (s, e) => ToggleTask();

        deleteButton = new() { Text = "Delete Selected", Left = 180, Top = 440, Width = 140, Height = 35 };
        deleteButton.Click += (s, e) => DeleteTask();

        // Add controls to the Form
        Controls.Add(titleLabel);
        Controls.Add(titleInput);
        Controls.Add(priorityInput);
        Controls.Add(addButton);
        Controls.Add(taskListBox);
        Controls.Add(toggleButton);
        Controls.Add(deleteButton);

        LoadTasks();
        RefreshList();
    }

    private void AddTask()
    {
        if (string.IsNullOrWhiteSpace(titleInput.Text))
        {
            MessageBox.Show("Please enter a title.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        tasks.Add(new TaskItem 
        { 
            Title = titleInput.Text.Trim(), 
            Priority = priorityInput.SelectedItem?.ToString() ?? "Medium" 
        });

        titleInput.Clear();
        SaveTasks();
        RefreshList();
    }

    private void ToggleTask()
    {
        if (taskListBox.SelectedIndex >= 0)
        {
            tasks[taskListBox.SelectedIndex].IsCompleted = !tasks[taskListBox.SelectedIndex].IsCompleted;
            SaveTasks();
            RefreshList();
        }
    }

    private void DeleteTask()
    {
        if (taskListBox.SelectedIndex >= 0)
        {
            tasks.RemoveAt(taskListBox.SelectedIndex);
            SaveTasks();
            RefreshList();
        }
    }

    private void RefreshList()
    {
        taskListBox.Items.Clear();
        foreach (var task in tasks)
        {
            taskListBox.Items.Add(task.ToString());
        }
    }

    private void SaveTasks()
    {
        string json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    private void LoadTasks()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            tasks = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new();
        }
    }

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}