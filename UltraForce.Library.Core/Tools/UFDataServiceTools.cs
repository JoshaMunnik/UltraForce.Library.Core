// <copyright file="UFDataServiceTools.cs" company="Ultra Force Development">
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

using System.Collections.ObjectModel;
using UltraForce.Library.Core.Models;
using UltraForce.Library.Core.Services;

namespace UltraForce.Library.Core.Tools;

/// <summary>
/// Support methods for <see cref="IUFDataServiceModel{TEntity}"/> and <see cref="IUFDataService"/>
/// </summary>
public static class UFDataServiceTools
{
  /// <summary>
  /// Creates a service model instance for an entity. 
  /// </summary>
  /// <param name="anEntity">Entity to create instance for</param>
  /// <typeparam name="TServiceModel">
  /// A class implementing <see cref="IUFDataServiceModel{TEntity}"/>
  /// </typeparam>
  /// <typeparam name="TEntity">The database entity class</typeparam>
  /// <returns>service model instance or null if anEntity was null</returns>
  public static async Task<TServiceModel> BuildFromAsync<TServiceModel, TEntity>(
    TEntity anEntity
  )
    where TEntity : class
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
  {
    TServiceModel result = new();
    await result.CopyFromEntityAsync(anEntity);
    return result;
  }

  /// <summary>
  /// Creates a service model instance for an entity. 
  /// </summary>
  /// <param name="anEntity">Entity to create instance for</param>
  /// <typeparam name="TServiceModel">A class implementing <see cref="IUFDataServiceModel{TEntity}"/></typeparam>
  /// <typeparam name="TEntity">The database entity class</typeparam>
  /// <returns>service model instance or null if anEntity was null</returns>
  public static async Task<TServiceModel?> BuildNullableFromAsync<TServiceModel, TEntity>(
    TEntity? anEntity
  )
    where TEntity : class
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
  {
    if (anEntity == null)
    {
      return null;
    }
    return await BuildFromAsync<TServiceModel, TEntity>(anEntity);
  }

  /// <summary>
  /// Creates a list of service model instances from a list of entities. 
  /// </summary>
  /// <param name="anEntities">Entities to create instance for</param>
  /// <typeparam name="TServiceModel">
  /// A class implementing <see cref="IUFDataServiceModel{TEntity}"/>
  /// </typeparam>
  /// <typeparam name="TEntity">The database entity class</typeparam>
  /// <returns>A list of service model instance</returns>
  public static async Task<IEnumerable<TServiceModel>> BuildFromAsync<TServiceModel, TEntity>(
    IEnumerable<TEntity> anEntities
  )
    where TEntity : class
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
  {
    Collection<TServiceModel> result = [];
    // don't use Task.WhenAll here, because it might cause multiple threads to access the 
    // context at the same time.
    foreach (TEntity entity in anEntities)
    {
      result.Add(await BuildFromAsync<TServiceModel, TEntity>(entity));
    }
    return result;
  }
}