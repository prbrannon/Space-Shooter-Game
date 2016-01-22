using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game2D
{
    public class Log
    {
        public static bool logging = true;
        static System.IO.StreamWriter log_stream;

        public static void open(String file_name)
        {
            log_stream = new System.IO.StreamWriter(file_name);
        }

        public static void write_line(String message)
        {
            if (logging)
            {
                log_stream.WriteLine(message);
                log_stream.Flush();
            }
        }

        public static bool Logging
        {
            set { logging = value; }
            get { return logging; }
        }
    }
}
