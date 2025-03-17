// <copyright file="UFCompareToAttribute.cs" company="Ultra Force Development">
// Ultra Force Library - Copyright (C) 2018 Ultra Force Development
// </copyright>
// <author>Josha Munnik</author>
// <email>josha@ultraforce.com</email>
// <license>
// The MIT License (MIT)
//
// Copyright (C) 2025 Ultra Force Development
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

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using UltraForce.Library.Core.Types.Enums;

namespace UltraForce.Library.Core.Annotations;

/// <summary>
/// This attribute can be used to annotate a property that should pass a certain comparison to
/// another property.
/// <para>
/// The type of the property must implement <see cref="IComparable"/>.
/// </para>
/// </summary>
public class UFCompareToAttribute : ValidationAttribute
{
  #region private properties

  /// <summary>
  /// Name of other property
  /// </summary>
  private readonly string m_comparisonProperty;

  /// <summary>
  /// How to compare to the other property
  /// </summary>
  private readonly UFCompareOption m_compareOption;

  #endregion

  #region constructor

  /// <summary>
  /// Constructs a new instance of <see cref="UFCompareToAttribute"/>.
  /// </summary>
  /// <param name="comparisonProperty">Name of other property to compare to</param>
  /// <param name="compareOption">How to compare this property to the other compare</param>
  public UFCompareToAttribute(
    string comparisonProperty,
    UFCompareOption compareOption
  )
  {
    this.m_comparisonProperty = comparisonProperty;
    this.m_compareOption = compareOption;
  }

  #endregion

  #region protected methods

  /// <inheritdoc />
  protected override ValidationResult IsValid(
    object? value,
    ValidationContext validationContext
  )
  {
    if (value == null)
    {
      return new ValidationResult($"Property {validationContext.DisplayName} is null");
    }
    PropertyInfo? property = validationContext.ObjectType.GetProperty(this.m_comparisonProperty);
    if (property == null)
    {
      return new ValidationResult($"Unknown property: {this.m_comparisonProperty}");
    }
    object? otherValue = property.GetValue(validationContext.ObjectInstance);
    if (otherValue == null)
    {
      return new ValidationResult($"Property {this.m_comparisonProperty} is null");
    }
    if (value is IComparable comparable)
    {
      return this.m_compareOption switch
      {
        UFCompareOption.LessThan => this.GetValidationResult(
          comparable.CompareTo(otherValue) < 0,
          $"{validationContext.DisplayName} must be less than {this.m_comparisonProperty}"
        ),
        UFCompareOption.LessThanOrEqual => this.GetValidationResult(
          comparable.CompareTo(otherValue) <= 0,
          $"{validationContext.DisplayName} must be less than or equal to {this.m_comparisonProperty}"
        ),
        UFCompareOption.Equal => this.GetValidationResult(
          comparable.CompareTo(otherValue) == 0,
          $"{validationContext.DisplayName} must be equal to {this.m_comparisonProperty}"
        ),
        UFCompareOption.NotEqual => this.GetValidationResult(
          comparable.CompareTo(otherValue) != 0,
          $"{validationContext.DisplayName} must not be equal to {this.m_comparisonProperty}"
        ),
        UFCompareOption.GreaterThanOrEqual => this.GetValidationResult(
          comparable.CompareTo(otherValue) >= 0,
          $"{validationContext.DisplayName} must be greater than or equal to {this.m_comparisonProperty}"
        ),
        UFCompareOption.GreaterThan => this.GetValidationResult(
          comparable.CompareTo(otherValue) > 0,
          $"{validationContext.DisplayName} must be greater than {this.m_comparisonProperty}"
        ),
        _ => throw new ArgumentOutOfRangeException()
      };
    }
    return new ValidationResult($"{validationContext.DisplayName} is not a comparable type.");
  }

  #endregion

  #region private methods

  /// <summary>
  /// Checks if the result is true, if not returns a validation result with the error message.
  /// </summary>
  /// <param name="result">Result to check</param>
  /// <param name="errorMessage">
  /// Error message to use if no <see cref="ValidationAttribute.ErrorMessage"/> has been set
  /// </param>
  /// <returns>Either a result with an error message or success.</returns>
  private ValidationResult GetValidationResult(
    bool result,
    string errorMessage
  )
  {
    return result
      ? ValidationResult.Success!
      : new ValidationResult(this.ErrorMessage ?? errorMessage);
  }

  #endregion
}