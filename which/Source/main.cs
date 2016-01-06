using System;
//using System.Collections.Specialized;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace NSWhich {
	class which_Main {

		// entry-point comment
		/// entry-point comment
		public static void Main(string[] args) {
			int nArgs;

			if (args!=null&&(nArgs=args.Length)>0) {
				string[] toks,saFiles;
				string tmp;
#if true
				List<string> scPaths;
#else
				StringCollection scPaths;
#endif
				bool bFound;

#if true
				scPaths=new List<string>();
#else
				scPaths=new StringCollection();
#endif
				toks=Environment.GetEnvironmentVariable("path").Split(new char[] { ';' });
				Array.Sort(toks);
#if !FALSE
				foreach (string s in toks)
					if (Directory.Exists(s)) {
						if (!scPaths.Contains(s.ToLower()))
							scPaths.Add(s.ToLower());

					} else
						Console.Error.WriteLine("warning: PATH element '{0}' doesn't exist.",s);
#else
            scPaths.AddRange(toks);
#endif
				string szFound;
				foreach (string arg in args) {
					bFound=false;
					foreach (string subPath in scPaths) {
						if (Directory.Exists(subPath)) {
							if (File.Exists(szFound=Path.Combine(subPath,arg))) {
								bFound|=true;
								Console.WriteLine(szFound);
							} else {
								if (string.IsNullOrEmpty(Path.GetExtension(arg))) {
									saFiles=Directory.GetFiles(subPath,string.Format("{0}.*",arg));
									if (saFiles!=null)
										foreach (string result in saFiles) {
											tmp=Path.GetExtension(result);
											if (tmp==".bat"||tmp==".cmd"||tmp==".exe"||tmp==".com"||tmp==".dll") {
												bFound|=true;
												Console.WriteLine(result);
											}
										}
								}
							}
						}
					}
					if (!bFound)
						Console.Error.WriteLine("not found: {0}",arg);
				}
			} else {
				Console.Error.WriteLine("no args");
				Environment.Exit(1);
			}
			Environment.Exit(0);
		}
	}
}