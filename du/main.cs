using System;
using System.IO;
using System.Reflection;
using System.Collections;

// start Assembly-level attributes.
[assembly:AssemblyProduct("du")]
[assembly:AssemblyTitle("du")]
[assembly:AssemblyVersion("1.0.0.0")]
[assembly:AssemblyCompany("Phibro Inc.")]
// end Assembly-level attributes.

// Namespace comment
/// Namespace comment
public class main {

    private static void usage(int exitCode) {
        string szApp=Assembly.GetEntryAssembly().Location;

        Console.Error.WriteLine("usage: {0} -[adhsS] dir [..dir]\r\n",
            szApp);
		Console.Error.WriteLine("-a\tcalculate space-usage for all files");
		Console.Error.WriteLine("-d\tshow directory-totals.");
		Console.Error.WriteLine("-s\tadd subtotals");
		Console.Error.WriteLine("-S\tsort results based on size.");

		Console.Error.WriteLine("-h\tthis help-message.");
		Console.Error.WriteLine("-?\tthis help-message.");
		Environment.Exit(exitCode);

    }

    // entry-point comment
    /// entry-point comment
    public static void Main(string[] args) {
        int nArgs;
        bool bSortSize=false;

        if (args!=null && (nArgs=args.Length)>0) {
            int c;
            bool bSubtotal,bAll,bDirsOnly;

            bSubtotal=bAll=bDirsOnly=false;
            //            bDirsOnly=true;

            while ((c=GetOpt.getopt(args,"adhsS"))>=0) {
                switch(c) {
                    case 'a': bAll=true; break;
                    case 'd': bDirsOnly=!bDirsOnly; break;
                    case 'h': usage(0); break;
                    case 's': bSubtotal=true; break;
                    case 'S': bSortSize=true; break;
                    default: usage(1); break;
                }
            }
            if (GetOpt.optind<nArgs)
                for (int i=GetOpt.optind;i<nArgs;i++)
                    doArg(args[i],bSubtotal,bAll,bSortSize,bDirsOnly);
            else
                doArg(Directory.GetCurrentDirectory(),bSubtotal,bAll,bSortSize,bDirsOnly);
        } else
            doArg(Directory.GetCurrentDirectory(),false,false,bSortSize,false);
    }

    private static void doArg(string szArg,bool bSub,bool bAll,bool bSortSize,bool bDirsOnly) {
        if (Directory.Exists(szArg)) {
            showContents(szArg,bSub,bAll,bSortSize,bDirsOnly);
        }
    }

    internal struct MyFileInfo {
        private string _szFileOrDir;
        private long _lSize;
        private bool _bIsDir;
        private bool _bShow;

        public MyFileInfo(string szDirName,long lSize,bool doShow) {
            _szFileOrDir=szDirName;
            _lSize=lSize;
            _bIsDir=true;
            _bShow=doShow;
        }

        public MyFileInfo(string szFileName,long lSize,bool isDirectory,bool doShow) : this(szFileName,lSize,doShow) {
            _bIsDir=isDirectory;
        }

        public string description { get { return _szFileOrDir; }}
        public long size { get { return _lSize; }}
        public bool show { get { return _bShow; }}
        public bool isDirectory { get { return _bIsDir; }}
    }

    private static ArrayList calcDirSize(string szDir,string szBaseDir,bool bAll,out long lRet) {
        long lTmp;
        int nLen;
        FileSystemInfo[] fsiDirs,fsiFiles;
        DirectoryInfo di;
        ArrayList ret=null,alTmp;

        lRet=0;
        nLen=szBaseDir.Length+1;
        di=new DirectoryInfo(szDir);
        fsiDirs=di.GetDirectories();
        if (fsiDirs!=null)
            foreach (DirectoryInfo dir in fsiDirs) {
                alTmp=calcDirSize(dir.FullName,szBaseDir,bAll,out lTmp);
                if (alTmp!=null) {
                    if (ret==null)
                        ret=new ArrayList();
                    lRet+=lTmp;
                    ret.Add(new MyFileInfo(dir.FullName,lTmp,bAll));
                    ret.AddRange(alTmp);
                }
            }
        fsiFiles=di.GetFiles();
        if (fsiFiles!=null)
            foreach (FileInfo fi in fsiFiles) {
                if (ret==null)
                    ret=new ArrayList();
                lRet+=fi.Length;
                ret.Add(new MyFileInfo(fi.FullName,fi.Length,false,bAll));
            }
        return ret;
    }

    private static void showContents(string szDir,bool bSub,bool bAll,bool bSortSize,bool bDirsOnly) {
        FileSystemInfo[] fsiFiles,fsiDirs;
        long lTotal,lTmp;
        ArrayList alRet,alTmp;

        DirectoryInfo di;
        int nLen;

        if (bAll)
            bSub|=bAll;

        alRet=null;
        nLen=szDir.Length+1;
        lTotal=0;
        di=new DirectoryInfo(szDir);
        fsiDirs=di.GetDirectories();
        if (fsiDirs!=null)
            foreach (DirectoryInfo dir in fsiDirs) {
                alTmp=calcDirSize(dir.FullName,szDir,bAll,out lTmp);
                lTotal+=lTmp;
                if (alTmp!=null) {
                    if (alRet==null)
                        alRet=new ArrayList();
                    alRet.AddRange(alTmp);
                }
            }

        fsiFiles=di.GetFiles();
        if (fsiFiles!=null)
            foreach (FileInfo fi in fsiFiles) {
                if (bSub && !bDirsOnly) {
                    if (alRet==null)
                        alRet=new ArrayList();
                    alRet.Add(new MyFileInfo(fi.FullName,fi.Length,false,bAll));
                }
                lTotal+=fi.Length;
            }
        if (alRet!=null) {
            int nItems=alRet.Count;

            if (nItems>0) {
                MyFileInfo[] mfis=new MyFileInfo[nItems];

                alRet.CopyTo(mfis,0);
                Array.Sort(mfis,new MyFileInfoComparer(bSortSize));
                foreach (MyFileInfo mfi in mfis) {
                    if (bDirsOnly && !mfi.isDirectory)
                        continue;
                    if (mfi.show)
                        Console.WriteLine("{0,4} {1}{2}",mfi.size/1024,mfi.description.Substring(nLen),mfi.isDirectory?"\\":"");
                }
            }
        }
        Console.WriteLine("{0,4} {1}",lTotal/1024,".");
    }
    internal class MyFileInfoComparer : IComparer {
        private enum SortType {
            ByDir,
            BySize
        };

        private SortType _sortType;

        internal MyFileInfoComparer(bool bBySize) {
            _sortType=bBySize?SortType.BySize:SortType.ByDir;
        }


        #region IComparer Members

        public int Compare(object x, object y) {
            int ret=0;

            if (x is MyFileInfo && y is MyFileInfo) {
                MyFileInfo mfi1,mfi2;

                mfi1=(MyFileInfo) x;
                mfi2=(MyFileInfo) y;

                if (mfi1.isDirectory==mfi2.isDirectory) {
                    long lRet;
                    if (_sortType==SortType.BySize) {
                        if ((lRet=mfi1.size-mfi2.size)==0)
                            ret=mfi1.description.CompareTo(mfi2.description);
                        else
                            ret=(lRet<0?-1:1);
                    } else {
                        if ((ret=Path.GetDirectoryName(mfi1.description).CompareTo(Path.GetDirectoryName(mfi2.description)))==0) {
                            ret=Path.GetFileName(mfi1.description).CompareTo(Path.GetFileName(mfi2.description));
                        }
                    }
                } else {
                    ret=(mfi1.isDirectory?-1:1);
                }
            }
            return ret;
        }
        #endregion

    }
}