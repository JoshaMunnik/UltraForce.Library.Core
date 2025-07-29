// <copyright file="UFRepository.cs" company="Ultra Force Development">
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

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UltraForce.Library.Core.Models;
using UltraForce.Library.Core.Services;

namespace UltraForce.Library.Core.Data;

/// <summary>
/// <see cref="UFRepository{TContext}" /> is a base class to implement a repository.
/// <para>
/// The class implements a locking mechanism to minimize the
/// <see cref="DbContext.SaveChanges()"/> calls.
/// </para>
/// </summary>
/// <remarks>
/// Interface definition for the repository should inherit from
/// <see cref="IUFRepository"/>.
/// <para>
/// See <see cref="IUFDataService"/> and <see cref="IUFDataServiceModel{TEntity}"/> for a
/// </para>
/// </remarks>
/// <typeparam name="TContext">The database context type</typeparam>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class UFRepository<TContext> : IUFRepository where TContext : DbContext
{
  #region Private methods

  /// <summary>
  /// Number of calls to lock
  /// </summary>
  private int m_lockCount;

  /// <summary>
  /// When true Entity Framework tracking is disabled
  /// </summary>
  private readonly bool m_disableTracking;

  #endregion

  #region Constructors

  /// <summary>
  /// Constructs an instance of <see cref="UFRepository{T}" />.
  /// </summary>
  /// <param name="context">
  /// Database context to use
  /// </param>
  /// <param name="disableTracking">
  /// When true tell <see cref="DbContext.ChangeTracker"/> to stop tracking
  /// and reset the state of any tracked entry to
  /// <see cref="EntityState.Detached"/> with the
  /// <see cref="SaveChangesAsync"/> call.
  /// </param>
  protected UFRepository(
    TContext context,
    bool disableTracking
  )
  {
    this.Context = context;
    this.Changed = false;
    this.m_lockCount = 0;
    this.m_disableTracking = disableTracking;
    if (!disableTracking)
    {
      return;
    }
    this.Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    this.Context.ChangeTracker.AutoDetectChangesEnabled = false;
  }

  #endregion

  #region IUFRepository

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

  #region Protected properties

  /// <summary>
  /// The context
  /// </summary>
  protected TContext Context { get; }

  /// <summary>
  /// This property is true when the repository is locked and there is at
  /// least one change pending.
  /// </summary>
  protected bool Changed { get; private set; }

  #endregion

  #region Protected methods

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
      try
      {
        await this.Context.SaveChangesAsync();
      }
      // ReSharper disable once RedundantCatchClause
      catch
      {
        throw;
      }
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
  /// <see cref="EntityState.Modified"/> or <see cref="EntityState.Deleted"/>
  /// to <see cref="EntityState.Detached"/>.
  /// </summary>
  /// <remarks>
  /// Based on code from:
  /// https://stackoverflow.com/questions/27423059/how-do-i-clear-tracked-entities-in-entity-framework
  /// </remarks>
  protected void DetachTrackedEntries()
  {
    List<EntityEntry> changedEntriesCopy = this.Context.ChangeTracker.Entries()
      .Where(entry =>
        entry.State == EntityState.Added ||
        entry.State == EntityState.Modified ||
        entry.State == EntityState.Deleted ||
        entry.State == EntityState.Unchanged
      )
      .ToList();
    foreach (EntityEntry entity in changedEntriesCopy)
    {
      this.Context.Entry(entity.Entity).State = EntityState.Detached;
    }
  }

  #endregion
}
