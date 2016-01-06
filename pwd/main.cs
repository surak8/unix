using System;
using System.IO;
using System.Reflection;

// start Assembly-level attributes.
[assembly:AssemblyProduct("pwd")]
[assembly:AssemblyTitle("pwd")]
[assembly:AssemblyVersion("1.0.0.0")]
[assembly:AssemblyCompany("Phibro Inc.")]
[assembly:AssemblyCopyright("Copyright(c) 2005,Phibro Inc.")]
#if DEBUG
	[assembly:AssemblyConfiguration("Debug version")]
#else
	[assembly:AssemblyConfiguration("Release version")]
#endif
// end Assembly-level attributes.

// Namespace comment
namespace pwd {

	/// <summary>driver.</summary>
	public class main {
		/// <summary>entry-point comment</summary>
		/// <param name="args"></param>
		public static void Main(string[] args) {
            Console.WriteLine(Directory.GetCurrentDirectory());
		}
	}
}
