using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ls")]
[assembly: AssemblyProduct("ls")]
[assembly: AssemblyDescription("unix-like \"ls\" utility")]
#if DEBUG
[assembly: AssemblyConfiguration("debug version")]
#else
[assembly: AssemblyConfiguration("release version")]
#endif
[assembly: AssemblyCompany("Rik Cousens")]
[assembly: AssemblyCopyright("Copyright 2005-2018, Rik Cousens")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]		

[assembly: AssemblyVersion("1.0.0.1")]
