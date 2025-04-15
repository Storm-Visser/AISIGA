using AISIGA.Program;
using AISIGA.Program.Experiments;
using System.Configuration;
using System.Data;
using System.Windows;

namespace AISIGA;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    //Start point of the code
    //Select the experiment to run here
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        #if DEBUG
                // Run your test class here
                var test = new MutationCrossoverTests();
                test.Run();

                Console.WriteLine("Press any key to start default program");
                Console.ReadKey(); // Pause so you can read output
        #endif
        // Initialize the experiment configuration & the controller
        ExperimentConfig expConfig = new TestConfig();
        Master master = new Master(expConfig);
        master.Initialize();
        // Set the startup window
        MainWindow = new MainWindow();
        MainWindow.Show();
    }
}

