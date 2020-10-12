using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdmsAdapter;
using NetUtil;
using System.Windows.Forms;
using System.Diagnostics;

namespace ApapterSamples
{
    class Program
    {
        /// <summary>
        /// The main program, will call either the CrewAdapter or CallAdapter to do its test
        /// To use the program, pass the following arguments in command line
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
           
            //CrewAdapter.TestCrewMessages(args);
            CallAdapter.TestCallMessages(args);
            string anyline = Console.ReadLine();   // read anyline to finish the program
        }

    }
}