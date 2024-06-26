﻿// <copyright file="UFCoreFileTools.cs" company="Ultra Force Development">
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

namespace UltraForce.Library.Core.Tools;

/// <summary>
/// File support methods.
/// </summary>
public static class UFCoreFileTools
{
  /// <summary>
  /// Checks if there is an file using an alternative extension, if there is the method will copy the alternative
  /// file to missing file.
  /// 
  /// This method uses `File.Copy` but does not handle any of the exceptions it can throw.
  /// </summary>
  /// <param name="aMissingFileName">
  ///   Full path and file name
  /// </param>
  /// <param name="aRequiredExtension">
  ///   The extension the missing filename must use (including '.')
  /// </param>
  /// <param name="anAlternativeExtension">
  ///   The extension of the alternative file (including '.')
  /// </param>
  /// <returns>
  ///   <c>true</c> if an alternative file was found and a copy was made or 
  ///   if the missing file is actually existing.
  /// </returns>
  public static bool CheckAlternativeFile(
    string aMissingFileName,
    string aRequiredExtension,
    string anAlternativeExtension
  )
  {
    // if the file exists, there is no need to make a copy
    if (File.Exists(aMissingFileName))
    {
      return true;
    }
    // check if the missing full path matches the required extension
    string extension = Path.GetExtension(aMissingFileName);
    if ((extension == null) || !extension.Equals(aRequiredExtension, StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }
    // check if the file using the alternative extension exists
    string alternativeFile = Path.ChangeExtension(aMissingFileName, anAlternativeExtension);
    if ((alternativeFile != null) && File.Exists(alternativeFile))
    {
      // alternative exists, so make copy to missing file
      File.Copy(alternativeFile, aMissingFileName);
      return true;
    }
    // no alternative file found
    return false;
  }
}