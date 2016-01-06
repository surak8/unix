using System;
using System.Reflection;
using System.Collections.Specialized;

namespace NSLS {

	/// <summary>
	/// This class provides a unix-like getopt facility.
	/// Like the unix version 'optarg' and 'optind' are available, and all of the
	/// arguments to be processed are prefixed with either the '-' or '/' 
	/// characters.  All "real" arguments to an executable follow the prefixed args.
	/// </summary>
	class GetOpt {
		static GetOpt singleInstance=null;
		StringCollection scArgs=null;
		string optArg=null;
		int optInd=-1;
		int nOpts=0;

		/// <summary>
		/// Calling this member in a loop will return each command-line option.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="opts"></param>
		/// <returns></returns>
		public static int getopt(string[] args,string opts) {
			if (singleInstance==null)
				singleInstance=new GetOpt(args,opts);
			return singleInstance.getoptInternal();
		}

		/// <summary>
		/// returns the 'optarg' property.
		/// </summary>
		public static string optarg {
			get { return singleInstance!=null?singleInstance.optArgInternal:null; }
		}

		/// <summary>
		/// returns the 'optind' property.
		/// </summary>
		public static int optind {
			get { return singleInstance!=null?singleInstance.optIndInternal:0; }
		}

		/// <summary>
		/// internal constructor which manages the list of options.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="opts"></param>
		protected GetOpt(string[] args,string opts) {
			scArgs=new StringCollection();
			parseOptions(args,opts);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		/// <param name="opts"></param>
		void parseOptions(string[] args,string opts) {
			int nArgs=args.Length;

			if (nArgs>0) {
				int i;
				string arg;

				for (i=0;i<nArgs;i++) {
					arg=args[i];
					if (arg[0]=='/'||arg[0]=='-') {
						char c;
						int findex,argLen,j;
						string addArg;

						nOpts++;
						argLen=arg.Length;
						for (j=1;j<argLen;j++) {
							c=arg[j];
							addArg=Convert.ToString(c);
							if ((findex=opts.IndexOf(c))>=0) {
								if (findex+1<opts.Length) {
									if (opts[findex+1]==':') {
										string optArg="?";
										if (argLen>j+1) {
											optArg=arg.Substring(j+1);
										} else {
											if (i+1<nArgs) {
												nOpts++;
												optArg=args[i+1];
												i++;
											}
										}
										addArg=string.Format("{0}:{1}",c,optArg);
										j=argLen;
									}
								}
							}
							scArgs.Add(addArg);
						}
					}
				}
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		int getoptInternal() {
			int ret=-1;

			optArg=null;
			if (scArgs.Count>0&&optInd<scArgs.Count) {

				optInd++;
				if (optInd<scArgs.Count) {
					string retStr;
					int findex;

					retStr=scArgs[optInd];
					if ((findex=retStr.IndexOf(':'))>=0) {
						string tmp=retStr.Substring(findex+1);

						ret=Convert.ToInt32(retStr[0]);
						if (tmp=="?")
							Console.Error.WriteLine("warning -- opt '{0}' has "+
								"null optarg",retStr[0]);
						else
							optArg=tmp;
					} else {
						ret=Convert.ToInt32(retStr[0]);
					}
				}
			}
			if (ret<0)
				optInd=nOpts;
			return ret;
		}

		/// <summary>
		///
		/// </summary>
		///
		int optIndInternal { get { return optInd; } }

		/// <summary>
		///
		/// </summary>
		string optArgInternal { get { return optArg; } }
	}

}