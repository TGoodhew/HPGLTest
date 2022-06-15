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

            resManager = new ResourceManager();

            gpibSession = (GpibSession)resManager.Open(@"GPIB0::6::INSTR");
            gpibSession.TimeoutMilliseconds = 20000; // Set the timeout to be 20s
            gpibSession.TerminationCharacterEnabled = true;

            gpibSession.Clear();

            string hpgl = System.IO.File.ReadAllText(@"Shuttle-HPGL.txt");

            string[] hpglCommands = Regex.Split(hpgl, @"(?<=[;])");

            foreach (var cmd in hpglCommands)
            {
                gpibSession.FormattedIO.WriteLine(cmd);

                Console.WriteLine(cmd);
            }
        }
    }
}
