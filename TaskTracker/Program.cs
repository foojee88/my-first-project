using System;
using System.Collections.Generic;

class Program
{
    static List<string> tasks = new List<string>();

    static void Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== TASK TRACKER ===");
            for (int i = 0; i < tasks.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {tasks[i]}");
            }
            if (tasks.Count == 0)
            {
                Console.WriteLine("(No tasks yet)");
            }

            Console.WriteLine("\n[1] Add Task  [2] Exit");
            Console.Write("Select an option: ");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("Enter task description: ");
                string task = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(task))
                {
                    tasks.Add(task);
                }
            }
            else if (choice == "2")
            {
                break;
            }
        }
    }
}