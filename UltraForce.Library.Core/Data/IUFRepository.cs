// <copyright file="IUFRepository.cs" company="Ultra Force Development">
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

namespace UltraForce.Library.Core.Data;

/// <summary>
/// IUFRepository should be used by interface definitions for repository
/// implementations that will inherit from 
/// <see cref="UFRepository{TContext}"/>.
/// </summary>
public interface IUFRepository
{
  /// <summary>
  /// Locks the repository, preventing the updating of underlying data 
  /// storage with every change.
  /// </summary>
  /// <remarks>
  /// Each call to <see cref="LockAsync"/> must be matched with a call to 
  /// <see cref="UnlockAsync"/> before data is saved again.
  /// </remarks>
  Task LockAsync();

  /// <summary>
  /// Unlocks the repository. If this call matches the first call to 
  /// <see cref="LockAsync"/> any pending changes are saved.
  /// </summary>
  Task UnlockAsync();
}