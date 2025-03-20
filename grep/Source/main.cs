using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;

using System.Text;

//using System.IO;
#region assembly-level attributes
// start Assembly-level attributes.
[assembly:AssemblyProduct("grep")]
[assembly:AssemblyTitle("grep")]
[assembly:AssemblyVersion("1.0.0.1")]
[assembly:AssemblyFileVersion("1.0.0.1")]
[assembly:AssemblyInformationalVersion("1.0.0.1")]
[assembly:AssemblyCompany("Rik Cousens")]
[assembly:AssemblyCopyright("Copyright(c) 2005-2025, Rik Cousens")]
[assembly:AssemblyDescription("unix 'grep' utility, implemented in .Net.")]
#if DEBUG
	[assembly:AssemblyConfiguration("Debug version")]
#else
	[assembly:AssemblyConfiguration("Release version")]
#endif
#endregion assembly-level attributes

// end Assembly-level attributes.

namespace grep {

    #region internal class GrepOpts
    internal class GrepOpts {
        private bool _bIgnoreCase,_bLineNumbers,_bFilename,_bExtraLine;

        public GrepOpts() {
            _bIgnoreCase=_bLineNumbers=_bFilename=_bExtraLine=false;
        }

        public bool ignoreCase { 
            get { return _bIgnoreCase; }
            set { _bIgnoreCase=value; }
        }

        public bool lineNumbers { 
            get { return _bLineNumbers; }
            set { _bLineNumbers=value; }
        }

        public bool fileName { 
            get { return _bFilename; }
            set { _bFilename=value; }
        }

        public bool extraLine { 
            get { return _bExtraLine; }
            set { _bExtraLine=value; }
        }
    }
    #endregion internal class GrepOpts

    // Namespace comment
    /// Namespace comment
    public class main {

        private static bool splitFileDescription(string szDesc,out string szPath,out string szWC) {
            bool ret=false;
            int nPos;

            szPath=szWC=null;
            if (szDesc!=null && szDesc!=string.Empty) {
                if ((nPos=szDesc.LastIndexOf('\\'))>=0) {
                    // have a path here.
                    string szTmp=szDesc.Substring(0,nPos);
                    szPath=Path.GetFullPath(szTmp);
                    szWC=szDesc.Substring(nPos+1);
                } else {
                    // have wildcard only
                    szPath=Directory.GetCurrentDirectory();
                    szWC=szDesc;
                }
                ret=true;
            }
            return ret;
        }


        private static bool validString(string szStr) {
            return szStr!=null && szStr!=string.Empty && szStr.Length>0;
        }

        private static int grepText(string szFile,string szText,string szRegex,GrepOpts go,bool bMultiFile) {
            int ret=0;

            if (validString(szText) && validString(szRegex)) {
                string[] aszLines=szText.Split(new char[] {'\n'});
                int nLines=(aszLines==null?0:aszLines.Length);
                bool bWroteFilename=false,bNeedNL=false;

                if (nLines>0) {
                    Match m;
                    RegexOptions ro=go.ignoreCase?RegexOptions.IgnoreCase:RegexOptions.None;

                    for (int i=0;i<nLines;i++) {
                        if ((m=Regex.Match(aszLines[i],szRegex,ro))!=Match.Empty) {
                            ret++;
                            if (bMultiFile || go.fileName) {
                                if (!bWroteFilename) { 
                                    bWroteFilename=true;
                                    Console.WriteLine("file {0}:{1}",szFile,go.extraLine?"\n":"");
                                }
                            }
                            if (go.lineNumbers && !go.fileName) {
                                Console.Write("{0}:",i);
                            }
                            if (!(go.fileName && go.lineNumbers)) {
                                Console.WriteLine(aszLines[i]);
                                bNeedNL=true;
                            }
                        }
                    }
                    if (/*go.extraLine && */ret>0 && bNeedNL)
                        Console.WriteLine();
                }
            } else {
                if (!validString(szText))
                    Console.Error.WriteLine("file {0} is empty?",szFile);
                if (!validString(szRegex))
                    Console.Error.WriteLine("invalid regex here");

//                Console.Error.WriteLine("invalid string or regexp here!");
            }
            return ret;
        }

        private static int doGrep(string szFile,string szRegex,GrepOpts go,bool bMultiFile) {
            int ret=0;
            StreamReader sr=null;

            try {
                sr=new StreamReader(szFile);
            } catch (Exception ex) {
                Console.Error.WriteLine("StreamReader({0}) failed [{1}]!",
                    szFile,ex.Message);
            }
            if (sr!=null) {
                string szContent=sr.ReadToEnd();

                //				go.fileName=szFile;
                sr.Close();
                ret=grepText(szFile,szContent,szRegex,go,bMultiFile);
            }
            return ret;
        }

        private static int grepStuff(string szRegex,string szFileDesc,GrepOpts go) {
            int ret=0;
            string szPath,szWildcard;

            Trace.WriteLine(string.Format("regex='{0}', filedes='{1}'",szRegex,szFileDesc));
            if (splitFileDescription(szFileDesc,out szPath,out szWildcard)) {
                if (Directory.Exists(szPath)) {
                    string[] files=Directory.GetFiles(szPath,szWildcard);

                    if (files==null)
                        Console.Error.WriteLine("{0}: no files matching '{1}'.",szPath,szWildcard);
                    else {
                        foreach (string szFile in files)
                            ret+=doGrep(szFile,szRegex,go,files.Length>1);
                    }
                } else
                    Console.Error.WriteLine("Directory {0} does not exist!",szPath);
            }
            return ret;
        }


        private static string buildDisplayString(Assembly asm) {
            object[] aAttrs=asm.GetCustomAttributes(false);
            StringBuilder sb=new StringBuilder();

            if (aAttrs!=null) {
                string szProd,szVersion,szCopy,szDesc,szCfg;

                szProd=szVersion=szCopy=szDesc=szCfg=null;
                foreach (Attribute a in aAttrs) {
                    if (a is AssemblyProductAttribute)
                        szProd=((AssemblyProductAttribute) a).Product;
                    else if (a is AssemblyVersionAttribute)
                        szVersion=((AssemblyVersionAttribute) a).Version.ToString();
                    else if (a is AssemblyFileVersionAttribute)
                        szVersion=((AssemblyFileVersionAttribute) a).Version;
                    else if (a is AssemblyInformationalVersionAttribute)
                        szVersion=((AssemblyInformationalVersionAttribute) a).InformationalVersion;
                    else if (a is AssemblyCopyrightAttribute)
                        szCopy=((AssemblyCopyrightAttribute) a).Copyright;
                    else if (a is AssemblyDescriptionAttribute)
                        szDesc=((AssemblyDescriptionAttribute) a).Description;
                    else if (a is AssemblyConfigurationAttribute)
                        szCfg=((AssemblyConfigurationAttribute) a).Configuration;
                }
                sb.AppendFormat("{0} {1} [{2}]\n",
                    validString(szProd)?szProd:"grep",
                    validString(szVersion)?szVersion:"????????",
                    validString(szCfg)?szCfg:
#if DEBUG
                    "DEBUG"
#else
					"RELEASE"
#endif
                    );
                if (szCopy!=null)
                    sb.AppendFormat("{0}\n",szCopy);
                if (szDesc!=null)
                    sb.AppendFormat("{0}\n",szDesc);
            }
            return sb.ToString();
        }

        private static void usage(int nExitCode) {
            Assembly a=Assembly.GetEntryAssembly();

            Console.WriteLine(buildDisplayString(a));
            Console.WriteLine("usage: {0} -[hilnvx?] reg-exp file [...file]",Path.GetFileNameWithoutExtension(Path.GetFullPath(a.Location)));
            Environment.Exit(nExitCode);
        }

        // entry-point comment
        /// entry-point comment
        public static void Main(string[] args) {
            int nArgs,nExitCode;
            bool bShowUsage,bShowVersion,bTrace;
            GrepOpts go;

            go=new GrepOpts();
            nArgs=args==null?0:args.Length;
            nExitCode=0;
            bShowUsage=false;
            bShowVersion=false;
            bTrace=false;

            if (nArgs>0) {
                int c;
                while ((c=GetOpt.getopt(args,"hilntv?"))>=0) {
                    switch(c) {
                        case 'i': go.ignoreCase=true; break;
                        case 'l': go.lineNumbers=true; break;
                        case 'n': go.fileName=true; break;
                        case 'v': bShowVersion=!bShowVersion; break;
                        case 't': bTrace=true; break;
                        case 'x': go.extraLine=true; break;
                        case 'h': 
                        case '?': nExitCode=0; bShowUsage=true; break;
                        default:
                            nExitCode=1; 
                            bShowUsage=true;
                            break;
                    }
                }
            }
            if (bShowUsage) {
                usage(nExitCode);
            } else {
                if (GetOpt.optind>=nArgs)
                    usage(1);
                else {
                    string szRegex;
                    int i,nFound=0;
#if TRACE
                    TextWriterTraceListener twtl;

                    twtl=null;
                    Trace.AutoFlush=true;
                    if (bTrace) {
                        try {
                            twtl=new TextWriterTraceListener(Console.Error);
                            Trace.Listeners.Add(twtl);
                        } catch (Exception ex) {
                            Console.Error.WriteLine("{0}\n{1}{2}\n{3}",
                                ex.GetType().FullName,
                                ex.Message,
                                ex.InnerException==null?string.Empty:" ["+ex.InnerException.Message+"]",
                                ex.StackTrace);
                        }
                    }
#endif

                    if (bShowVersion)
                        Console.WriteLine(buildDisplayString(Assembly.GetExecutingAssembly()));
                    i=GetOpt.optind;
                    szRegex=args[i++];
                    if (i>=nArgs) {
                        Console.Error.WriteLine("reading 'stdin' for regex['{0}'].",szRegex);
                        nFound=grepText("stdin",Console.In.ReadToEnd(),szRegex,go,false);
                        nExitCode=nFound>0?0:2;
                    } else {
                        for (;i<nArgs;i++)
                            nFound+=grepStuff(szRegex,args[i],go);
                        nExitCode=nFound>0?0:2;
                    }
#if TRACE
                    if (bTrace) {
                        if (twtl!=null) {
                            twtl.Flush();
                            Trace.Listeners.Remove(twtl);
                        }
                    }
#endif
                }
            }
            Environment.Exit(nExitCode);
        }
    }
}