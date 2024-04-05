// <copyright file="UFEnumExtensions.cs" company="Ultra Force Development">
// Ultra Force Library - Copyright (C) 2018 Ultra Force Development
// </copyright>
// <author>Josha Munnik</author>
// <email>josha@ultraforce.com</email>
// <license>
// The MIT License (MIT)
//
// Copyright (C) 2018 Ultra Force Development
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to 
// deal in the Software without restriction, including without limitation the 
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
// IN THE SOFTWARE.
// </license>

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using UltraForce.Library.NetStandard.Annotations;
using UltraForce.Library.NetStandard.Extensions;

namespace UltraForce.Library.Core.Extensions;

/// <summary>
/// Defines extension method for use with <see cref="Enum"/> type.
/// </summary>
public static class UFEnumExtensions
{
  /// <summary>
  /// Get the description value of an enum. Try to get a value from <see cref="DisplayAttribute"/>,
  /// <see cref="UFDescriptionAttribute"/> and <see cref="DescriptionAttribute"/>. 
  /// </summary>
  /// <param name="anEnumerationValue">Enumeration value.</param>
  /// <returns>
  /// The value of a description attribute or enum value converted to string.
  /// </returns>
  public static string GetDisplayDescription(this Enum anEnumerationValue)
  {
    // try to get attribute for field value
    return
      anEnumerationValue.GetAttribute<DisplayAttribute>()?.Description ??
      anEnumerationValue.GetAttribute<UFDescriptionAttribute>()?.Description ??
      anEnumerationValue.GetAttribute<DescriptionAttribute>()?.Description ??
      anEnumerationValue.GetDescription();
  }

  /// <summary>
  /// Get the name value of an enum. Try to get a value from <see cref="DisplayAttribute"/>,
  /// <see cref="UFDescriptionAttribute"/> and <see cref="DisplayNameAttribute"/>. 
  /// </summary>
  /// <param name="anEnumerationValue">Enumeration value.</param>
  /// <returns>
  /// The value of a description attribute or enum value converted to string.
  /// </returns>
  public static string GetDisplayName(this Enum anEnumerationValue)
  {
    // try to get attribute for field value
    return
      anEnumerationValue.GetAttribute<DisplayAttribute>()?.Name ??
      anEnumerationValue.GetAttribute<DisplayNameAttribute>()?.DisplayName ??
      anEnumerationValue.GetAttribute<UFDescriptionAttribute>()?.Name ??
      anEnumerationValue.ToString();
  }
}