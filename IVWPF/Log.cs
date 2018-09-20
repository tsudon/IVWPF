using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace IVWIN
{
    class LogWriter : INotifyPropertyChanged
    {
        long ticks;
        public static string Text="Log... \n";
        private static TextBlock _TextBlock;

        public LogWriter()
        {
            ticks = DateTimeOffset.Now.Ticks;
        }

        static public void SetTextBlock(TextBlock block)
        {
            _TextBlock = block;
            _TextBlock.Inlines.Add(Text);
        }


        static public void write(String str)
        {

            if (_TextBlock != null)
            {
                _TextBlock.Inlines.Add($"{str}\n");
            }
            else
            {
                Console.WriteLine(str);
            }
        }

        public void time()
        {
            long t = DateTimeOffset.Now.Ticks;
            LogWriter.write("" + (double)((t - ticks)/10000.0)+"ms" );
            ticks = t;
        }

        static public void TIME ()
        {
            LogWriter.write("" +DateTimeOffset.Now.Ticks);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
