﻿using AISIGA.Program.Experiments;
using AISIGA.Program;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AISIGA.Program.Tests;

namespace AISIGA.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void StartProgram_Click(object sender, RoutedEventArgs e)
    {
        
        this.Hide();
        Tests.Run();
        // Initialize the experiment configuration & the controller
        AbstractExperimentConfig expConfig = new Experiment1_1();
        // Example: Show the dashboard
        DashboardWindow dashboard = new DashboardWindow();
        if (expConfig.UseUI)
        {
            dashboard.Show();
        }
        Master master = new Master(expConfig, dashboard);
        Task.Run(() =>
        {
            master.Initialize();
        });
    }
}