using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ApapterSamples;
namespace AdapterSamples
{
    public class SetTimer
    {
        
        public Timer TheTimer = new Timer();
 
        private int timerCount;
 
        // constructor
        public SetTimer(double interval, bool startNow)
        {
            timerCount = 0;
 
            // this is not really necessary, since the default value
            // of a new System.Timer's 'AutoReset Property is 'true: 
            // shown here only for educational value
            //
            // if AutoReset is set to 'false: the Elapsed EventHandler 
            // is called once: the System.Timer must be re-started to use again
            TheTimer.AutoReset = true;
 
            TheTimer.Interval = interval;
 
            TheTimer.Elapsed += new ElapsedEventHandler(TheTimer_Elapsed);
 
            TheTimer.Enabled = startNow;
            
        }
 
        public void TheTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
          //  DateTime dt = CallAdapter.chkHelloDateTime;
           
            //// test case
            //Console.WriteLine(timerCount);
            //timerCount++;
        }
 
        public void StartTimer()
        {
            TheTimer.Enabled = true;
        }
 
        public void StopTimer()
        {
            TheTimer.Enabled = false;
        }
    }
}
