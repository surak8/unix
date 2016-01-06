#define NEW_ARGS
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace NSLS {
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class driver {

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			bool showUsage;
			int exitCode=0;
			ls ls=null;
			LSOpts opts;
#if NEW_ARGS
			CmdLineParameterCollection parms;
			string[] lsArgs;
#else
			bool bDefault=true;
			int c;

#endif
			opts=new LSOpts();
			showUsage=false;
			lsArgs=null;
#if NEW_ARGS
			parms=new CmdLineParameterCollection(
				new ICmdLineParameter[] {
						new SwitchParameter('1',"single","Generate output on a single line."),
						new SwitchParameter('a',"a","\tcan't remember."),
						new SwitchParameter('d',"dirs","Show directories in listing.",true),
						new SwitchParameter('l',"long","Show long format."),
						new SwitchParameter('R',"recursive","Descend subfolders."),

						new SwitchParameter('n',"sort-name","Sort by name.",true),
						new SwitchParameter('s',"sort-size","Sort by size."),
						new SwitchParameter('t',"sort-time","Sort by date/time."),

						new SwitchParameter('h',"help","This help message."),
						new SwitchParameter('?',"help","This help message."),
				});
			if (args!=null&&args.Length>0) {
				lsArgs=parms.decode(args);
				if (parms.errorFound) {
					Console.Error.WriteLine(parms.errorMessages);
					showUsage=true;
					exitCode=1;
				} else {
					if (parms["help"].isSelected) {
						Console.Error.WriteLine("help requested.");
						showUsage=true;
					} else {
						if (parms["sort-size"].isSelected) {
							parms["sort-name"].isSelected=false;
							parms["sort-time"].isSelected=false;
							parms["long"].isSelected=true;
							parms["single"].isSelected=true;
						} else if (parms["sort-time"].isSelected) {
							parms["sort-name"].isSelected=false;
							parms["sort-size"].isSelected=false;
							parms["long"].isSelected=true;
							parms["single"].isSelected=true;
						}
						if (parms["sort-name"].isSelected) opts.order=LSSortOrder.Name;
						else if (parms["sort-size"].isSelected) opts.order=LSSortOrder.Size;
						else if (parms["sort-time"].isSelected) opts.order=LSSortOrder.Time;
						opts.singleLine=parms["single"].isSelected;
						opts.showDirectories=parms["dirs"].isSelected;
						opts.longListing=parms["long"].isSelected;
//						if (opts.longListing)
//							opts.singleLine=true;
						opts.recursive=parms["recursive"].isSelected;
					}
				}
			}
			if (showUsage)
				usage(parms);
			else {
				ls=new ls(opts);
				if (lsArgs!=null&&lsArgs.Length>0)
					foreach (string anArg in lsArgs)
						ls.findFiles(anArg);
				else
					ls.findFiles(Path.Combine(Directory.GetCurrentDirectory(),"*.*"));
				ls.showResults(Console.Out);
			}

#else
			if (args==null&&args.Length>0) {

				while ((c=GetOpt.getopt(args,"1adhlnst?"))>=0)
					switch (c) {
						case '1': opts.singleLine=!opts.singleLine; break;
						case 'a': break;
						case 'd': opts.showDirectories=!opts.showDirectories; break;
						case 'l': opts.longListing=!opts.longListing; break;
						case 'n': opts.order=LSSortOrder.Name; break;
						case 'R': opts.recursive=true; break;
						case 's': opts.order=LSSortOrder.Size; break;
						case 't': opts.order=LSSortOrder.Time; break;
						case 'h':
						case '?': showUsage=true;
							break;
						default:
							Console.Error.WriteLine("unknown command-line flag '{0}'!",(char) c);
							exitCode=1;
							break;
					}
				if (exitCode==0&&!showUsage) {
					ls=new ls(opts);
					if (GetOpt.optind<args.Length) {
						for (int i=GetOpt.optind;i<args.Length;i++)
							ls.findFiles(args[i]);
						bDefault=false;
					}
				}
			}
			if (!showUsage&&exitCode==0) {
				if (bDefault) {
					if (ls==null)
						ls=new ls(opts);
					ls.findFiles(Directory.GetCurrentDirectory());
				}
			}
			if (ls!=null)
				ls.showResults(Console.Out);
			if (showUsage)
				usage();
#endif
			Environment.Exit(exitCode);
		}

#if NEW_ARGS
		static void usage(CmdLineParameterCollection parms) {
#else
		static void usage() {
#endif
#if NEW_ARGS
			string appName=Assembly.GetEntryAssembly().GetName().Name;

			Console.Error.WriteLine(
				"usage: {0} [OPTIONS] class_name [... class_name]",appName);
			if (parms!=null)
				foreach (ICmdLineParameter iarg in parms)
					iarg.showArg(Console.Error);
#else
			Console.Error.WriteLine("usage: {0} -[1dhlnRst?] path_or_file [...path_or_file]",Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
			Console.Error.WriteLine("\t-1\tgenerate 1 line per file/directory.");
			Console.Error.WriteLine("\t-a\tshow \"dot\" files (not applicable).");
			Console.Error.WriteLine("\t-d\tdirectories.");
			Console.Error.WriteLine("\t-l\tgenerate long listing.");
			Console.Error.WriteLine("\t-n\tsort by name.");
			Console.Error.WriteLine("\t-R\tgenerate recursively.");
			Console.Error.WriteLine("\t-s\tsort by size.");
			Console.Error.WriteLine("\t-t\tsort by time.");
			Console.Error.WriteLine("\t-h\tthis help message.");
			Console.Error.WriteLine("\t-?\tthis help message.");
#endif
		}
	}

	class ls {
		#region fields
		static int maxLen=0;
		static string fmtShort;
		LSOpts _opts;
		List<LSFileInfo> _entries;
		List<LSFileInfo> entries { get { return _entries; } }
		#endregion

		#region CTORs
		internal ls() {
			_opts=new LSOpts();
			_entries=new List<LSFileInfo>();
		}

		internal ls(LSOpts opts)
			: this() {
			if (opts!=null)
				_opts=opts;
		}
		#endregion

		#region properties
		LSOpts options { get { return _opts; } }
		public bool singleLine { get { return options.singleLine; } }
		public bool directories { get { return options.showDirectories; } }
		public bool longListing { get { return options.longListing; } }
		public bool recursive { get { return options.recursive; } }
		#endregion

		public void findFiles(string szDir) {
			int n1,n2;
			string szBaseDir,szWildcard;
			string[] aszFiles;
			int len;

			if (string.IsNullOrEmpty(szDir))
				throw new ArgumentNullException("szDir","path is null!");

			if ((n1=szDir.IndexOf('*'))>=0||
				(n2=szDir.IndexOf('?'))>=0) {
				if (string.IsNullOrEmpty(szBaseDir=Path.GetDirectoryName(szDir))) {
					szBaseDir=Path.GetFullPath(".");
					szWildcard=szDir;
				} else
					szWildcard=szDir.Substring(szBaseDir.Length+1);
			} else {
				szBaseDir=szDir;
				szWildcard="*.*";
			}
			if (!Directory.Exists(szBaseDir)) {
#if true
				Console.Error.WriteLine("non-existent directory: {0}",szBaseDir);
#else
				throw new DirectoryNotFoundException(string.Format("non-existent directory: {0}",szBaseDir));
#endif
			} else {
				if (options.showDirectories)
					entries.Add(new LSFileInfo(new DirectoryInfo(szBaseDir)));
				if ((aszFiles=Directory.GetFiles(szBaseDir,szWildcard,recursive?SearchOption.AllDirectories:SearchOption.TopDirectoryOnly))!=null) {
					foreach (string file in aszFiles) {
						if (maxLen<(len=file.Length))
							maxLen=len;
						entries.Add(new LSFileInfo(new FileInfo(file)));
					}
				}
			}
		}

		public int sortBySize(LSFileInfo lsfi1,LSFileInfo lsfi2) {
			int rc=0;
			FileInfo fi1,fi2;

			if (lsfi1.isDirectory&&lsfi2.isDirectory) {
				return string.Compare(lsfi1.info.Name,lsfi2.info.Name);
			} else {
				if (!lsfi1.isDirectory&&!lsfi2.isDirectory) {
					// both files.
					fi1=lsfi1.info as FileInfo;
					fi2=lsfi2.info as FileInfo;
					if ((rc=string.Compare(fi1.DirectoryName,fi2.DirectoryName))==0)
						rc=(int) (fi1.Length-fi2.Length);
				} else {
					// One file, one directory.
					return lsfi1.isDirectory?-1:1;
				}
			}
			return rc;
		}

		public int sortByTime(LSFileInfo lsfi1,LSFileInfo lsfi2) {
			int rc=0;
			FileInfo fi1,fi2;

			if (lsfi1.isDirectory&&lsfi2.isDirectory) {
				return string.Compare(lsfi1.info.Name,lsfi2.info.Name);
			} else if (!lsfi1.isDirectory&&!lsfi2.isDirectory) {
				fi1=lsfi1.info as FileInfo;
				fi2=lsfi2.info as FileInfo;
#if true
				rc=fi1.LastWriteTime.CompareTo(fi2.LastWriteTime);
#else
				lrc=(fi1.LastWriteTime.Ticks-fi2.LastWriteTime.Ticks);
				if (lrc==0)
					rc=0;
				else if (lrc>0)
					rc=-1;
				else
					rc=1;
#endif
			} else {
				return lsfi1.isDirectory?-1:1;
			}
			return rc;
		}

		public int sortByName(LSFileInfo lsfi1,LSFileInfo lsfi2) {
			if (lsfi1.isDirectory&&lsfi2.isDirectory) return string.Compare(lsfi1.info.Name,lsfi2.info.Name);
			if (!lsfi1.isDirectory&&!lsfi2.isDirectory) return string.Compare(lsfi1.info.Name,lsfi2.info.Name);
			return lsfi1.isDirectory?-1:1;
		}

		public void showResults(TextWriter textWriter) {
			StringBuilder sb=new StringBuilder();
#if true
			int dirLen,fileLen,len,fileCount;
#else
			string prevDir=string.Empty,currDir;
			int fileCount,nFile,nFilesPerLine;
			int dirLen,fileLen,len;
			LSFileInfo lsfi;
			FileInfo fi;
			DateTime now;
#endif

			if ((fileCount=entries.Count)>0) {
				switch (options.order) {
					case LSSortOrder.Size: entries.Sort(sortBySize); break;
					case LSSortOrder.Time: entries.Sort(sortByTime); break;
					case LSSortOrder.Name:
					default:
						entries.Sort(sortByName); break;
				}
				dirLen=fileLen=-1;
				foreach (LSFileInfo lsfi2 in entries) {
					if (lsfi2.isDirectory) {
						if (dirLen<(len=lsfi2.info.Name.Length))
							dirLen=len;
					} else {
						if (fileLen<(len=lsfi2.info.Name.Length))
							fileLen=len;
					}
				}
				if (options.longListing) {
					writeLongVersion(sb,DateTime.Now);
				} else {
					writeStuff(sb,dirLen,fileLen);
				}
			}
			textWriter.WriteLine(sb.ToString());
		}

		void writeStuff(StringBuilder sb,int dirLen,int fileLen) {
			int nFile,nFilesPerLine,nMax;
			string currDir,prevDir;
			LSFileInfo lsfi;
			FileInfo fi;

			nMax=Math.Max(dirLen,fileLen)+1;

			prevDir=null;
			nFile=nFilesPerLine=0;
			nFile++;
#if false
			nFilesPerLine=80/maxLen;
			fmtShort=string.Format("{0}0,-{1}{2} ",'{',maxLen,'}');
#else
			nFilesPerLine=Console.BufferWidth/nMax;
			fmtShort=string.Format("{0}0,-{1}{2} ",'{',nMax,'}');
#endif
			if (nFilesPerLine<=0)
				nFilesPerLine=1;

			for (int i=0;i<entries.Count;i++) {
				if (nFilesPerLine>1&&nFile%nFilesPerLine==0)
					sb.Append("\r\n");

				lsfi=entries[i];
				if (lsfi.isDirectory) {
					prevDir=currDir=Path.GetFullPath(lsfi.info.Name);
#if true
#	if true
					append(sb,string.Format("{0}/",lsfi.info.Name),nMax);
#	else
					sb.Append(lsfi.info.Name);
					sb.Append('/');
					if ((len=currDir.Length+1)<nMax)
						sb.Append(new string(' ',nMax-len));
#	endif
#else
					sb.Append("\r\n");
					sb.Append(currDir);
					sb.Append("\r\n");
#endif
				} else {
					fi=lsfi.info as FileInfo;
					if (string.Compare(currDir=fi.DirectoryName,prevDir)!=0) {
						prevDir=currDir;
#if false
						Console.WriteLine("here-2");
#else
						sb.Append("\r\n");
						sb.Append(currDir);
						sb.Append("\r\n");
						nFile=0;
#endif
					}
#if true
					append(sb,fi.Name,nMax);
#else
					sb.Append(fi.Name);
					if ((len=fi.Name.Length)<nMax)
						sb.Append(new string(' ',nMax-len));
#endif
				}
				nFile++;
			}
		}

		void append(StringBuilder sb,string p,int nMax) {
			int len;

			sb.Append(p);
			if ((len=p.Length)<nMax)
				sb.Append(new string(' ',nMax-len));
		}

		void writeLongVersion(StringBuilder sb,DateTime now) {
			LSFileInfo lsfi;
			FileInfo fi;
			string prevDir,currDir;

			prevDir=string.Empty;
			for (int i=0;i<entries.Count;i++) {
				if (i>0)
					sb.Append("\r\n");
				lsfi=entries[i];
				if (lsfi.isDirectory) {
					prevDir=currDir=lsfi.info.Name;
					sb.Append("\r\n");
					sb.Append(currDir);
					sb.Append("\r\n");
				} else {
					fi=lsfi.info as FileInfo;
					if (string.Compare(currDir=fi.DirectoryName,prevDir)!=0) {
						prevDir=currDir;
						sb.Append("\r\n");
						sb.Append(currDir);
						sb.Append("\r\n");
					}
					sb.Append(isFA(fi.Attributes,FileAttributes.Archive)?"A":"-");
					sb.Append(isFA(fi.Attributes,FileAttributes.Compressed)?"C":"-");
					sb.Append(isFA(fi.Attributes,FileAttributes.Directory)?"D":"-");
					sb.Append(isFA(fi.Attributes,FileAttributes.Encrypted)?"E":"-");
					sb.Append(isFA(fi.Attributes,FileAttributes.Hidden)?"H":"-");
					sb.Append(isFA(fi.Attributes,FileAttributes.Normal)?"N":"-");
					sb.Append(isFA(fi.Attributes,FileAttributes.ReadOnly)?"R":"-");
					sb.Append(isFA(fi.Attributes,FileAttributes.System)?"S":"-");
					sb.Append(isFA(fi.Attributes,FileAttributes.Temporary)?"T":"-");
					sb.AppendFormat(" {0,8}",fi.Length);
					if (fi.LastWriteTime.Date==now.Date)
						sb.AppendFormat("{0,8}","");
					else
						sb.AppendFormat(" {0}",fi.LastWriteTime.ToString("ddMMMyy"));
					sb.AppendFormat(" {0}",fi.LastWriteTime.ToString("HH:mm:ss"));
					sb.AppendFormat(" {0}",fi.Name);
				}
			}
		}

		static bool isFA(FileAttributes src,FileAttributes dest) {
			return ((src&dest)==dest);
		}
	}
}