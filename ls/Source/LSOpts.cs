namespace NSLS {
	class LSOpts {
		#region fields
		bool _singleLine;
		bool _directories;
		bool _longListing;
		bool _recursive;
		LSSortOrder _order;
		#endregion

		#region CTOR
		internal LSOpts() {
			_singleLine=_longListing=_recursive=false;
			_directories=true;
			_order=LSSortOrder.Name;
		}
		#endregion

		#region properties
		public bool singleLine { get { return _singleLine; } set { _singleLine=value; } }
		public bool showDirectories { get { return _directories; } set { _directories=value; } }
		public bool longListing { get { return _longListing; } set { _longListing=value; } }
		public bool recursive { get { return _recursive; } set { _recursive=value; } }
		public LSSortOrder order { get { return _order; } set { _order=value; } }
		#endregion
	}
}