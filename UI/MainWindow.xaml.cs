using AISIGA.Program.Experiments;
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
        // Example: Show the dashboard
        DashboardWindow dashboard = new DashboardWindow();
        dashboard.Show();
        // Optional: Hide main window
        this.Hide();

        // Initialize the experiment configuration & the controller
        ExperimentConfig expConfig = new TestConfig();
        Master master = new Master(expConfig, dashboard);
        Task.Run(() =>
        {
            master.Initialize();
        });
    }
}