/*******************************************************************************
* Copyright (c) 2011 Nokia Corporation
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://www.eclipse.org/legal/epl-v10.html
*
* Contributors:
* Comarch team - initial API and implementation
*******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace NedWp
{
    // WARN: As screen rotation is not supported yet thus it was not checed if it actually works as desired on screen rotation
    public partial class MarqueeTextBlock : UserControl
    {
        private const int KDurationBase = 10;
        private const int KOutOfTheScreenWidth = 1000;
        private int Duration { get; set; }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("MarqueeText", typeof(string), typeof(MarqueeTextBlock), new PropertyMetadata(String.Empty, new PropertyChangedCallback(OnMarqueeTextChanged)));
        public string MarqueeText
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value);}
        }

        public static readonly DependencyProperty IsMarqueeAnimationRunningProperty = DependencyProperty.Register("IsMarqueeAnimationRunning", typeof(bool), typeof(MarqueeTextBlock), new PropertyMetadata(false, new PropertyChangedCallback(OnIsMarqueeAnimationRunningChanged)));
        public bool IsMarqueeAnimationRunning
        {
            get { return (bool)GetValue(IsMarqueeAnimationRunningProperty); }
            set { SetValue(IsMarqueeAnimationRunningProperty, value); }
        }

        public void OnMarqueeTextChanged()
        {
            AnimatedTextBlock.Text = (string)GetValue(TextProperty);
            AdjustWidth();
        }

        private void UpdateAnimationRunning()
        {
            if (IsMarqueeAnimationRunning)
            {
                MarqueeAnimation.RepeatBehavior = RepeatBehavior.Forever;
                MarqueeStoryboard.Begin();
            }
            else
            {
                MarqueeAnimation.RepeatBehavior = new RepeatBehavior(0);
                MarqueeStoryboard.Stop();
            }
        }

        public MarqueeTextBlock()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            MarqueeAnimation.Duration = new Duration(TimeSpan.FromSeconds(KDurationBase));
        }

        public void OnContainerSizeChanged(object sender, SizeChangedEventArgs args)
        {
            FrameworkElement parent = Parent as FrameworkElement;
            if (parent != null)
            {
                AdjustWidth();
            }
        }

        public void OnLoaded(object sender, RoutedEventArgs args)
        {
            (Parent as FrameworkElement).SizeChanged += OnContainerSizeChanged;
            AdjustWidth();
            MarqueeStoryboard.Begin();
        }

        private void AdjustAnimationDuration()
        {
            FrameworkElement parent = Parent as FrameworkElement;
            if (parent == null)
                return;
            double realativeWidth = parent.ActualWidth > 0 ? parent.ActualWidth : 1;
            double duration = (1 + (AnimatedTextBlock.ActualWidth / realativeWidth)) * KDurationBase;
            MarqueeAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
        }

        private static void OnMarqueeTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            MarqueeTextBlock userControl = sender as MarqueeTextBlock;
            userControl.OnMarqueeTextChanged();
        }

        private static void OnIsMarqueeAnimationRunningChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            MarqueeTextBlock userControl = sender as MarqueeTextBlock;
            userControl.UpdateAnimationRunning();
        }

        private void AdjustWidth()
        {
            AnimatedTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = AnimatedTextBlock.DesiredSize.Width;
            double widthToBeSet = AnimatedTextBlock.ActualWidth;
            FrameworkElement parent = Parent as FrameworkElement;
            if (parent != null)
            {
                widthToBeSet = parent.ActualWidth < AnimatedTextBlock.ActualWidth ? AnimatedTextBlock.ActualWidth : parent.ActualWidth;
                MarqueeStoryboard.Stop();
                AdjustAnimationDuration();
                Width = widthToBeSet;
                MarqueeAnimation.To = -AnimatedTextBlock.DesiredSize.Width;
                MarqueeAnimation.From = parent.ActualWidth == 0 ? KOutOfTheScreenWidth : parent.ActualWidth; // Workaround: To avoid flicker on screen show up
                MarqueeStoryboard.Begin();
            }
        }
    }
}
