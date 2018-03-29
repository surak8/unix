using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

// \\colt-sql\export\bkp\HGKanban*_2017-07-10*.csv

namespace NSWc {
    /// <summary>Sample implementation of wc utility.</summary>
    public class gnu_wc {
        // https://www.gnu.org/software/cflow/manual/html_node/Source-of-wc-command.html 

        #region fields
        bool countAll = true;
        bool charCount;
        bool lineCount;
        bool wordCount;

        /// <summary>number of characters for the current file.</summary>    
        long ccount;
        /// <summary>number of words for the current file.</summary>
        long wcount;
        /// <summary>number of lines for the current file.</summary>
        long lcount;

        /// <summary>total number of characters.</summary>
        long total_ccount = 0;
        /// <summary>total number of words.</summary>
        long total_wcount = 0;
        /// <summary>total number of lines.</summary>
        long total_lcount = 0; 
        #endregion

        /// <summary>Print error message and exit with error status.</summary>
        /// <param name="perr">a <see cref="bool"/></param> 
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        /// <remarks><para>If PERR is <b>true</b>, display current errno status.</para></remarks> 
        static void error_print(bool perr, string fmt, params object[] args) {
            Console.Error.WriteLine(fmt, args);
            if (perr)
                Console.Error.Write(" ");
            else
                Console.Error.WriteLine();
            Environment.Exit(1);
        }

        /// <summary>Print error message and exit with error status.</summary>
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        static void errf(string fmt, params object[] args) {
            error_print(false, fmt, args);
        }

        /*  */
        /// <summary>Print error message followed by errno status and exit with error code.</summary>
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        static void perrf(string fmt, params object[] args) {
            error_print(true, fmt, args);
        }

        /// <summary>Output counters for given file</summary>
        /// <param name="file"></param>
        /// <param name="ccount"></param>
        /// <param name="wcount"></param>
        /// <param name="lcount"></param>
        void report(string file, long ccount, long wcount, long lcount) {
            if (countAll)
                Console.WriteLine("{0,6:#####0} {1,6:#####0} {2,6:#####0} {3}", lcount, wcount, ccount, file);
            else {
                const string FMT = "{0,6:#####0} ";
            Console.WriteLine("figure out what to write, here.");
                if (lineCount)
                    Console.Write(FMT,lcount);
                if (wordCount)
                    Console.Write(FMT,wcount);
                if (charCount)
                    Console.Write(FMT,ccount);
                Console.WriteLine(file);
            }
        }

        /// <summary>Return true if C is a valid word constituent</summary>
        /// <param name="c"></param>
        /// <returns></returns>
        static bool isword(int c) {
            return char.IsLetterOrDigit(Convert.ToChar(c));
            //            return isalpha(c);
        }

        void COUNT(int c) {
            ccount++;
            if (c == '\n')
                lcount++;
        }

        /*  */
        /// <summary>Get next word from the input stream./// </summary>
        /// <param name="fp"></param>
        /// <returns><b>false</b>on end-of-file or error condition. <b>true</b>otherwise.</returns>
        bool getword(TextReader fp) {
            int c;

            while ((c = fp.Read()) > 0) {
                if (isword(c)) {
                    wcount++;
                    break;
                }
                COUNT(c);
            }

            for (; c > 0; c = fp.Read()) {
                COUNT(c);
                if (!isword(c))
                    break;
            }
            return c > 0;
        }

        /// <summary>
        /// Process file 'file'.
        /// </summary>
        /// <param name="file"></param>
        void counter(string file) {
            TextReader fp;

            if (File.Exists(file)) {
                fp = new StreamReader(file);

                if (fp == null)
                    perrf("cannot open file `{0}'", file);

                ccount = wcount = lcount = 0;
                while (getword(fp))
                    ;
                fp.Close();

                report(file, ccount, wcount, lcount);
                total_ccount += ccount;
                total_wcount += wcount;
                total_lcount += lcount;
            } else
                Console.Error.WriteLine("non-existent: " + file);
        }

        [STAThread]
        public static void Main(string[] args) {
            if (args.Length < 1)
                errf("usage: wc FILE [FILE...]");

            new gnu_wc().run(args);
            Environment.Exit(0);
        }


        void run(string[] args) {
            int pStar, nfiles = 0, len;
            char c;
            List<string> newArgs = new List<string>();

            countAll = true;
            charCount = lineCount = wordCount = false;
            foreach (string anArg in args) {
                if ((len = anArg.Length) >= 2 && ((c = anArg[0]) == '-' || c == '/')) {
                    //                 Debug.Print("here");
                    for (int i = 1; i < len; i++) {
                        switch (anArg[i]) {
                            case 'c': charCount = true; break;
                            case 'l': lineCount = true; break;
                            case 'w': wordCount = true; break;
                            default: Console.Error.WriteLine("unhandled flag: -" + anArg[i]); break;
                        }
                    }
                } else {
                    if (anArg.Contains("*") || anArg.Contains("?")) {
                        pStar = anArg.IndexOfAny(new char[] { '*', '?' });
                        while (pStar > 0) {
                            if (anArg[pStar] != '\\')
                                pStar--;
                            else
                                break;
                        }
                        foreach (string aFile in Directory.EnumerateFiles(anArg.Substring(0, pStar), anArg.Substring(pStar + 1))) {
                            newArgs.Add(aFile);
                            //counter(aFile);
                            //nfiles++;
                        }
                    } else {
                        newArgs.Add(anArg);
                        //counter(anArg);
                        //nfiles++;
                    }
                }
            }
            if ((nfiles = newArgs.Count) > 0) {
                if (charCount || lineCount || wordCount)
                    countAll = false;
                foreach (string aFile in newArgs)
                    counter(aFile);
            }

            if (nfiles > 1)
                report("total", total_ccount, total_wcount, total_lcount);
        }
    }
}