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
        private static TextBox TextBox;

        public LogWriter()
        {
            ticks = DateTimeOffset.Now.Ticks;
        }

        static public void SetTextBox(TextBox box)
        {
            TextBox = box;
            TextBox.Text = LogWriter.Text;
            TextBox.ScrollToEnd();
        }


        static public void write(String str)
        {
            LogWriter.Text += str + "\n";

            if (TextBox != null)
            {
                TextBox.Text = LogWriter.Text;
                TextBox.ScrollToEnd();
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
