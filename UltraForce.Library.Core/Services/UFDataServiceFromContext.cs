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
/// <para>
/// The class defines both a read and write context, to support in-memory copies to read
/// from. All protected methods that perform some operation that changes the database
/// will use the write context, while all methods that only read data will use the read
/// context.
/// </para>
/// </summary>
/// <remarks>
/// Interface definition for the data service should inherit from <see cref="IUFDataService"/>.
/// </remarks>
/// <typeparam name="TContext">The database context type</typeparam>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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

  /// <summary>
  /// Context to perform read database operations with.
  /// </summary>
  private TContext? m_readContext;

  /// <summary>
  /// Context to perform write database operations with.
  /// </summary>
  private TContext? m_writeContext;

  #endregion

  #region constructors

  /// <summary>
  /// Constructs an instance of <see cref="UFDataServiceFromContext{TContext}" />.
  /// </summary>
  /// <param name="context">
  /// Database context to use for both read and write operations.
  /// </param>
  /// <param name="disableTracking">
  /// When true tell <see cref="DbContext.ChangeTracker"/> to stop tracking and reset the state
  /// of any tracked entry to <see cref="EntityState.Detached"/> with
  /// the <see cref="SaveChangesAsync"/> call.
  /// <para>
  /// To disable the tracking everywhere, the method assigns:
  /// <code>{context}.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;</code>
  /// And disables auto-detect changes with:
  /// <code>{context}.ChangeTracker.AutoDetectChangesEnabled = false;</code>
  /// </para>
  /// </param>
  protected UFDataServiceFromContext(
    TContext context,
    bool disableTracking = false
  ) : this(context, context, disableTracking)
  {
  }

  /// <summary>
  /// Constructs an instance of <see cref="UFDataServiceFromContext{TContext}" />.
  /// </summary>
  /// <param name="readContext">
  /// Database context to use for read operations.
  /// </param>
  /// <param name="writeContext">
  /// Database context to use for write operations.
  /// </param>
  /// <param name="disableTracking">
  /// When true tell <see cref="DbContext.ChangeTracker"/> to stop tracking and reset the state
  /// of any tracked entry to <see cref="EntityState.Detached"/> with
  /// the <see cref="SaveChangesAsync"/> call.
  /// <para>
  /// To disable the tracking everywhere, the method assigns:
  /// <code>{context}.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;</code>
  /// And disables auto-detect changes with:
  /// <code>{context}.ChangeTracker.AutoDetectChangesEnabled = false;</code>
  /// </para>
  /// </param>
  protected UFDataServiceFromContext(
    TContext readContext,
    TContext writeContext,
    bool disableTracking = false
  )
  {
    this.m_readContext = readContext;
    this.m_writeContext = writeContext;
    this.Changed = false;
    this.m_lockCount = 0;
    this.m_disableTracking = disableTracking;
    if (!disableTracking)
    {
      return;
    }
    readContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    readContext.ChangeTracker.AutoDetectChangesEnabled = false;
    writeContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    writeContext.ChangeTracker.AutoDetectChangesEnabled = false;
  }

  #endregion

  #region IDisposable & IAsyncDisposable

  /// <inheritdoc />
  public virtual void Dispose()
  {
    this.m_readContext = null;
    this.m_writeContext = null;
    GC.SuppressFinalize(this);
  }

  /// <inheritdoc />
  public virtual ValueTask DisposeAsync()
  {
    this.m_readContext = null;
    this.m_writeContext = null;
    GC.SuppressFinalize(this);
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
  /// The context to read data. If the instance is disposed, this property will throw an
  /// exception.
  /// </summary>
  protected TContext ReadContext
  {
    get
    {
      if (this.m_readContext == null)
      {
        throw new Exception("Trying to use a disposed instance");
      }
      return this.m_readContext;
    }
  }

  /// <summary>
  /// The context to perform operation with that changes the database. If the instance is
  /// disposed, this property will throw an exception.
  /// </summary>
  protected TContext WriteContext
  {
    get
    {
      if (this.m_writeContext == null)
      {
        throw new Exception("Trying to use a disposed instance");
      }
      return this.m_writeContext;
    }
  }

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
  /// <param name="entity"></param>
  protected void DetachEntity(
    object entity
  )
  {
    this.ReadContext.Entry(entity).State = EntityState.Detached;
  }

  /// <summary>
  /// Finds a entity for a certain id and convert it to a service model.
  /// </summary>
  /// <param name="id">Id to find entity for</param>
  /// <typeparam name="TServiceModel">Service model to convert to</typeparam>
  /// <typeparam name="TEntity">Type of entity</typeparam>
  /// <typeparam name="TKey">Type of id</typeparam>
  /// <returns>Service model instance or null if no entity could be found</returns>
  protected async Task<TServiceModel?> FindForIdAsync<TServiceModel, TEntity, TKey>(
    TKey id
  )
    where TEntity : class, new()
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
  {
    return await this
      .ReadContext
      .Set<TEntity>()
      .FindAsync(id)
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
    if (this.m_disableTracking)
    {
      return await this.ReadContext
        .Set<TEntity>()
        .AsNoTracking()
        .AsModelAsync<TServiceModel, TEntity>();
    }
    else
    {
      return await this.ReadContext
        .Set<TEntity>()
        .AsModelAsync<TServiceModel, TEntity>();
    }
  }

  /// <summary>
  /// Creates and adds an entity from a service model. Since aData might be an immutable instance,
  /// after adding the entity a new service model instance is created from the added entity that
  /// might include updated fields (like the id of the entity).
  /// </summary>
  /// <param name="data">Service model instance to add</param>
  /// <typeparam name="TServiceModel">Type of service model</typeparam>
  /// <typeparam name="TEntity">Entity to add</typeparam>
  /// <returns>The service model build from the added entity; this is a new instance</returns>
  protected async Task<TServiceModel> AddAsync<TServiceModel, TEntity>(
    TServiceModel data
  )
    where TEntity : class, new()
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
  {
    TEntity entity = new();
    await data.CopyToEntityAsync(entity, UFEntityAction.Add);
    await this.WriteContext.Set<TEntity>().AddAsync(entity);
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
  /// <param name="data">Data to update entity with</param>
  /// <typeparam name="TServiceModel">Service model type</typeparam>
  /// <typeparam name="TEntity">Entity type</typeparam>
  /// <returns>The service model build from the updated entity; this is a new instance</returns>
  protected async Task<TServiceModel> UpdateAsync<TServiceModel, TEntity>(
    TServiceModel data
  )
    where TEntity : class, new()
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
  {
    TEntity? entity = await this.WriteContext.Set<TEntity>()
      .FindAsync(
        this.GetPrimaryKeyFromServiceModel<TServiceModel, TEntity, object>(data)
      );
    if (entity == null)
    {
      throw new Exception("Trying to update a non-existing entity");
    }
    await data.CopyToEntityAsync(entity, UFEntityAction.Update);
    this.ReadContext.Set<TEntity>().Update(entity);
    await this.SaveChangesAsync();
    TServiceModel result = new();
    await result.CopyFromEntityAsync(entity);
    return result;
  }

  /// <summary>
  /// Removes an entity represented by a service model instance.
  /// </summary>
  /// <param name="data">Data that represents an entity</param>
  /// <typeparam name="TServiceModel">Service model type</typeparam>
  /// <typeparam name="TEntity">Entity type</typeparam>
  protected async Task RemoveAsync<TServiceModel, TEntity>(
    TServiceModel data
  )
    where TEntity : class, new()
    where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
  {
    TEntity? entity = await this.WriteContext.Set<TEntity>()
      .FindAsync(
        this.GetPrimaryKeyFromServiceModel<TServiceModel, TEntity, object>(data)
      );
    if (entity == null)
    {
      throw new Exception("Trying to remove a non-existing entity");
    }
    this.ReadContext.Set<TEntity>().Remove(entity);
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
    if (this.m_lockCount == 0)
    {
      this.Changed = false;
      await this.WriteContext.SaveChangesAsync();
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
    IEnumerable<EntityEntry> changedEntriesCopy = this.WriteContext.ChangeTracker
      .Entries()
      .Where(entry =>
        entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
          or EntityState.Unchanged
      )
      .AsEnumerable();
    foreach (EntityEntry entity in changedEntriesCopy)
    {
      this.WriteContext.Entry(entity.Entity).State = EntityState.Detached;
    }
  }

  /// <summary>
  /// Executes an action within a transaction. If a transaction is already active, the action
  /// just gets executed without using a transaction assuming the commit or rollback is handled
  /// by other code.
  /// </summary>
  /// <param name="action">Action to execute</param>
  /// <exception>
  /// Any exception thrown by anAction will rollback the transaction and gets rethrown
  /// </exception>
  protected async Task TransactionAsync(
    Func<Task> action
  )
  {
    if (this.WriteContext.Database.CurrentTransaction != null)
    {
      await action();
      return;
    }
    await using IDbContextTransaction transaction =
      await this.WriteContext.Database.BeginTransactionAsync();
    try
    {
      await action();
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
  /// <param name="action">Action to execute</param>
  /// <returns>The result of the action</returns>
  /// <exception>
  /// Any exception thrown by anAction will rollback the transaction and gets rethrown
  /// </exception>
  protected async Task<T> TransactionAsync<T>(
    Func<Task<T>> action
  )
  {
    if (this.WriteContext.Database.CurrentTransaction != null)
    {
      return await action();
    }
    await using IDbContextTransaction transaction =
      await this.WriteContext.Database.BeginTransactionAsync();
    try
    {
      T result = await action();
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
  /// <param name="tableName">Name of table</param>
  /// <param name="columnName">Name of column</param>
  /// <param name="firstId">First id to swap value of</param>
  /// <param name="secondId">Second id to swap value of</param>
  /// <param name="modifiedName">When not null, assign <c>modifiedDate</c> to this column</param>
  /// <param name="modifiedDate">When null, use <c>DateTime.UtcNow();</c></param>
  /// <typeparam name="TEntity">Type of the entity record</typeparam>
  /// <typeparam name="TKey">Type of the id values</typeparam>
  [SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.")]
  protected virtual async Task SwapAsync<TEntity, TKey>(
    string tableName,
    string columnName,
    TKey firstId,
    TKey secondId,
    string? modifiedName = null,
    DateTime? modifiedDate = null
  ) where TEntity : class
    where TKey : notnull
  {
    string primaryKey = this.GetPrimaryKeyNameFromEntity<TEntity>();
    if (modifiedName == null)
    {
      await this.WriteContext.Database.ExecuteSqlRawAsync(
        $@"
            UPDATE [{tableName}]
            SET 
              [{columnName}] = CASE [{primaryKey}]
                WHEN {{0}} THEN (SELECT [{columnName}] FROM [{tableName}] WHERE [{primaryKey}] = {{1}})
                WHEN {{1}} THEN (SELECT [{columnName}] FROM [{tableName}] WHERE [{primaryKey}] = {{0}})
              END
            WHERE [{primaryKey}] IN ({{0}}, {{1}})
          ",
        firstId,
        secondId
      );
    }
    else
    {
      await this.WriteContext.Database.ExecuteSqlRawAsync(
        $@"
            UPDATE [{tableName}]
            SET 
              [{modifiedName}] = {{2}}, 
              [{columnName}] = CASE [{primaryKey}]
                WHEN {{0}} THEN (SELECT [{columnName}] FROM [{tableName}] WHERE [{primaryKey}] = {{1}})
                WHEN {{1}} THEN (SELECT [{columnName}] FROM [{tableName}] WHERE [{primaryKey}] = {{0}})
              END
            WHERE [{primaryKey}] IN ({{0}}, {{1}})
          ",
        firstId,
        secondId,
        modifiedDate ?? DateTime.UtcNow
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
  /// <param name="tableName">Name of table</param>
  /// <param name="columnName">Name of column</param>
  /// <param name="first">First entity to swap value of</param>
  /// <param name="second">Second entity to swap value of</param>
  /// <param name="modifiedName">When not null, assign <c>modifiedDate</c> to this column</param>
  /// <param name="modifiedDate">When null, use <c>DateTime.UtcNow();</c></param>
  /// <typeparam name="T"></typeparam>
  protected virtual async Task SwapAsync<T>(
    string tableName,
    string columnName,
    T first,
    T second,
    string? modifiedName = null,
    DateTime? modifiedDate = null
  ) where T : class
  {
    string primaryKey = this.GetPrimaryKeyNameFromEntity<T>();
    PropertyInfo? primaryProperty = (typeof(T)).GetProperty(primaryKey);
    if (primaryProperty == null)
    {
      throw new Exception(@"Can not find primary key property for {primaryKey}");
    }
    await this.SwapAsync<T, object>(
      tableName,
      columnName,
      primaryProperty.GetValue(first, null)!,
      primaryProperty.GetValue(second, null)!,
      modifiedName,
      modifiedDate
    );
    await this.WriteContext.Entry(first).ReloadAsync();
    await this.WriteContext.Entry(second).ReloadAsync();
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
    string? primaryName = this.ReadContext
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
  /// <param name="serviceData"></param>
  /// <returns></returns>
  /// <exception cref="MissingPrimaryKeyException"></exception>
  protected virtual TKey GetPrimaryKeyFromServiceModel<TServiceModel, TEntity, TKey>(
    TServiceModel serviceData
  )
    where TEntity : class
  {
    Type serviceModelType = typeof(TServiceModel);
    if (s_primaryKeys.TryGetValue(serviceModelType, out PropertyInfo? primaryInfo))
    {
      return (TKey)primaryInfo.GetValue(serviceData)!;
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
    return (TKey)primaryInfo.GetValue(serviceData)!;
  }

  #endregion
}
