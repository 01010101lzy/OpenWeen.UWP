﻿using OpenWeen.UWP.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace OpenWeen.UWP.Common.Controls
{
    public sealed partial class SplitViewPaneList : UserControl
    {
        public List<HeaderModel> ItemsSource { get; set; }
        public SplitViewPaneList()
        {
            this.InitializeComponent();
        }
        
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(SplitViewPaneList), new PropertyMetadata(-1, OnSelectedIndexChanged));

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SplitViewPaneList).ChangeSelectedIndex((int)e.NewValue, (int)e.OldValue);
        }
        public void ChangeSelectedIndex(int newValue, int oldValue)
        {
            try
            {
                ItemsSource[newValue].IsActive = true;
                ItemsSource[oldValue].IsActive = false;
            }
            catch { }
        }

        private void ItemsControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SelectedIndex = ItemsSource.FindIndex(item => item == (e.OriginalSource as FrameworkElement).DataContext as HeaderModel);
        }
    }
}