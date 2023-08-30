using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Spectre.Console;
using System.Diagnostics.Eventing.Reader;

namespace HPGLTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GpibSession gpibSession;
            ResourceManager resManager;
            bool autoFeedAvailable = false;
            int gpibIntAddress = 6;

            // Setup the GPIB connection via the ResourceManager
            resManager = new ResourceManager();

            while (true)
            {
                // Start the plotting process
                AnsiConsole.MarkupLine("[bold lime]HP 7090A Measurement System Plotter - Samples[/]");
                AnsiConsole.MarkupLine("[bold]This program produces two samples - An Imperial Walker in the default color and a colored version of the Space Shuttle\r\n\r\n[/]");

                // Select the plotter
                var plotter = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select the target plotter or Exit?")
                        .PageSize(4)
                        .AddChoices(new[] { "HP 7090A", "HP 7475A", "HP 7550A", "Exit" }));

                // Exit the demo if selected
                if (plotter == "Exit")
                    break;

                // Get the GPIB address for the plotter
                gpibIntAddress = AnsiConsole.Prompt(
                new TextPrompt<int>("Enter the GPIB address for the plotter?")
                    .PromptStyle("green")
                    .DefaultValue(6)
                    .ShowDefaultValue(true)
                    .ValidationErrorMessage("[red]Invalid address[/]")
                    .Validate(address =>
                    {
                        if ((address < 0) || (address > 30))
                        {
                            ValidationResult.Error("[red]The GPIB Address must be between 0 and 30[/]");
                            return false;
                        }
                        else ValidationResult.Success();
                        
                        return true;
                    }));

                // Echo the plotter back to the terminal
                AnsiConsole.WriteLine("You selected a {0} at GPIB address {1}\r\n\r\n",plotter, gpibIntAddress);

                // Connect to the plotter and configure connection
                gpibSession = (GpibSession)resManager.Open(string.Format("GPIB0::{0}::INSTR", gpibIntAddress));
                gpibSession.TimeoutMilliseconds = 4000; // Set the timeout to be 20s
                gpibSession.TerminationCharacterEnabled = true;

                // Clear the session
                gpibSession.Clear();

                // Set variables to handle plotter differences.
                // I believe that the 7090A & 7475A are functionally identical w.r.t basic plotting. The 7550A has auto-feed so it doesn't need the paper prompt
                switch (plotter)
                {
                    case "HP 7090A":
                    case "HP 7475A":
                        autoFeedAvailable = false;
                        break;
                    case "HP 7550A":
                        autoFeedAvailable = true;
                        break;
                    default:
                        break;
                }

                // Ensure paper loaded
                if (autoFeedAvailable) 
                {
                    // HP 7550A - Automatically get new paper
                    gpibSession.FormattedIO.WriteLine("PG 1;");
                }
                else
                {
                    AnsiConsole.MarkupLine("[bold red]Load the paper into the plotter and hit any key to continue[/]");
                    Console.ReadKey(true);
                }

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

                // Ensure paper loaded
                if (autoFeedAvailable)
                {
                    // HP 7550A - Automatically get new paper
                    gpibSession.FormattedIO.WriteLine("PG 1;");
                }
                else
                {
                    // Flip the paper over
                    AnsiConsole.MarkupLine("[bold red]Load new paper and hit any key to continue[/]");
                    Console.ReadKey(true);
                }

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
                if (autoFeedAvailable)
                {
                    gpibSession.FormattedIO.WriteLine("NR");
                }

                // Plotting complete - Check to see if the user wants the demo to run again
                if (!AnsiConsole.Confirm("Would you like to run the demo again?"))
                {
                    AnsiConsole.MarkupLine("Exiting...");
                    return;
                }

                gpibSession.Dispose();
                AnsiConsole.Clear();
            }
        }
    }
}
