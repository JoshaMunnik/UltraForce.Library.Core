// <copyright file="UFDataServiceModel.cs" company="Ultra Force Development">
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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UltraForce.Library.Core.Annotations;
using UltraForce.Library.Core.Types.Enums;
using UltraForce.Library.NetStandard.Tools;

namespace UltraForce.Library.Core.Models;

/// <summary>
/// A base class for models in a data service that encapsulates an entity model from a database.
/// <para>
/// The default implementation of the <see cref="CopyFromEntityAsync"/> and
/// <see cref="CopyToEntityAsync"/> uses reflection and copies all properties of the service model
/// class that have been annotated with <see cref="UFEntityAttribute"/>. The code caches property
/// info mapping (so for every model type the properties are only scanned once), but for optimal
/// speed subclasses can override the <see cref="CopyToEntityAsync" /> and
/// <see cref="CopyFromEntityAsync" /> methods to provide a faster implementation.
/// </para>
/// </summary>
/// <typeparam name="TServiceModel">Should be set to its own class</typeparam>
/// <typeparam name="TEntity">an entity model the data service model encapsulates</typeparam>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class UFDataServiceModel<TServiceModel, TEntity> : IUFDataServiceModel<TEntity>
  where TEntity : class
  where TServiceModel : class, IUFDataServiceModel<TEntity>, new()
{
  #region private types

  /// <summary>
  /// Contains lists of properties to copy.
  /// </summary>
  private struct PropertyCopyMap
  {
    /// <summary>
    /// Copy properties from the entity to the service. The key is the source, the value the
    /// target. 
    /// </summary>
    public Dictionary<PropertyInfo, PropertyInfo> EntityToServiceMap { get; set; }

    /// <summary>
    /// Copy properties from the service to entity. The key is the source, the value the
    /// target. 
    /// </summary>
    public Dictionary<PropertyInfo, PropertyInfo> ServiceToEntityMap { get; set; }
  }

  #endregion

  #region private variables

  /// <summary>
  /// Contains a mapping from TServiceModel properties to TEntityModel properties for every subclass of
  /// <see cref="UFDataServiceModel{TServiceModel,TEntity}"/>
  /// </summary>
  // ReSharper disable once StaticMemberInGenericType
  private static readonly Dictionary<Type, PropertyCopyMap> s_propertyMap =
    new();

  #endregion

  #region IUFDataServiceModel

  /// <inheritdoc />
  public virtual Task CopyToEntityAsync(TEntity anEntity, UFEntityAction anAction)
  {
    CopyProperties(this, anEntity, false);
    return Task.CompletedTask;
  }

  /// <inheritdoc />
  public virtual Task CopyFromEntityAsync(TEntity anEntity)
  {
    CopyProperties(anEntity, this, true);
    return Task.CompletedTask;
  }

  #endregion

  #region private methods

  /// <summary>
  /// Copies all properties that have a <see cref="UFEntityAttribute"/>. The method caches the
  /// property information, assuming property types do not change while the program runs.
  /// </summary>
  /// <param name="aSource">Source to copy from</param>
  /// <param name="aTarget">Target to copy to</param>
  /// <param name="anEntityIsSource">True if aSource is the entity object</param>
  private static void CopyProperties(object aSource, object aTarget, bool anEntityIsSource)
  {
    Type serviceModelType = anEntityIsSource ? aTarget.GetType() : aSource.GetType();
    PropertyCopyMap map;
    lock (s_propertyMap)
    {
      if (!s_propertyMap.TryGetValue(serviceModelType, out map))
      {
        map = new PropertyCopyMap()
        {
          ServiceToEntityMap = new Dictionary<PropertyInfo, PropertyInfo>(),
          EntityToServiceMap = new Dictionary<PropertyInfo, PropertyInfo>()
        };
        Type entityModelType = anEntityIsSource ? aSource.GetType() : aTarget.GetType();
        foreach (PropertyInfo serviceProperty in serviceModelType.GetProperties())
        {
          UFEntityAttribute? attribute = serviceProperty.GetCustomAttribute<UFEntityAttribute>();
          if (attribute == null)
          {
            continue;
          }
          string name = string.IsNullOrEmpty(attribute.Name)
            ? serviceProperty.Name
            : attribute.Name;
          PropertyInfo? entityProperty = entityModelType.GetProperty(name);
          if (entityProperty == null)
          {
            continue;
          }
          // always copy all entity properties to service properties
          map.EntityToServiceMap.Add(entityProperty, serviceProperty);
          // but do not copy readonly properties back to the entity
          if (
            !attribute.ReadOnly
            && entityProperty.CanWrite
            && (entityProperty.GetSetMethod(false) != null)
          )
          {
            map.ServiceToEntityMap.Add(serviceProperty, entityProperty);
          }
        }
        s_propertyMap.Add(serviceModelType, map);
      }
    }
    Dictionary<PropertyInfo, PropertyInfo> properties = anEntityIsSource
      ? map.EntityToServiceMap
      : map.ServiceToEntityMap;
    foreach ((PropertyInfo sourceProperty, PropertyInfo targetProperty) in properties)
    {
      UFReflectionTools.CopyProperty(
        sourceProperty, targetProperty, aSource, aTarget
      );
    }
  }

  #endregion
}