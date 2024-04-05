// <copyright file="UFDateOnlyJsonConverter.cs" company="Ultra Force Development">
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

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UltraForce.Library.Core.Converters;

/// <summary>
/// Converts a DateOnly to text and vice versa. The format is yyyy-MM-dd. 
/// </summary>
public class UFDateOnlyJsonConverter : JsonConverter<DateOnly>
{
  /// <summary>
  /// Format to use for the date
  /// </summary>
  private const string Format = "yyyy-MM-dd";
  
  /// <inheritdoc />
  public override DateOnly Read(
    ref Utf8JsonReader aReader, Type aTypeToConvert, JsonSerializerOptions anOptions
  )
  {
    string? dateText = aReader.GetString();
    if (dateText == null)
    {
      throw new JsonException("Expected string");
    }
    return DateOnly.ParseExact(dateText, Format, CultureInfo.InvariantCulture);
  }

  /// <inheritdoc />
  public override void Write(Utf8JsonWriter aWriter, DateOnly aValue, JsonSerializerOptions anOptions)
  {
    aWriter.WriteStringValue(aValue.ToString(Format, CultureInfo.InvariantCulture));    
  }
}