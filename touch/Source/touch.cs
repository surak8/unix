using System;
using System.IO;

namespace touch {
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    class Class1 {

        private static void touchFile(string fname) {
            try {
                if (File.Exists(fname)) {
                    File.SetLastAccessTime(fname,DateTime.Now);
                    File.SetLastWriteTime(fname,DateTime.Now);
                } else {
                    File.Create(fname);
                }
            } catch (Exception ex) {
                Console.Error.WriteLine("{0}:\n{1}{2}\n{3}",
                    ex.GetType().FullName,
                    ex.Message,
                    ex.InnerException==null?string.Empty:
                    " ["+ex.InnerException.Message+"]",
                    ex.StackTrace);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            if (args==null || args.Length<0)
                Console.Error.WriteLine("no args");
            else {
                foreach (string arg in args)
                    touchFile(arg);
            }
        }
    }
}