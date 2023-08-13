using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HPGLTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GpibSession gpibSession;
            ResourceManager resManager;

            // Setup the console
            Output.SetupConsole();

            // Setup the GPIB connection via the ResourceManager
            resManager = new ResourceManager();

            // Connect to the plotter and configure connection
            gpibSession = (GpibSession)resManager.Open(@"GPIB0::5::INSTR");
            gpibSession.TimeoutMilliseconds = 20000; // Set the timeout to be 20s
            gpibSession.TerminationCharacterEnabled = true;

            // Clear the session
            gpibSession.Clear();

            // Start the plotting process
            Output.Heading("HP 7090A Measurement System Plotter - Samples", true);
            Output.Information("This program produces two samples - An Imperial Walker in the default color and a colored version of the Space Shuttle");

            // Ensure paper loaded
            Output.Prompt("Load the paper");

            // Get the first model
            string hpgl = System.IO.File.ReadAllText(@"ImperialWalker-HPGL.PLT");

            // Split the commands up so they can be sent individually to the plotter
            string[] hpglCommands = Regex.Split(hpgl, @"(?<=[;])");

            // Send the commands
            foreach (var cmd in hpglCommands)
            {
                gpibSession.FormattedIO.WriteLine(cmd);

                Console.WriteLine(cmd);
            }

            // HP 7550A - Automatically get new paper
            gpibSession.FormattedIO.WriteLine("PG 1;");

            // Flip the paper over
            // Output.Prompt("Flip the paper");

            // Get the second model
            hpgl = System.IO.File.ReadAllText(@"columbia-Model-SHPGL.PLT");

            // Split the commands up so they can be sent individually to the plotter
            hpglCommands = Regex.Split(hpgl, @"(?<=[;])");

            // Send the commands
            foreach (var cmd in hpglCommands)
            {
                gpibSession.FormattedIO.WriteLine(cmd);

                Console.WriteLine(cmd);
            }

            // HP 7550A - Unload the paper
            gpibSession.FormattedIO.WriteLine("NR");
            
            // Plotting complete
            Output.Prompt("Plotting complete");
        }
    }
}
