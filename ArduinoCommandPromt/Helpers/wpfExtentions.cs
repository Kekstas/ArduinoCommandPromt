using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCommandPromt.Helpers
{
    public static class WpfExtentions
    {
        public static void Load(this System.Windows.Controls.ListBox list, string filePath)
        {
            using (var stream = File.OpenRead(filePath))  // open file
            using (var reader = new StreamReader(stream))   // read the stream with TextReader
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    list.Items.Add(line);
                }
            }
        }
    }
}
