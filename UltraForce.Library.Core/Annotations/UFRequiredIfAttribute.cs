// <copyright file="UFRequiredIfAttribute.cs" company="Ultra Force Development">
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

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace UltraForce.Library.Core.Annotations;

/// <summary>
/// Provides conditional validation based on related property value.
/// </summary>
/// <remarks>
/// Based on code from:
/// https://stackoverflow.com/questions/26354853/conditionally-required-property-using-data-annotations
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public sealed class UFRequiredIfAttribute : ValidationAttribute
{
  #region Properties

  /// <summary>
  /// Gets or sets the other property name that will be used during
  /// validation.
  /// </summary>
  /// <value>
  /// The other property name.
  /// </value>
  public string OtherProperty { get; }

  /// <summary>
  /// Gets or sets the display name of the other property.
  /// </summary>
  /// <value>
  /// The display name of the other property.
  /// </value>
  public string OtherPropertyDisplayName { get; set; } = null!;

  /// <summary>
  /// Gets or sets the other property value that will be relevant for
  /// validation.
  /// </summary>
  /// <value>
  /// The other property value.
  /// </value>
  public object OtherPropertyValue { get; }

  /// <summary>
  /// Gets or sets a value indicating whether other property's value should
  /// match or differ from provided other property's value (default is
  /// <c>false</c>).
  /// </summary>
  /// <value>
  ///   <c>true</c> if other property's value validation should be
  ///   inverted; otherwise, <c>false</c>.
  /// </value>
  /// <remarks>
  /// How this works
  /// - true: validated property is required when other property doesn't
  ///   equal provided value
  /// - false: validated property is required when other property
  ///   matches provided value
  /// </remarks>
  public bool IsInverted { get; set; }

  /// <summary>
  /// Gets a value that indicates whether the attribute requires
  /// validation context.
  /// </summary>
  /// <returns>
  /// <c>true</c> if the attribute requires validation context;
  /// otherwise, <c>false</c>.
  /// </returns>
  public override bool RequiresValidationContext => true;

  #endregion

  #region Constructor

  /// <summary>
  /// Creates a new instance of <see cref="UFRequiredIfAttribute"/>.
  /// </summary>
  /// <param name="otherProperty">
  /// The other property.
  /// </param>
  /// <param name="otherPropertyValue">
  /// The other property value.
  /// </param>
  public UFRequiredIfAttribute(
    string otherProperty,
    object otherPropertyValue
  ) : base("'{0}' is required because '{1}' has a value {3}'{2}'.")
  {
    this.OtherProperty = otherProperty;
    this.OtherPropertyValue = otherPropertyValue;
    this.IsInverted = false;
  }

  #endregion

  /// <summary>
  /// Applies formatting to an error message, based on the data field
  /// where the error occurred.
  /// </summary>
  /// <param name="name">The name to include in the formatted message.</param>
  /// <returns>
  /// An instance of the formatted error message.
  /// </returns>
  public override string FormatErrorMessage(
    string name
  )
  {
    return string.Format(
      CultureInfo.CurrentCulture,
      base.ErrorMessageString,
      name,
      this.OtherPropertyDisplayName ?? this.OtherProperty,
      this.OtherPropertyValue,
      this.IsInverted ? "other than " : "of "
    );
  }

  /// <summary>
  /// Validates the specified value with respect to the current
  /// validation attribute.
  /// </summary>
  /// <param name="value">
  /// The value to validate.
  /// </param>
  /// <param name="validationContext">
  /// The context information about the validation operation.
  /// </param>
  /// <returns>
  /// An instance of the
  /// <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" />
  /// class.
  /// </returns>
  protected override ValidationResult? IsValid(
    object? value,
    ValidationContext? validationContext
  )
  {
    ArgumentNullException.ThrowIfNull(validationContext);
    PropertyInfo? otherProperty = validationContext.ObjectType.GetProperty(
      this.OtherProperty
    );
    if (otherProperty == null)
    {
      return new ValidationResult(
        string.Format(
          CultureInfo.CurrentCulture,
          "Could not find a property named '{0}'.",
          this.OtherProperty
        )
      );
    }
    object? otherValue = otherProperty.GetValue(validationContext.ObjectInstance);
    // check if this value is actually required and validate it
    if ((this.IsInverted || !Equals(otherValue, this.OtherPropertyValue)) &&
        (!this.IsInverted || Equals(otherValue, this.OtherPropertyValue)))
    {
      return ValidationResult.Success;
    }
    return value switch
    {
      null => new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName)),
      // additional check for strings so they're not empty
      string textValue when textValue.Trim().Length == 0 => new ValidationResult(
        this.FormatErrorMessage(validationContext.DisplayName)
      ),
      _ => ValidationResult.Success
    };
  }
}
