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
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NedWp
{

    public class IntegerToVisibilityConverter : IValueConverter
    {
        public const string InvertVisibilityParam = "ShowWhenZero";

        public object Convert(object integerValue, Type targetType, object parameter, CultureInfo culture)
        {
            bool showWhenZero = (parameter as string == IntegerToVisibilityConverter.InvertVisibilityParam);
            bool integerIsZero = (System.Convert.ToInt32(integerValue) == 0);
            return (showWhenZero == integerIsZero) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
