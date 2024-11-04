// <copyright file="UFDataServiceFromContext.cs" company="Ultra Force Development">
// Ultra Force Library - Copyright (C) 2023 Ultra Force Development
// </copyright>
// <author>Josha Munnik</author>
// <email>josha@ultraforce.com</email>
// <license>
// The MIT License (MIT)
//
// Copyright (C) 2023 Ultra Force Development
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

using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using UltraForce.Library.Core.Extensions;
using UltraForce.Library.Core.Models;
using UltraForce.Library.Core.Types.Enums;

namespace UltraForce.Library.Core.Services;

/// <summary>
/// <see cref="UFDataServiceFromContext{TContext}" /> is a base class to implement a data
/// service using a <see cref="DbContext"/> to manage underlying entities. 
/// <para>
/// The class implements a locking mechanism to minimize the
/// <see cref="DbContext.SaveChanges()"/> calls.
/// </para>
/// </summary>
/// <remarks>
/// Interface definition for the data service should inherit from <see cref="IUFDataService"/>.
/// </remarks>
/// <typeparam name="TContext">The database context type</typeparam>
public class UFDataServiceFromContext<TContext> : IUFDataService, IDisposable, IAsyncDisposable
  where TContext : DbContext
{
  #region private variables

  /// <summary>
  /// Number of calls to lock
  /// </summary>
  private int m_lockCount;

  /// <summary>
  /// When true Entity Framework tracking is disabled
  /// </summary>
  private readonly bool m_disableTracking;

  /// <summary>
  /// Maps TServiceModel to the primary key property.
  /// </summary>
  // ReSharper disable once StaticMemberInGenericType
  private static readonly Dictionary<Type, PropertyInfo> s_primaryKeys = new();

  #endregion

  #region constructors & IDisposable

  /// <summary>
  /// Constructs an instance of <see cref="UFDataServiceFromContext{TContext}" />.
  /// </summary>
  /// <param name="aContext">
  /// Database context to use
  /// </param>
  /// <param name="aDisableTracking">
  /// When true tell <see cref="DbContext.ChangeTracker"/> to stop tracking and reset the state
  /// of any tracked entry to <see cref="EntityState.Detached"/> with
  /// the <see cref="SaveChangesAsync"/> call.
  /// <para>
  /// To disable tracking everywhere, use
  /// <code>{context}.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;</code>
  /// </para>
  /// 
  /// </param>
  protected UFDataServiceFromContext(TContext aContext, bool aDisableTracking = false)
  {
    this.Context = aContext;
    this.Changed = false;
    this.m_lockCount = 0;
    this.m_disableTracking = aDisableTracking;
    if (aDisableTracking)
    {
      this.Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
      this.Context.ChangeTracker.AutoDetectChangesEnabled = false;
    }
  }

  /// <inheritdoc />
  public async void Dispose()
  {
    await this.DisposeAsync();
  }

  /// <inheritdoc />
  public virtual ValueTask DisposeAsync()
  {
    this.Context = null;
    return ValueTask.CompletedTask;
  }

  #endregion

  #region IUFDataService

  /// <inheritdoc />
  public Task LockAsync()
  {
    this.m_lockCount++;
    return Task.CompletedTask;
  }

  /// <inheritdoc />
  public async Task UnlockAsync()
  {
    if (this.m_lockCount == 0)
    {
      throw new Exception("Trying to unlock a non-locked instance");
    }
    this.m_lockCount--;
    if ((this.m_lockCount == 0) && this.Changed)
    {
      await this.SaveChangesAsync();
      if (this.m_disableTracking)
      {
        this.DetachTrackedEntries();
      }
    }
  }

  #endregion

  #region protected properties

  /// <summary>
  /// The context or null if the service has been disposed.
  /// </summary>
  protected TContext? Context { get; private set; }

  /// <summary>
  /// This property is true when the data service is locked and there is at least one change
  /// pending.
  /// </summary>
  protected bool Changed { get; private set; }

  #endregion

  #region protected methods

  /// <summary>
  /// Stops an entity from being tracked by the entity framework.
  /// </summary>
  /// <param name="anEntity"></param>
  protected void DetachEntity(object anEntity)
  {
    this.Context!.Entry(anEntity).State = EntityState.Detached;
  }

  /// <summary>
  /// Finds a entity for a certain id and convert it to a service model.
  /// </summary>
  /// <param name="anId">Id to find entity for</param>
  /// <typeparam name="TServiceModel">Service model to convert to</typeparam>
  /// <typeparam name="TEntity">Type of entity</typeparam>
  /// <typeparam name="TKey">Type of id</typeparam>
  /// <returns>Service model instance or null if no entity could be found</returns>
  protected async Task<TServiceModel?> FindForIdAsync<TServiceModel, TEntity, TKey>(TKey anId)
    where TEntity : class, new()
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
  {
    this.CheckContext();
    return await this
      .Context!
      .Set<TEntity>()
      .FindAsync(anId)
      .AsNullableModelAsync<TServiceModel, TEntity>();
  }

  /// <summary>
  /// Finds all entities and convert them to a service model.
  /// </summary>
  /// <typeparam name="TServiceModel">Service model to convert to</typeparam>
  /// <typeparam name="TEntity">Entity type</typeparam>
  /// <returns>List of service model instances</returns>
  protected async Task<IEnumerable<TServiceModel>> FindAllAsync<TServiceModel, TEntity>()
    where TEntity : class, new()
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
  {
    this.CheckContext();
    if (this.m_disableTracking)
    {
      return await this.Context!
        .Set<TEntity>()
        .AsNoTracking()
        .AsModelAsync<TServiceModel, TEntity>();
    }
    else
    {
      return await this.Context!
        .Set<TEntity>()
        .AsModelAsync<TServiceModel, TEntity>();
    }
  }

  /// <summary>
  /// Creates and adds an entity from a service model. Since aData might be an immutable instance,
  /// after adding the entity a new service model instance is created from the added entity that
  /// might include updated fields (like the id of the entity).
  /// </summary>
  /// <param name="aData">Service model instance to add</param>
  /// <typeparam name="TServiceModel">Type of service model</typeparam>
  /// <typeparam name="TEntity">Entity to add</typeparam>
  /// <returns>The service model build from the added entity; this is a new instance</returns>
  protected async Task<TServiceModel> AddAsync<TServiceModel, TEntity>(TServiceModel aData)
    where TEntity : class, new()
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
  {
    this.CheckContext();
    TEntity entity = new();
    await aData.CopyToEntityAsync(entity, UFEntityAction.Add);
    await this.Context!.Set<TEntity>().AddAsync(entity);
    await this.SaveChangesAsync();
    TServiceModel result = new();
    await result.CopyFromEntityAsync(entity);
    return result;
  }

  /// <summary>
  /// Updates an entity in the database with the data from a service model instance. Since
  /// aData might be immutable, a new service model instance is created after updating the
  /// entity in the database. The new service model might include updated fields (like a modified
  /// date/time field). 
  /// </summary>
  /// <param name="aData">Data to update entity with</param>
  /// <typeparam name="TServiceModel">Service model type</typeparam>
  /// <typeparam name="TEntity">Entity type</typeparam>
  /// <returns>The service model build from the updated entity; this is a new instance</returns>
  protected async Task<TServiceModel> UpdateAsync<TServiceModel, TEntity>(TServiceModel aData)
    where TEntity : class, new()
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
  {
    this.CheckContext();
    TEntity? entity = await this.Context!.Set<TEntity>()
      .FindAsync(
        this.GetPrimaryKeyFromServiceModel<TServiceModel, TEntity, object>(aData)
      );
    if (entity == null)
    {
      throw new Exception("Trying to update a non-existing entity");
    }
    await aData.CopyToEntityAsync(entity, UFEntityAction.Update);
    this.Context.Set<TEntity>().Update(entity);
    await this.SaveChangesAsync();
    TServiceModel result = new();
    await result.CopyFromEntityAsync(entity);
    return result;
  }

  /// <summary>
  /// Removes an entity represented by a service model instance.
  /// </summary>
  /// <param name="aData">Data that represents an entity</param>
  /// <typeparam name="TServiceModel">Service model type</typeparam>
  /// <typeparam name="TEntity">Entity type</typeparam>
  protected async Task RemoveAsync<TServiceModel, TEntity>(TServiceModel aData)
    where TEntity : class, new()
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
  {
    this.CheckContext();
    TEntity? entity = await this.Context!.Set<TEntity>()
      .FindAsync(
        this.GetPrimaryKeyFromServiceModel<TServiceModel, TEntity, object>(aData)
      );
    if (entity == null)
    {
      throw new Exception("Trying to remove a non-existing entity");
    }
    this.Context.Set<TEntity>().Remove(entity);
    await this.SaveChangesAsync();
    this.DetachEntity(entity);
  }

  /// <summary>
  /// Saves the changes when the instance is not locked, else set an internal
  /// flag to indicate data needs to be saved.
  /// <para>
  /// If tracking was disabled, this method will also call 
  /// <see cref="DetachTrackedEntries"/>.
  /// </para>
  /// </summary>
  protected async Task SaveChangesAsync()
  {
    this.CheckContext();
    if (this.m_lockCount == 0)
    {
      this.Changed = false;
      await this.Context!.SaveChangesAsync();
      if (this.m_disableTracking)
      {
        this.DetachTrackedEntries();
      }
    }
    else
    {
      this.Changed = true;
    }
  }

  /// <summary>
  /// Sets the changed entries that have <see cref="EntityState.Added"/>,
  /// <see cref="EntityState.Modified"/> or <see cref="EntityState.Deleted"/> to
  /// <see cref="EntityState.Detached"/>.
  /// </summary>
  /// <remarks>
  /// Based on code from:
  /// https://stackoverflow.com/questions/27423059/how-do-i-clear-tracked-entities-in-entity-framework
  /// </remarks>
  protected void DetachTrackedEntries()
  {
    this.CheckContext();
    IEnumerable<EntityEntry> changedEntriesCopy = this.Context!.ChangeTracker
      .Entries()
      .Where(entry =>
        entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
          or EntityState.Unchanged
      )
      .AsEnumerable();
    foreach (EntityEntry entity in changedEntriesCopy)
    {
      this.Context.Entry(entity.Entity).State = EntityState.Detached;
    }
  }

  /// <summary>
  /// Executes an action within a transaction. If a transaction is already active, the action
  /// just gets executed without using a transaction assuming the commit or rollback is handled
  /// by other code.
  /// </summary>
  /// <param name="anAction">Action to execute</param>
  /// <exception>
  /// Any exception thrown by anAction will rollback the transaction and gets rethrown
  /// </exception>
  protected async Task TransactionAsync(Func<Task> anAction)
  {
    this.CheckContext();
    if (this.Context!.Database.CurrentTransaction != null)
    {
      await anAction();
      return;
    }
    await using IDbContextTransaction transaction =
      await this.Context.Database.BeginTransactionAsync();
    try
    {
      await anAction();
      await transaction.CommitAsync();
    }
    catch (Exception)
    {
      await transaction.RollbackAsync();
      throw;
    }
  }

  /// <summary>
  /// Executes an action within a transaction. If a transaction is already active, the action just
  /// gets executed without using a transaction assuming the commit or rollback is handled by
  /// other code.
  /// </summary>
  /// <param name="anAction">Action to execute</param>
  /// <returns>The result of the action</returns>
  /// <exception>
  /// Any exception thrown by anAction will rollback the transaction and gets rethrown
  /// </exception>
  protected async Task<T> TransactionAsync<T>(Func<Task<T>> anAction)
  {
    this.CheckContext();
    if (this.Context!.Database.CurrentTransaction != null)
    {
      return await anAction();
    }
    await using IDbContextTransaction transaction =
      await this.Context.Database.BeginTransactionAsync();
    try
    {
      T result = await anAction();
      await transaction.CommitAsync();
      return result;
    }
    catch (Exception)
    {
      await transaction.RollbackAsync();
      throw;
    }
  }

  /// <summary>
  /// Swaps values of two records in a table using a single update statement (so that any unique
  /// index constraint does not generate an error). 
  /// <para>
  /// Code based on: https://stackoverflow.com/a/8109360/968451
  /// </para>
  /// <para>
  /// The default implementation executes a SQL statement directly, it assumes the database used is
  /// MSSQL.
  /// </para>
  /// </summary>
  /// <param name="aTableName">Name of table</param>
  /// <param name="aColumnName">Name of column</param>
  /// <param name="aFirstId">First id to swap value of</param>
  /// <param name="aSecondId">Second id to swap value of</param>
  /// <param name="aModifiedName">When non null, assign DateTime.Now to this column</param>
  /// <typeparam name="TEntity">Type of the entity record</typeparam>
  /// <typeparam name="TKey">Type of the id values</typeparam>
  [SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.")]
  protected virtual async Task SwapAsync<TEntity, TKey>(
    string aTableName,
    string aColumnName,
    TKey aFirstId,
    TKey aSecondId,
    string? aModifiedName = null
  ) where TEntity : class 
    where TKey : notnull
  {
    this.CheckContext();
    string primaryKey = this.GetPrimaryKeyNameFromEntity<TEntity>();
    if (aModifiedName == null)
    {
      await this.Context!.Database.ExecuteSqlRawAsync(
        $@"
            UPDATE [{aTableName}]
            SET 
              [{aColumnName}] = CASE [{primaryKey}]
                WHEN {{0}} THEN (SELECT [{aColumnName}] FROM [{aTableName}] WHERE [{primaryKey}] = {{1}})
                WHEN {{1}} THEN (SELECT [{aColumnName}] FROM [{aTableName}] WHERE [{primaryKey}] = {{0}})
              END
            WHERE [{primaryKey}] IN ({{0}}, {{1}})
          ",
        aFirstId,
        aSecondId
      );
    }
    else
    {
      await this.Context!.Database.ExecuteSqlRawAsync(
        $@"
            UPDATE [{aTableName}]
            SET 
              [{aModifiedName}] = {{2}}, 
              [{aColumnName}] = CASE [{primaryKey}]
                WHEN {{0}} THEN (SELECT [{aColumnName}] FROM [{aTableName}] WHERE [{primaryKey}] = {{1}})
                WHEN {{1}} THEN (SELECT [{aColumnName}] FROM [{aTableName}] WHERE [{primaryKey}] = {{0}})
              END
            WHERE [{primaryKey}] IN ({{0}}, {{1}})
          ",
        aFirstId,
        aSecondId,
        DateTime.Now
      );
    }
  }

  /// <summary>
  /// Determine primary key and swap two entities calling <see cref="SwapAsync{TEntity,TKey}"/>. 
  /// <para>
  /// After swapping the method will call <see cref="EntityEntry.ReloadAsync"/> to reload
  /// both entities from the database.
  /// </para>
  /// </summary>
  /// <param name="aTableName">Name of table</param>
  /// <param name="aColumnName">Name of column</param>
  /// <param name="aFirst">First entity to swap value of</param>
  /// <param name="aSecond">Second entity to swap value of</param>
  /// <param name="aModifiedName">When non null, assign DateTime.Now to this column</param>
  /// <typeparam name="T"></typeparam>
  protected virtual async Task SwapAsync<T>(
    string aTableName,
    string aColumnName,
    T aFirst,
    T aSecond,
    string? aModifiedName = null
  ) where T : class
  {
    this.CheckContext();
    string primaryKey = this.GetPrimaryKeyNameFromEntity<T>();
    PropertyInfo? primaryProperty = (typeof(T)).GetProperty(primaryKey);
    if (primaryProperty == null)
    {
      throw new Exception(@"Can not find primary key property for {primaryKey}");
    }
    await this.SwapAsync<T, object>(aTableName,
      aColumnName,
      primaryProperty.GetValue(aFirst, null)!,
      primaryProperty.GetValue(aSecond, null)!,
      aModifiedName
    );
    await this.Context!.Entry(aFirst).ReloadAsync();
    await this.Context!.Entry(aSecond).ReloadAsync();
  }

  /// <summary>
  /// Returns the primary key of an entity
  /// <para>
  /// Code based on: https://stackoverflow.com/a/34993637/968451
  /// </para>
  /// <remarks>
  /// Subclasses can override this method to return the key directly for better performance.
  /// </remarks>
  /// </summary>
  /// <typeparam name="TEntity">Type of the entity</typeparam>
  /// <returns>primary key name</returns>
  /// <exception>If no primary key could be determined</exception>
  protected virtual string GetPrimaryKeyNameFromEntity<TEntity>() where TEntity : class
  {
    this.CheckContext();
    string? primaryName = this.Context!
      .Model
      .FindEntityType(typeof(TEntity))?
      .FindPrimaryKey()?
      .Properties
      .Select(x => x.Name)
      .Single();
    if (primaryName == null)
    {
      throw new MissingPrimaryKeyException(
        $"{typeof(TEntity).Name} does not contain a primary key property"
      );
    }
    return primaryName;
  }

  /// <summary>
  /// Gets the primary key value from a service model instance. The method assumes the service
  /// model instance contains a property with the same name as the primary key property in the
  /// entity.
  /// <para>
  /// With the first call the default implementation determines the primary key property in
  /// aServiceData using the result from
  /// <see cref="GetPrimaryKeyNameFromEntity{TEntity}"/> and caches
  /// it for future calls. 
  /// </para>
  /// <remarks>
  /// Subclasses can override this method to return the key directly for better performance.
  /// </remarks>
  /// </summary>
  /// <param name="aServiceData"></param>
  /// <returns></returns>
  /// <exception cref="MissingPrimaryKeyException"></exception>
  protected virtual TKey GetPrimaryKeyFromServiceModel<TServiceModel, TEntity, TKey>(
    TServiceModel aServiceData
  )
    where TEntity : class
  {
    Type serviceModelType = typeof(TServiceModel);
    if (s_primaryKeys.TryGetValue(serviceModelType, out PropertyInfo? primaryInfo))
    {
      return (TKey)primaryInfo.GetValue(aServiceData)!;
    }
    string primaryName = this.GetPrimaryKeyNameFromEntity<TEntity>();
    primaryInfo = serviceModelType.GetProperty(primaryName);
    if (primaryInfo == null)
    {
      throw new MissingPrimaryKeyException(
        $"{serviceModelType.Name} does not contain a primary key property {primaryName}"
      );
    }
    s_primaryKeys.Add(serviceModelType, primaryInfo);
    return (TKey)primaryInfo.GetValue(aServiceData)!;
  }

  /// <summary>
  /// Checks if the context is not null, throws an exception if it is.
  /// </summary>
  /// <exception cref="Exception"></exception>
  protected void CheckContext()
  {
    if (this.Context == null)
    {
      throw new Exception("Trying to use a disposed instance");
    }
  }
  
  #endregion
}