// <copyright file="UFTestTools.cs" company="Ultra Force Development">
// Ultra Force Library - Copyright (C) 2024 Ultra Force Development
// </copyright>
// <author>Josha Munnik</author>
// <email>josha@ultraforce.com</email>
// <license>
// The MIT License (MIT)
//
// Copyright (C) 2024 Ultra Force Development
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

using UltraForce.Library.NetStandard.Testing;
using Xunit;

namespace UltraForce.Library.Core.Tools;

/// <summary>
/// THis class contains static methods to support unit testing with xUnit.
/// </summary>
public static class UFTestTools
{
  /// <summary>
  /// Checks if a list of items has unique values for a certain property.
  /// </summary>
  /// <param name="list">List to check</param>
  /// <param name="getValue">A function to get the value</param>
  /// <typeparam name="TObject">Object to get value from</typeparam>
  /// <typeparam name="TValue">Value type</typeparam>
  /// <exception>If a value is found that is in the list at least two times</exception>
  public static void HasUniqueValues<TObject, TValue>(
    IEnumerable<TObject> list,
    Func<TObject, TValue> getValue
  )
    where TValue : IEquatable<TValue>
  {
    List<TValue> values = [];
    foreach (TObject item in list)
    {
      TValue value = getValue(item);
      Assert.DoesNotContain(value, values);
      values.Add(value);
    }
  }

  /// <summary>
  /// Checks if all properties are the same.
  /// </summary>
  /// <param name="expected"></param>
  /// <param name="actual"></param>
  /// <param name="notEqualProperties"></param>
  /// <typeparam name="T"></typeparam>
  /// <exception>If properties are not equal</exception>
  public static void AssertEqualProperties<T>(
    T expected,
    T actual,
    IEnumerable<string>? notEqualProperties = null
  )
  {
    UFPropertiesComparer<T> comparer = new(true, notEqualProperties);
    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
    comparer.Equals(expected, actual);
  }

  /// <summary>
  /// Checks if a list contains all the items (and not more) of an another list. The method succeeds
  /// if the size are equal and all items are found within the list. The method does not look at
  /// the order of the items.
  /// </summary>
  /// <param name="actualList"></param>
  /// <param name="expectedList"></param>
  /// <param name="notEqualProperties"></param>
  /// <typeparam name="T"></typeparam>
  /// <exception cref="Exception">
  /// When an item can not be found or lists are not equal in size
  /// </exception>
  public static void AssertEqualList<T>(
    IEnumerable<T> expectedList,
    IEnumerable<T> actualList,
    IEnumerable<string>? notEqualProperties = null
  )
  {
    UFPropertiesComparer<T> comparer = new(false, notEqualProperties);
    AssertEqualList(
      expectedList,
      actualList,
      (
        first,
        second
      ) => comparer.Equals(first, second)
    );
  }

  /// <summary>
  /// Checks if a list contains all the items (and not more) of another list. The method succeeds
  /// if the size are equal and all items are found within the list.
  /// </summary>
  /// <param name="expectedEnumerable"></param>
  /// <param name="actualEnumerable"></param>
  /// <param name="comparer"></param>
  /// <typeparam name="TFirst"></typeparam>
  /// <typeparam name="TSecond"></typeparam>
  /// <exception cref="Exception">
  /// When an item can not be found or lists are not equal in size
  /// </exception>
  public static void AssertEqualList<TFirst, TSecond>(
    IEnumerable<TFirst> expectedEnumerable,
    IEnumerable<TSecond> actualEnumerable,
    Func<TFirst, TSecond, bool> comparer
  )
  {
    List<TSecond> actualList = actualEnumerable.ToList();
    List<TFirst> expectedList = expectedEnumerable.ToList();
    Assert.Equal(expectedList.Count, actualList.Count);
    if (
      !expectedList.All(expected => actualList.Any(actual => comparer(expected, actual))
      )
    )
    {
      throw new Exception("Item not found in list");
    }
  }
}
