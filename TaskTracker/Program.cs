using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class TaskItem
{
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
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
        Console.WriteLine("                 ADVANCED TASK MANAGER - V2 FEATURE           ");
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
        Console.Write("\nEnter task description: ");
        string? title = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            PauseMessage("Task description cannot be empty.");
            return;
        }

        Console.Write("Set Priority ([1] High, [2] Medium, [3] Low - default: 2): ");
        string? priorityChoice = Console.ReadLine()?.Trim();

        string priority = priorityChoice switch
        {
            "1" => "High",
            "3" => "Low",
            _ => "Medium"
        };

        tasks.Add(new TaskItem
        {
            Title = title,
            Priority = priority,
            IsCompleted = false,
            CreatedAt = DateTime.Now
        });

        SaveTasksToFile();
    }

    static void ToggleTask()
    {
        if (tasks.Count == 0) return;

        Console.Write("\nEnter task number to toggle status: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= tasks.Count)
        {
            tasks[index - 1].IsCompleted = !tasks[index - 1].IsCompleted;
            SaveTasksToFile();
        }
        else
        {
            PauseMessage("Invalid task number.");
        }
    }

    static void DeleteTask()
    {
        if (tasks.Count == 0) return;

        Console.Write("\nEnter task number to delete: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= tasks.Count)
        {
            tasks.RemoveAt(index - 1);
            SaveTasksToFile();
        }
        else
        {
            PauseMessage("Invalid task number.");
        }
    }

    static void SaveTasksToFile()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(tasks, options);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving tasks: {ex.Message}");
        }
    }

    static void LoadTasksFromFile()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                tasks = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
            }
        }
        catch
        {
            tasks = new List<TaskItem>();
        }
    }

    static void PauseMessage(string message)
    {
        Console.WriteLine($"\n{message}");
        Console.ReadKey();
    }
}