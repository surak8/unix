using System.Collections.Generic;
using System.Collections;
using System.Text;
using System;
using System.IO;

//234567890123456789012345678901234567890123456789012345678901234567890123456789
namespace NSUniq {
	/// <summary>
	/// Interface describing functionality of a command-line parameter.
	/// </summary>
	interface ICmdLineParameter {
		/// <summary>Key name for this parameter.</summary>
		string parameterKey { get; }

		/// <summary>Verbose name for this parameter.</summary>
		string longArgumentName { get; }

		/// <summary>Help text associated with this parameter.</summary>
		string helpText { get; }

		/// <summary>Returns an indicator whether the underlying parameter is a 
		/// <see cref="bool"/> value or not.</summary>
		bool isBooleanSwitch { get; }

		/// <summary>Returns or sets the underlying (<see cref="bool"/>) 
		/// parameter value.</summary>
		bool isSelected { get; set;}

		/// <summary>Returns or sets the underlying (<see cref="string"/>) 
		/// parameter.</summary>
		string switchValue { get; set; }

		/// <summary>Output the argument <b>usage</b>.</summary>
		/// <param name="tw">a <see cref="TextWriter"/>.</param>
		void showArg(TextWriter tw);
	}

	/// <summary>
	/// Base class for all command-line parameters.
	/// </summary>
	abstract class CmdLineParameter : ICmdLineParameter {
		#region fields
		string _key;
		string _longKey;
		string _helpText;
		string _switchValue;
		bool _required;
		#endregion

		#region CTOR
		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="aChar"></param>
		/// <param name="aLongArg"></param>
		/// <param name="helpMsg"></param>
		/// <param name="bRequired"></param>
		protected CmdLineParameter(char aChar,string aLongArg,string helpMsg,bool bRequired) {
			_key=aChar.ToString();
			_longKey=aLongArg;
			_helpText=helpMsg;
			_required=bRequired;
		}
		#endregion

		#region abstract implementation
		/// <summary>Returns an indicator as to whether the underlying 
		/// parameter is a <see cref="bool"/> switch or not.</summary>
		/// <value><b>True</b> for a <see cref="bool"/> switch, <b>false</b>
		/// otherwise.</value>
		protected abstract bool isBoolean { get; }

		/// <summary>Returns an indicator for <see cref="bool"/> 
		/// switches.</summary>
		/// <value><b>True</b> if the underlying parameter is 
		/// <see cref="bool"/> and the switch is found in the 
		/// command-line arguments, <b>False</b> otherwise.</value>
		protected abstract bool selected { get; set; }
		#endregion

		#region virtual implementation
		/// <summary>Outputs text describing the current parameter.</summary>
		/// <param name="tw">a <see cref="TextWriter"/> to use for the
		/// generated text.</param>
		protected virtual void showUsage(TextWriter tw) {
			tw.Write("\t-{0}",parameterKey);
			if (!string.IsNullOrEmpty(longArgumentName))
				tw.Write(", --{0}",longArgumentName);
			else
				tw.Write('\t');
			tw.WriteLine("\t\t{0}",helpText);
		}
		#endregion virtual implementation

		#region ICmdLineParameter Members
		public string parameterKey { get { return string.IsNullOrEmpty(_key)?string.Empty:_key; ; } }
		public string longArgumentName { get { return string.IsNullOrEmpty(_longKey)?string.Empty:_longKey; ; } }
		public string helpText { get { return string.IsNullOrEmpty(_helpText)?string.Empty:_helpText; } }
		public bool isBooleanSwitch { get { return isBoolean; } }
		public bool isSelected {
			get {
				if (isBoolean) return selected;
				return !string.IsNullOrEmpty(switchValue);
			}
			set {
				if (isBoolean)
					selected=value;
			}
		}
		public string switchValue {
			get {
				if (isBoolean) return selected.ToString().ToLower();
				return string.IsNullOrEmpty(_switchValue)?string.Empty:_switchValue;
			}
			set {
				if (isBoolean) {
					selected=string.Compare(value,"true",true)==0;
				} else {
					_switchValue=value;
				}
			}
		}

		void ICmdLineParameter.showArg(TextWriter tw) {
			this.showUsage(tw);
		}

		#endregion

		#region methods
		public override string ToString() {
			return string.Format("{0} --{1} {2}",parameterKey,longArgumentName,helpText);
		}
		#endregion
	}

	class SwitchParameter : CmdLineParameter {
		#region fields
		bool _sel;
		#endregion

		#region CTOR
		public SwitchParameter(char aChar,string aLongArg,string helpMsg) : this(aChar,aLongArg,helpMsg,false,false) { _sel=false; }
		public SwitchParameter(char aChar,string aLongArg,string helpMsg,bool currValue) : this(aChar,aLongArg,helpMsg,currValue,false) { }
		public SwitchParameter(char aChar,string aLongArg,string helpMsg,bool currValue,bool bRequired) : base(aChar,aLongArg,helpMsg,bRequired) { _sel=currValue; }
		#endregion

		#region properties
		protected override bool isBoolean { get { return true; } }
		protected override bool selected { get { return _sel; } set { _sel=value; } }
		#endregion

		#region methods
		public override string ToString() {
			return string.Format("{0} {1}",base.ToString(),selected);
		}
		#endregion
	}

	class StringParameter : CmdLineParameter {
		#region fields
		string _argName;

		#endregion

		#region CTOR
		public StringParameter(char aChar,string aLongArg,string argName,string helpMsg) : this(aChar,aLongArg,argName,helpMsg,string.Empty,false) { }

#if true
		public StringParameter(char aChar,string aLongArg,string argName,string helpMsg,string currValue)
			: this(aChar,aLongArg,argName,helpMsg,currValue,false) {
		}
#else
		public StringParameter(char aChar,string aLongArg,string argName,string helpMsg,string currValue)
			: base(aChar,aLongArg,helpMsg,false) {
		}
#endif

		public StringParameter(char aChar,string aLongArg,string argName,string helpMsg,string currValue,bool bRequired)
			: base(aChar,aLongArg,helpMsg,bRequired) {
			if (string.IsNullOrEmpty(argName))
				throw new ArgumentNullException("argName","argument-name is null!");
			_argName=argName;
			switchValue=currValue;
		}

		#endregion

		#region properties
		public string argumentName { get { return string.IsNullOrEmpty(_argName)?string.Empty:_argName; } }
		protected override bool isBoolean { get { return false; } }
		protected override bool selected { get { return false; } set { } }
		#endregion

		#region methods
		public override string ToString() {
			if (string.IsNullOrEmpty(switchValue))
				return string.Format("{0} null",base.ToString());
			return string.Format("{0} \"{1}\"",base.ToString(),switchValue);
		}
		protected override void showUsage(TextWriter tw) {
			tw.Write("\t-{0} {1}",parameterKey,argumentName);
			if (!string.IsNullOrEmpty(longArgumentName))
				tw.Write(", --{0}={1}",longArgumentName,argumentName);
			else
				tw.Write('\t');
			tw.WriteLine("\t\t{0}",helpText);
		}
		#endregion
	}

	/// <summary>
	/// Generic interface describing functionality
	/// </summary>
	/// <typeparam name="T"></typeparam>
	interface ICmdlineParmCollection<T> : IEnumerable<T> {
		T this[char c] { get; }
		T this[string longOpt] { get; }
		bool ContainsParameter(char singleChar);
		bool ContainsParameter(string argName);
	}

	/// <summary>
	/// Collection of command-line arguments accepted by the current executable.
	/// </summary>
	class CmdLineParameterCollection : ICmdlineParmCollection<ICmdLineParameter> {
		#region fields
		/// <summary>
		/// Unique list of possible parameters.
		/// </summary>
		List<ICmdLineParameter> _uniqueArgs;

		/// <summary>
		/// Collection of error messages.
		/// </summary>
		List<string> _errs;

		/// <summary>
		/// All possible command-line arguments.
		/// </summary>
		IDictionary<string,ICmdLineParameter> _args1;
		#endregion

		#region CTORs
		/// <summary>
		/// default CTOR.
		/// </summary>
		public CmdLineParameterCollection() {
			_uniqueArgs=new List<ICmdLineParameter>();
			_args1=new Dictionary<string,ICmdLineParameter>();
			_errs=new List<string>();
		}

		/// <summary>
		/// CTOR override initializing possible parameters.
		/// </summary>
		/// <param name="iargs"></param>
		public CmdLineParameterCollection(ICmdLineParameter[] iargs)
			: this() {
			string key2;

			if (iargs!=null) {
				_uniqueArgs.AddRange(iargs);
				foreach (ICmdLineParameter iarg in iargs) {
					args.Add(iarg.parameterKey,iarg);
					if (!string.IsNullOrEmpty(key2=iarg.longArgumentName)&&!args.ContainsKey(key2))
						args.Add(iarg.longArgumentName,iarg);
				}
			}
		}
		#endregion

		#region properties

		/// <summary>
		/// Returns a unique list of arguments.
		/// </summary>
		/// <value>
		/// <para>A <see cref="List&lt;T&gt;"/> (where  <b>T</b> is 
		/// <see cref="ICmdLineParameter"/>).</para>
		/// <para>Exposes the <see cref="_uniqueArgs"/> field.</para>
		/// </value>
		List<ICmdLineParameter> uniqueArgs { get { return _uniqueArgs; } }

		/// <summary>
		/// Returns a collection of all possible arguments.
		/// </summary>
		IDictionary<string,ICmdLineParameter> args { get { return _args1; } }

		/// <summary>
		/// Returns a collection of error messages.
		/// </summary>
		List<string> errors { get { return _errs; } }

		/// <summary>
		/// Returns an indicator of whether or not 
		/// </summary>
		public bool errorFound { get { return errors.Count>0; } }

		/// <summary>
		/// Returns an aggregated error message.
		/// </summary>
		public string errorMessages {
			get {
				StringBuilder sb;

				if (!errorFound) return string.Empty;

				sb=new StringBuilder();
				for (int i=0;i<errors.Count;i++) {
					if (i>0)
						sb.Append("\r\n");
					sb.Append(errors[0]);
				}
				return sb.ToString();
			}
		}
		#endregion

		#region methods
		/// <summary>
		/// Iterate throught all command-line arguments, setting flags used and 
		/// extracting those which are not tied to specific parameters.
		/// </summary>
		/// <param name="cmdLineArgs"></param>
		/// <returns>an <see cref="Array"/> of <see cref="string"/>s indicating files 
		/// or directories to process.</returns>
		/// 
		/// <seealso cref="uniqueArgs"/>
		/// 
		/// <seealso cref="updateArgValue"/>
		/// <seealso cref="ContainsParameter(char)"/>
		/// <seealso cref="errors"/>
		internal string[] decode(string[] cmdLineArgs) {
			List<string> ret=new List<string>();
			ICmdLineParameter iarg;
			int nargs,len,index,pos;
			string szArg,largName;
			char c;
			bool exitWithError=false;

			if ((nargs=cmdLineArgs==null?0:cmdLineArgs.Length)>0) {
				for (int i=0;i<nargs&&!exitWithError;i++) {
					szArg=cmdLineArgs[i];
					if ((len=szArg.Length)>=2&&((c=szArg[0])=='-'||c=='/')) {
						if (szArg[1]=='-') {
							index=2;
							if ((pos=szArg.IndexOf('=',index))>0)
								updateArgValue(szArg.Substring(index,pos-index),szArg.Substring(pos+1));
							else
								updateArgValue(szArg.Substring(index),null);
						} else {
							index=1;
							while (!exitWithError&&index<len) {
								if (!ContainsParameter(c=szArg[index])) {
									errors.Add(string.Format("unhandled: '{0}'",c));
									exitWithError=true;
								} else {
									if ((iarg=this[c]).isBooleanSwitch) {
										iarg.isSelected=!iarg.isSelected;
										if (!string.IsNullOrEmpty(largName=iarg.longArgumentName))
											foreach (ICmdLineParameter iarg2 in uniqueArgs)
												if (iarg2!=iarg&&string.Compare(iarg2.longArgumentName,largName,true)==0)
													iarg2.isSelected=!iarg2.isSelected;

									} else {
										// read next arg.
										if (index+1>=len) {
											i++;
											len=0;
											iarg.switchValue=cmdLineArgs[i];
										} else {
											iarg.switchValue=szArg.Substring(index+1);
											index=len;
										}
									}
								}
								index++;
							}
						}
					} else {
						ret.Add(szArg);
					}
				}
			}
			return ret.ToArray();
		}

		/// <summary>
		/// Updates the underlying parameter with the argument found on
		/// the command-line.
		/// </summary>
		/// <param name="longArgName">a <see cref="string"/> containing the 
		/// name of the command-line parameter to be updated.</param>
		/// <param name="longArgValue">a <see cref="string"/> containing the
		/// value to be used for the given parameter.</param>
		void updateArgValue(string longArgName,string longArgValue) {
			ICmdLineParameter iarg;

			if (string.IsNullOrEmpty(longArgName))
				throw new ArgumentNullException("longArgName","argument-name is null!");
			if (args.ContainsKey(longArgName))
				if ((iarg=args[longArgName]).isBooleanSwitch)
					iarg.isSelected=!iarg.isSelected;
				else
					iarg.switchValue=longArgValue;
			else
				errors.Add(string.Format("invalid command-line argument: {0}",longArgName));
		}

		#endregion

		#region ICmdlineParmCollection<ICmdLineParameter> Members
		/// <summary>
		/// Returns the given argument.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public ICmdLineParameter this[char c] { get { return this[new string(c,1)]; } }
		public ICmdLineParameter this[string longOpt] { get { return this.args[longOpt]; } }
		public bool ContainsParameter(char singleChar) { return ContainsParameter(new string(singleChar,1)); }
		public bool ContainsParameter(string argName) { return args.ContainsKey(argName); }
		#endregion

		#region IEnumerable<ICmdLineParameter> Members
		IEnumerator<ICmdLineParameter> IEnumerable<ICmdLineParameter>.GetEnumerator() { return uniqueArgs.GetEnumerator(); }
		#endregion

		#region IEnumerable Members
		IEnumerator IEnumerable.GetEnumerator() { return uniqueArgs.GetEnumerator(); }
		#endregion
	}
}