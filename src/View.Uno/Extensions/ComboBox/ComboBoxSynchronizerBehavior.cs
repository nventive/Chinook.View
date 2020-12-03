﻿#if WINDOWS_UWP || __ANDROID__ || __IOS__ || __WASM__
using System.Collections;
using System.ComponentModel;
using System;
using System.Reflection;
using Uno.Collections;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using SharedBinding = Windows.UI.Xaml.Data.Binding;
#if __ANDROID__ || __IOS__ || __WASM__
using static Windows.UI.Xaml.DependencyObjectStore;
#endif

namespace Chinook.View.Extensions
{
	public static class ComboBoxSynchronizerBehavior
	{
		private static WeakAttachedDictionary<ComboBox, string> _holder = new WeakAttachedDictionary<ComboBox, string>();

		public static object GetItemsSource(ComboBox obj)
		{
			return (object)obj.GetValue(ItemsSourceProperty);
		}

		//needed in Windows to be able to set ItemsSource
		public static void SetItemsSource(ComboBox obj, object itemsSource)
		{
			obj.SetValue(ItemsSourceProperty, itemsSource);
		}

		// Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.RegisterAttached("ItemsSource", typeof(object), typeof(ComboBoxSynchronizerBehavior), new PropertyMetadata(null, OnChanged));

		public static object GetSelectedItem(ComboBox obj)
		{
			return (object)obj.GetValue(SelectedItemProperty);
		}

		public static void SetSelectedItem(ComboBox obj, object selectedItem)
		{
			obj.SetValue(SelectedItemProperty, selectedItem);
		}

		// Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(ComboBoxSynchronizerBehavior), new PropertyMetadata(null, OnChanged));

		public static WeakAttachedDictionary<ComboBox, string> Holder
		{
			get
			{
				return _holder;
			}

			set
			{
				_holder = value;
			}
		}

		private static void OnChanged(object d, DependencyPropertyChangedEventArgs e)
		{
			var owner = d as ComboBox;
			if (owner != null)
			{
				var container = Holder.GetValue(owner, "holder", () => new ComboBoxContainer(owner));
				container.Update();
			}
		}
	}

#if __ANDROID__
	//getter is not generated by AOT compilation in Android for SelectedItem
	[Android.Runtime.Preserve(AllMembers = true)]
#endif
	[Windows.UI.Xaml.Data.Bindable]
	public partial class ComboBoxContainer : DependencyObject
#if WINDOWS_UWP
		, INotifyPropertyChanged
#endif
	{
		private ComboBox _owner;

#if WINDOWS_UWP
			public event PropertyChangedEventHandler PropertyChanged;
#endif

		public ComboBoxContainer(ComboBox owner)
		{
			_owner = owner;
			_owner.SetBinding(ComboBox.SelectedItemProperty, new SharedBinding { Path = new PropertyPath("InnerSelectedItem"), Source = this, Mode = BindingMode.TwoWay });
		}

		public object InnerSelectedItem
		{
			get { return (object)this.GetValue(InnerSelectedItemProperty); }
			set { this.SetValue(InnerSelectedItemProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty InnerSelectedItemProperty =
			DependencyProperty.RegisterAttached("InnerSelectedItem", typeof(object), typeof(ComboBoxContainer), new PropertyMetadata(null, OnInnerSelectedItemChanged));

		private static void OnInnerSelectedItemChanged(object d, DependencyPropertyChangedEventArgs e)
		{
			var comboBoxContainer = d as ComboBoxContainer;
			if (comboBoxContainer != null)
			{
				if (AreDifferent(e.OldValue, e.NewValue))
				{
					comboBoxContainer.UpdateSelectedItem(e.NewValue);
				}
			}
		}

#if !__ANDROID__ && !__IOS__
		internal static bool AreDifferent(object previousValue, object newValue)
		{
			var type = previousValue?.GetType() ?? newValue?.GetType() ?? typeof(object);
			if (type.GetTypeInfo().IsValueType)
			{
				return !object.Equals(previousValue, newValue);
			}
			else
			{
				return !object.ReferenceEquals(previousValue, newValue);
			}
		}
#endif

		internal void UpdateSelectedItem(object newValue)
		{
			ComboBoxSynchronizerBehavior.SetSelectedItem(_owner, newValue);

#if WINDOWS_UWP
				//needed in Windows to nofify the ComboBox that InnerSelectedItem has changed
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("InnerSelectedItem"));
				}
#endif
		}

		internal void Update()
		{
			var items = ComboBoxSynchronizerBehavior.GetItemsSource(_owner) as IEnumerable;
			if (items == null)
			{
				InnerSelectedItem = null;
				return;
			}

			_owner.ItemsSource = items;

			var selectedItem = ComboBoxSynchronizerBehavior.GetSelectedItem(_owner);
			if (selectedItem != null)
			{
				InnerSelectedItem = selectedItem;
			}
		}
	}
}
#endif
