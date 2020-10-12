using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApapterSamples;
namespace AdapterSamples
{
    public partial class ResetApp : Form
    {
        CallAdapter callad = new CallAdapter();
        public ResetApp()
        {

            InitializeComponent();
        }


        public void methodRestart()
        {
           // Application.Restart();
            Environment.Exit(0);
        }

    }
}
