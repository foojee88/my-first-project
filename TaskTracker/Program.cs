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
}

class Program
{
    private static readonly string FilePath = "tasks.json";
    private static List<TaskItem> tasks = new List<TaskItem>();

    static void Main(string[] args)
    {
        LoadTasksFromFile();

        while (true)
        {
            Console.Clear();
            DisplayHeader();
            DisplayTasks();

            Console.WriteLine("\n[1] Add Task");
            Console.WriteLine("[2] Toggle Complete / Incomplete");
            Console.WriteLine("[3] Delete Task");
            Console.WriteLine("[4] Save & Exit");
            Console.Write("\nSelect an option: ");

            string? choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    AddTask();
                    break;
                case "2":
                    ToggleTask();
                    break;
                case "3":
                    DeleteTask();
                    break;
                case "4":
                    SaveTasksToFile();
                    Console.WriteLine("\nTasks saved successfully. Goodbye!");
                    return;
                default:
                    PauseMessage("Invalid selection. Press any key to try again...");
                    break;
            }
        }
    }

    static void DisplayHeader()
    {
        Console.WriteLine("==============================================================");
        Console.WriteLine("                 ADVANCED TASK MANAGER                        ");
        Console.WriteLine("==============================================================");
    }

    static void DisplayTasks()
    {
        if (tasks.Count == 0)
        {
            Console.WriteLine("  No tasks found. Press [1] to add your first task.");
            return;
        }

        Console.WriteLine($" {"#",-3} | {"Status",-8} | {"Priority",-8} | {"Date Added",-12} | Title");
        Console.WriteLine(new string('-', 62));

        for (int i = 0; i < tasks.Count; i++)
        {
            var task = tasks[i];
            string status = task.IsCompleted ? "[DONE]" : "[TODO]";
            string dateStr = task.CreatedAt.ToString("yyyy-MM-dd");

            Console.WriteLine($" {i + 1,-3} | {status,-8} | {task.Priority,-8} | {dateStr,-12} | {task.Title}");
        }
    }

    static void AddTask()
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