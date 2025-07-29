// <copyright file="UFBoolJsonConverter.cs" company="Ultra Force Development">
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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace UltraForce.Library.Core.Converters;

/// <summary>
/// Converts a bool to 1 or 0 and vice versa.
/// <remarks>
/// Based on https://stackoverflow.com/a/68073802/968451
/// </remarks>
/// </summary>
public class UFBoolJsonConverter : JsonConverter<bool>
{
  /// <inheritdoc />
  public override bool Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
  {
    return reader.TryGetInt32(out int value) && value == 1;
  }

  /// <inheritdoc />
  public override void Write(
    Utf8JsonWriter writer,
    bool value,
    JsonSerializerOptions options
  )
  {
    writer.WriteNumberValue(value == true ? 1 : 0);
  }
}
