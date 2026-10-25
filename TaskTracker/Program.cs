using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace TaskTrackerApp;

public class TaskItem
{
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public bool IsCompleted { get; set; } = false;
    public string? ImagePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public override string ToString()
    {
        string status = IsCompleted ? "[✓ DONE]" : "[PENDING]";
        string hasImg = string.IsNullOrEmpty(ImagePath) ? "" : " 🖼";
        return $"{status} [{Priority}] {Title}{hasImg}";
    }
}

public class MainForm : Form
{
    private List<TaskItem> tasks = new();
    private readonly ListBox taskListBox = new();
    private readonly TextBox titleInput = new();
    private readonly ComboBox priorityInput = new();
    private readonly Button selectImageButton = new();
    private readonly Button clearImageButton = new();
    private readonly Label selectedImageLabel = new();
    private readonly Button addButton = new();
    private readonly Button toggleButton = new();
    private readonly Button deleteButton = new();
    private readonly PictureBox taskPictureBox = new();
    private readonly Label previewHeaderLabel = new();

    private string? currentImagePath = null;
    private readonly string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tasks.json");

    public MainForm()
    {
        Text = "Task Tracker Desktop";
        Width = 740;
        Height = 560;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        // Title Input
        Label titleLabel = new() { Text = "Task Title:", Left = 20, Top = 20, Width = 80 };
        titleInput = new() { Left = 100, Top = 18, Width = 200 };

        // Priority Dropdown
        priorityInput = new() { Left = 310, Top = 18, Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
        priorityInput.Items.AddRange(new string[] { "High", "Medium", "Low" });
        priorityInput.SelectedIndex = 1;

        // Image Selection Controls
        selectImageButton = new() { Text = "Choose Image...", Left = 20, Top = 55, Width = 120, Height = 28 };
        selectImageButton.Click += SelectImage_Click;

        clearImageButton = new() { Text = "Clear", Left = 145, Top = 55, Width = 55, Height = 28 };
        clearImageButton.Click += (s, e) => ResetSelectedImage();

        selectedImageLabel = new() { Text = "No image selected", Left = 210, Top = 61, Width = 210, AutoEllipsis = true };

        // Add Button
        addButton = new() { Text = "Add Task", Left = 20, Top = 90, Width = 400, Height = 32 };
        addButton.Click += (s, e) => AddTask();
        AcceptButton = addButton;

        // Task List Box
        taskListBox = new() { Left = 20, Top = 135, Width = 400, Height = 310, Font = new Font("Segoe UI", 10) };
        taskListBox.SelectedIndexChanged += (s, e) => UpdateImagePreview();
        taskListBox.DoubleClick += (s, e) => ToggleTask();

        // Image Preview Area
        previewHeaderLabel = new() { Text = "Task Attachment:", Left = 440, Top = 20, Width = 260, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        taskPictureBox = new()
        {
            Left = 440,
            Top = 50,
            Width = 260,
            Height = 395,
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(245, 245, 245)
        };

        // Action Buttons
        toggleButton = new() { Text = "Toggle Complete", Left = 20, Top = 460, Width = 140, Height = 35 };
        toggleButton.Click += (s, e) => ToggleTask();

        deleteButton = new() { Text = "Delete Selected", Left = 170, Top = 460, Width = 140, Height = 35 };
        deleteButton.Click += (s, e) => DeleteTask();

        Controls.AddRange(new Control[] {
            titleLabel, titleInput, priorityInput,
            selectImageButton, clearImageButton, selectedImageLabel, addButton,
            taskListBox, previewHeaderLabel, taskPictureBox,
            toggleButton, deleteButton
        });

        LoadTasks();
        RefreshList();
    }

    private void SelectImage_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new()
        {
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*",
            Title = "Select Task Image"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            currentImagePath = dialog.FileName;
            selectedImageLabel.Text = Path.GetFileName(dialog.FileName);
        }
    }

    private void ResetSelectedImage()
    {
        currentImagePath = null;
        selectedImageLabel.Text = "No image selected";
    }

    private void AddTask()
    {
        if (string.IsNullOrWhiteSpace(titleInput.Text))
        {
            MessageBox.Show("Please enter a task title.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            titleInput.Focus();
            return;
        }

        tasks.Add(new TaskItem
        {
            Title = titleInput.Text.Trim(),
            Priority = priorityInput.SelectedItem?.ToString() ?? "Medium",
            ImagePath = currentImagePath,
            CreatedAt = DateTime.Now
        });

        titleInput.Clear();
        ResetSelectedImage();
        SaveTasks();
        RefreshList(selectedIndex: tasks.Count - 1);
    }

    private void UpdateImagePreview()
    {
        DisposePreviewImage();

        int index = taskListBox.SelectedIndex;
        if (index >= 0 && index < tasks.Count)
        {
            string? path = tasks[index].ImagePath;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    // Load into a memory stream to avoid file locking
                    using var stream = new MemoryStream(File.ReadAllBytes(path));
                    taskPictureBox.Image = Image.FromStream(stream);
                    return;
                }
                catch
                {
                    // Fall back to no image if file is corrupt/unreadable
                }
            }
        }

        taskPictureBox.Image = null;
    }

    private void DisposePreviewImage()
    {
        if (taskPictureBox.Image != null)
        {
            var oldImage = taskPictureBox.Image;
            taskPictureBox.Image = null;
            oldImage.Dispose();
        }
    }

    private void ToggleTask()
    {
        int index = taskListBox.SelectedIndex;
        if (index >= 0 && index < tasks.Count)
        {
            tasks[index].IsCompleted = !tasks[index].IsCompleted;
            SaveTasks();
            RefreshList(selectedIndex: index);
        }
    }

    private void DeleteTask()
    {
        int index = taskListBox.SelectedIndex;
        if (index >= 0 && index < tasks.Count)
        {
            tasks.RemoveAt(index);
            SaveTasks();
            int nextIndex = Math.Min(index, tasks.Count - 1);
            RefreshList(selectedIndex: nextIndex);
        }
    }

    private void RefreshList(int selectedIndex = -1)
    {
        taskListBox.BeginUpdate();
        taskListBox.Items.Clear();

        foreach (var task in tasks)
        {
            taskListBox.Items.Add(task);
        }

        if (selectedIndex >= 0 && selectedIndex < taskListBox.Items.Count)
        {
            taskListBox.SelectedIndex = selectedIndex;
        }
        else if (taskListBox.Items.Count > 0)
        {
            taskListBox.SelectedIndex = 0;
        }
        else
        {
            DisposePreviewImage();
        }

        taskListBox.EndUpdate();
        UpdateImagePreview();
    }

    private void SaveTasks()
    {
        try
        {
            string json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
            string tempFile = filePath + ".tmp";
            File.WriteAllText(tempFile, json);
            File.Move(tempFile, filePath, overwrite: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save tasks: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadTasks()
    {
        if (!File.Exists(filePath)) return;

        try
        {
            string json = File.ReadAllText(filePath);
            tasks = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new();
        }
        catch
        {
            tasks = new();
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        DisposePreviewImage();
        base.OnFormClosed(e);
    }

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}