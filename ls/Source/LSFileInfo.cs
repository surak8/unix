using System.IO;
using System;
namespace NSLS {
	class LSFileInfo {
		#region fields
		DirectoryInfo _di;
		FileInfo _fi;
		bool _isDir;
//		FileSystemInfo _fi;
		#endregion

		#region CTOR
		LSFileInfo() {
			_di=null;
			_fi=null;
			_isDir=false;
		}
		internal LSFileInfo(DirectoryInfo di) {
			if (di==null)
				throw new ArgumentNullException("di","directory-info is null");
			_isDir=true;
			_di=di;
		}
		internal LSFileInfo(FileInfo fi) {
			if (fi==null)
				throw new ArgumentNullException("fi","file-info is null");
			_fi=fi;
		}
		#endregion

		#region properties
		public FileSystemInfo info { get {
				if (isDirectory) return _di;
				return _fi;
			}
		}
		public bool isDirectory { get { return _isDir; } }
		#endregion

		const string defaultDateFormat="dd-MMM-yy HH:mm:ss";

		public override string ToString() {
#if true
			return string.Format("{0,-4} {1,5} {2} {3}",
				isDirectory?"DIR":"FILE",
				isDirectory?0:((FileInfo) this.info).Length,
				info.LastWriteTime.ToString(defaultDateFormat),
				info.Name);

#else
			return string.Format("{0,-40} CREATE={1} ACCESS={2} WRITE={3}",
				info.Name,
				info.CreationTime.ToString(defaultDateFormat),
				info.LastAccessTime.ToString(defaultDateFormat),
				info.LastWriteTime.ToString(defaultDateFormat));
#endif
		}
	}
}