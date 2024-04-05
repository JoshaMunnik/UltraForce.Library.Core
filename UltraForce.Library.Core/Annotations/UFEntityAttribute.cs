// <copyright file="UFEntityAttribute.cs" company="Ultra Force Development">
// Ultra Force Library - Copyright (C) 2021 Ultra Force Development
// </copyright>
// <author>Josha Munnik</author>
// <email>josha@ultraforce.com</email>
// <license>
// The MIT License (MIT)
//
// Copyright (C) 2021 Ultra Force Development
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

namespace UltraForce.Library.Core.Annotations;

/// <summary>
/// Indicates that a property maps to a property in an entity
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class UFEntityAttribute : Attribute
{
  #region Properties

  /// <summary>
  /// The name of the other property or empty to use the name of the property this attribute
  /// is set to. 
  /// </summary>
  /// <value>
  /// The other property name.
  /// </value>
  public string Name { get; } = string.Empty;

  /// <summary>
  /// When true the property is not copied to the entity. The property is still copied from
  /// the entity.
  /// </summary>
  public bool ReadOnly { get; set; } = false;

  #endregion

  #region Constructor

  /// <summary>
  /// Creates a new instance of <see cref="UFEntityAttribute"/> using a name.
  /// </summary>
  /// <param name="aName">Entity name to use</param>
  public UFEntityAttribute(string aName)
  {
    this.Name = aName;
  }

  /// <summary>
  /// Creates a new instance of <see cref="UFEntityAttribute"/> using a name and readonly.
  /// </summary>
  /// <param name="aName">Entity name to use</param>
  /// <param name="aReadOnly"><see cref="ReadOnly"/> value</param>
  public UFEntityAttribute(string aName, bool aReadOnly)
  {
    this.Name = aName;
    this.ReadOnly = aReadOnly;
  }

  /// <summary>
  /// Creates a new instance of <see cref="UFEntityAttribute"/> using readonly.
  /// </summary>
  /// <param name="aReadOnly"><see cref="ReadOnly"/> value</param>
  public UFEntityAttribute(bool aReadOnly)
  {
    this.ReadOnly = aReadOnly;
  }

  /// <summary>
  /// Constructs an instance of <see cref="UFEntityAttribute"/> with default values.
  /// </summary>
  public UFEntityAttribute()
  {
  }

  #endregion
}