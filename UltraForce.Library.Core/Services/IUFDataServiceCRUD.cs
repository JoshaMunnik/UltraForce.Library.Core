// <copyright file="IUFDataServiceCRUD.cs" company="Ultra Force Development">
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

using UltraForce.Library.Core.Models;

namespace UltraForce.Library.Core.Services;

/// <summary>
/// A definition of data services that offers CRUD methods, assuming the data service maps
/// to a single entity type and each record is identified by a single key value.
/// </summary>
/// <typeparam name="TServiceModel"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public interface IUFDataServiceCRUD<TServiceModel, in TKey, TEntity> : IUFDataService
  where TEntity : class
  where TServiceModel : IUFDataServiceModel<TEntity>, new()
{
  /// <summary>
  /// Finds a record instance for a certain id.
  /// </summary>
  /// <param name="id">Id to find record for</param>
  /// <returns>instance or null when not found</returns>
  public Task<TServiceModel?> FindForIdAsync(TKey id);

  /// <summary>
  /// Gets all records.
  /// </summary>
  /// <returns>a collection of all records</returns>
  public Task<IEnumerable<TServiceModel>> FindAllAsync();

  /// <summary>
  /// Adds a new record to the data.
  /// </summary>
  /// <param name="data">
  /// Record to add. Properties within in this value might get updated.
  /// </param>
  /// <returns>
  /// The record that has been added, this value might be a different instance then data.
  /// </returns>
  public Task<TServiceModel> AddAsync(TServiceModel data);

  /// <summary>
  /// Updates the data with an existing record.
  /// </summary>
  /// <param name="data">
  /// Record to update with. Properties within in this value might get updated.
  /// </param>
  /// <returns>The updated record, this value might be a different instance then data.</returns>
  public Task<TServiceModel> UpdateAsync(TServiceModel data);

  /// <summary>
  /// Removes a record from the data.
  /// </summary>
  /// <param name="data">Record to remove</param>
  public Task RemoveAsync(TServiceModel data);
}
