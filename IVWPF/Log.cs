using System;
using System.Collections.Generic;
using System.Text;

namespace IVWIN
{
    class LogWritter
    {
        long ticks;

        public LogWritter()
        {
            ticks = DateTimeOffset.Now.Ticks;
        }

        static public void write (String str)
        {
           Console.WriteLine(str);
        }

        public void time()
        {
            long t = DateTimeOffset.Now.Ticks;
            Console.WriteLine( (double)((t - ticks)/10000.0)+"ms" );
            ticks = t;
        }



        static public void TIME ()
        {
            Console.WriteLine(DateTimeOffset.Now.Ticks);
        }
    }
}
