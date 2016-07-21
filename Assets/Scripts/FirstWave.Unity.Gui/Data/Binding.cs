﻿namespace FirstWave.Unity.Gui.Data
{
	public class Binding
	{
		private object source;
		private object target;

		// Used in the case of a one time binding
		private object cachedValue;
		private bool calculatedValue;

		public object DataContext { get; set; }
		public string Path { get; set; }
		public string ElementName { get; set; }
		public BindingMode Mode { get; set; }

		public object Source
		{
			get
			{
				// In the future this will get to choose between DataContext and ElementName
				if (source == null)
					source = GetSource();

				return source;
			}
		}

		public Binding(object target)
		{
			this.target = target;

			Mode = BindingMode.OneWay;
		}

		public object GetValue()
		{
			var src = Source;

			if (src == null)
				return null;

			if (Mode == BindingMode.OneTime && calculatedValue)
				return cachedValue;

			var localSrc = src;

			if (!string.IsNullOrEmpty(Path))
			{
				var pathParts = Path.Split(new char[] { '.' });
				for (int i = 0; i <= pathParts.Length - 1; i++)
					localSrc = localSrc.GetType().GetProperty(pathParts[i]).GetValue(localSrc, null);
			}

			if (Mode == BindingMode.OneTime)
			{
				calculatedValue = true;
				cachedValue = localSrc;
			}

			// Shouldn't ever get here
			return localSrc;
		}

		private object GetSource()
		{
			if (target is Control)
			{
				var c = target as Control;

				var dc = c.DataContext;
				while (dc == null && c != null)
				{
					c = c.Parent;
					if (c != null)
						dc = c.DataContext;
				}

				// Now let's wire up a listener for changes to this
				if (dc is INotifyPropertyChanged)
					(dc as INotifyPropertyChanged).PropertyChanged += DataContext_PropertyChanged;

				return dc;
			}

			return null;
		}

		private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var targetCtrl = target as Control;

			if (Path == null)
				// We already know the target is a control because we wired up this event
				targetCtrl.InvalidateLayout(targetCtrl);
			else
			{
				// Only check the first part of the path for a match right now (this is probably gonna be the most common case anyway
				var pathStart = Path.Split(new char[] { '.' })[0];

				if (pathStart == e.PropertyName)
					(targetCtrl).InvalidateLayout(targetCtrl);
			}
        }
	}
}
