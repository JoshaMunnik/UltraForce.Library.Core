// <copyright file="UFDataServiceExtensions.cs" company="Ultra Force Development">
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
using UltraForce.Library.Core.Services;
using UltraForce.Library.Core.Tools;
using UltraForce.Library.Core.Types.Enums;

namespace UltraForce.Library.Core.Extensions;

/// <summary>
/// Defines extension methods in relation to <see cref="IUFDataService"/>.
/// </summary>
public static class UFDataServiceExtensions
{
  /// <summary>
  /// Uses <see cref="UFDataServiceTools.BuildNullableFromAsync{TServiceModel,TEntity}"/>
  /// to create a data service model from a database entity. It supports nullable entities.
  /// </summary>
  /// <param name="entity">Entity to create data service model instance from</param>
  /// <typeparam name="TServiceModel">Type of the data service model</typeparam>
  /// <typeparam name="TEntity">Type of the entity</typeparam>
  /// <returns>Data service instance or null if the entity is null</returns>
  public static async Task<TServiceModel?> AsNullableModelAsync<TServiceModel, TEntity>(
    this Task<TEntity?> entity
  )
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
    where TEntity : class
  {
    return await UFDataServiceTools.BuildNullableFromAsync<TServiceModel, TEntity>(
      await entity
    );
  }

  /// <summary>
  /// Uses
  /// <see cref="UFDataServiceTools.BuildFromAsync{TServiceModel,TEntity}(TEntity)"/>
  /// to create a data service model from a database entity. It supports nullable entities.
  /// </summary>
  /// <param name="entity">Entity to create data service model instance from</param>
  /// <typeparam name="TServiceModel">Type of the data service model</typeparam>
  /// <typeparam name="TEntity">Type of the entity</typeparam>
  /// <returns>Data service instance or null if the entity is null</returns>
  public static async Task<TServiceModel?> AsNullableModelAsync<TServiceModel, TEntity>(
    this ValueTask<TEntity?> entity
  )
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
    where TEntity : class
  {
    return await UFDataServiceTools.BuildNullableFromAsync<TServiceModel, TEntity>(
      await entity
    );
  }

  /// <summary>
  /// Uses <see cref="UFDataServiceTools.BuildFromAsync{TServiceModel,TEntity}(TEntity)"/> to
  /// create a data service model from a database entity. It supports nullable entities.
  /// </summary>
  /// <param name="entity">Entity to create data service model instance from</param>
  /// <typeparam name="TServiceModel">Type of the data service model</typeparam>
  /// <typeparam name="TEntity">Type of the entity</typeparam>
  /// <returns>Data service instance or null if the entity is null</returns>
  public static async Task<TServiceModel?> AsNullableModelAsync<TServiceModel, TEntity>(
    this TEntity? entity
  )
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
    where TEntity : class
  {
    return await UFDataServiceTools.BuildNullableFromAsync<TServiceModel, TEntity>(entity);
  }

  /// <summary>
  /// Uses <see cref="UFDataServiceTools.BuildFromAsync{TServiceModel,TEntity}(TEntity)"/> to
  /// create a data service model from a database entity.
  /// </summary>
  /// <param name="entity">Entity to create data service model instance from</param>
  /// <typeparam name="TServiceModel">Type of the data service model</typeparam>
  /// <typeparam name="TEntity">Type of the entity</typeparam>
  /// <returns>Data service instance</returns>
  public static async Task<TServiceModel> AsModelAsync<TServiceModel, TEntity>(
    this Task<TEntity> entity
  )
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
    where TEntity : class
  {
    return await UFDataServiceTools.BuildFromAsync<TServiceModel, TEntity>(await entity);
  }

  /// <summary>
  /// Uses <see cref="UFDataServiceTools.BuildFromAsync{TServiceModel,TEntity}(TEntity)"/> to
  /// create a data service model from a database entity.
  /// </summary>
  /// <param name="entity">Entity to create data service model instance from</param>
  /// <typeparam name="TServiceModel">Type of the data service model</typeparam>
  /// <typeparam name="TEntity">Type of the entity</typeparam>
  /// <returns>Data service instance</returns>
  public static async Task<TServiceModel> AsModelAsync<TServiceModel, TEntity>(
    this TEntity entity
  )
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
    where TEntity : class
  {
    return await UFDataServiceTools.BuildFromAsync<TServiceModel, TEntity>(entity);
  }

  /// <summary>
  /// Uses <see cref="UFDataServiceTools"/> to convert a list of entities to a list of data service
  /// model instances.
  /// </summary>
  /// <param name="entities">Entities to create data service model instances for</param>
  /// <typeparam name="TServiceModel">Type of the data service model</typeparam>
  /// <typeparam name="TEntity">Type of the entity</typeparam>
  /// <returns>Data service instances</returns>
  public static async Task<IEnumerable<TServiceModel>> AsModelAsync<TServiceModel, TEntity>(
    this IEnumerable<TEntity> entities
  )
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
    where TEntity : class
  {
    return await UFDataServiceTools.BuildFromAsync<TServiceModel, TEntity>(entities);
  }
}
