// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Microsoft.OData.UriParser;
using Microsoft.OData.WebApi.Builder;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Interfaces;
using Microsoft.OData.WebApi.Properties;
using Microsoft.OData.WebApi.Query;
using Microsoft.OData.WebApi.Query.Expressions;
using Microsoft.Spatial;
using ODataPath = Microsoft.OData.WebApi.Routing.ODataPath;

namespace Microsoft.OData.WebApi.Formatter
{
    /// <summary>
    /// Helpers for EdmLib.
    /// </summary>
    public static class EdmLibHelpers
    {
        private static readonly EdmCoreModel _coreModel = EdmCoreModel.Instance;

        private static readonly IWebApiAssembliesResolver _defaultAssemblyResolver = new WebApiDefaultAssembliesResolver();

        private static ConcurrentDictionary<IEdmEntitySet, IEnumerable<IEdmStructuralProperty>> _concurrencyProperties;

        private static readonly Dictionary<Type, IEdmPrimitiveType> _builtInTypesMapping =
            new[]
            {
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(string), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(bool), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(bool?), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte?), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(decimal), GetPrimitiveType(EdmPrimitiveTypeKind.Decimal)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(decimal?), GetPrimitiveType(EdmPrimitiveTypeKind.Decimal)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(double), GetPrimitiveType(EdmPrimitiveTypeKind.Double)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(double?), GetPrimitiveType(EdmPrimitiveTypeKind.Double)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Guid), GetPrimitiveType(EdmPrimitiveTypeKind.Guid)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Guid?), GetPrimitiveType(EdmPrimitiveTypeKind.Guid)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(short), GetPrimitiveType(EdmPrimitiveTypeKind.Int16)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(short?), GetPrimitiveType(EdmPrimitiveTypeKind.Int16)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(int), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(int?), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(long), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(long?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(sbyte), GetPrimitiveType(EdmPrimitiveTypeKind.SByte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(sbyte?), GetPrimitiveType(EdmPrimitiveTypeKind.SByte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(float), GetPrimitiveType(EdmPrimitiveTypeKind.Single)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(float?), GetPrimitiveType(EdmPrimitiveTypeKind.Single)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte[]), GetPrimitiveType(EdmPrimitiveTypeKind.Binary)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Stream), GetPrimitiveType(EdmPrimitiveTypeKind.Stream)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Geography), GetPrimitiveType(EdmPrimitiveTypeKind.Geography)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyCollection), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyMultiLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyMultiPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyMultiPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Geometry), GetPrimitiveType(EdmPrimitiveTypeKind.Geometry)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryCollection), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryMultiLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryMultiPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryMultiPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTimeOffset), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTimeOffset?), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeSpan), GetPrimitiveType(EdmPrimitiveTypeKind.Duration)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeSpan?), GetPrimitiveType(EdmPrimitiveTypeKind.Duration)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Date), GetPrimitiveType(EdmPrimitiveTypeKind.Date)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Date?), GetPrimitiveType(EdmPrimitiveTypeKind.Date)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeOfDay), GetPrimitiveType(EdmPrimitiveTypeKind.TimeOfDay)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeOfDay?), GetPrimitiveType(EdmPrimitiveTypeKind.TimeOfDay)),

                // Keep the Binary and XElement in the end, since there are not the default mappings for Edm.Binary and Edm.String.
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(XElement), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ushort), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ushort?), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(uint), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(uint?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ulong), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ulong?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(char[]), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(char), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(char?), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTime), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTime?), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
            }
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        /// <summary>
        /// Get the EdmType for a given Type.
        /// </summary>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The Edm type matching Type.</returns>
        public static IEdmType GetEdmType(this IEdmModel edmModel, Type clrType)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull("edmModel");
            }

            if (clrType == null)
            {
                throw Error.ArgumentNull("clrType");
            }

            return GetEdmType(edmModel, clrType, testCollections: true);
        }

        private static IEdmType GetEdmType(IEdmModel edmModel, Type clrType, bool testCollections)
        {
            Contract.Assert(edmModel != null);
            Contract.Assert(clrType != null);

            IEdmPrimitiveType primitiveType = GetEdmPrimitiveTypeOrNull(clrType);
            if (primitiveType != null)
            {
                return primitiveType;
            }
            else
            {
                if (testCollections)
                {
                    Type enumerableOfT = ExtractGenericInterface(clrType, typeof(IEnumerable<>));
                    if (enumerableOfT != null)
                    {
                        Type elementClrType = enumerableOfT.GetGenericArguments()[0];

                        // IEnumerable<SelectExpandWrapper<T>> is a collection of T.
                        Type entityType;
                        if (IsSelectExpandWrapper(elementClrType, out entityType))
                        {
                            elementClrType = entityType;
                        }

                        IEdmType elementType = GetEdmType(edmModel, elementClrType, testCollections: false);
                        if (elementType != null)
                        {
                            return new EdmCollectionType(elementType.ToEdmTypeReference(IsNullable(elementClrType)));
                        }
                    }
                }

                Type underlyingType = TypeHelper.GetUnderlyingTypeOrSelf(clrType);
                if (underlyingType.GetTypeInfo().IsEnum)
                {
                    clrType = underlyingType;
                }

                // search for the ClrTypeAnnotation and return it if present
                IEdmType returnType =
                    edmModel
                    .SchemaElements
                    .OfType<IEdmType>()
                    .Select(edmType => new { EdmType = edmType, Annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmType) })
                    .Where(tuple => tuple.Annotation != null && tuple.Annotation.ClrType == clrType)
                    .Select(tuple => tuple.EdmType)
                    .SingleOrDefault();

                // default to the EdmType with the same name as the ClrType name
                returnType = returnType ?? edmModel.FindType(clrType.EdmFullName());

                if (clrType.GetTypeInfo().BaseType != null)
                {
                    // go up the inheritance tree to see if we have a mapping defined for the base type.
                    returnType = returnType ?? GetEdmType(edmModel, clrType.GetTypeInfo().BaseType, testCollections);
                }
                return returnType;
            }
        }

        /// <summary>
        /// Get the EdmTypeReference for a given type.
        /// </summary>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The EdmTypeReference matching Type.</returns>
        public static IEdmTypeReference GetEdmTypeReference(this IEdmModel edmModel, Type clrType)
        {
            IEdmType edmType = edmModel.GetEdmType(clrType);
            if (edmType != null)
            {
                bool isNullable = IsNullable(clrType);
                return ToEdmTypeReference(edmType, isNullable);
            }

            return null;
        }

        /// <summary>
        /// Convert an EdmType to an EdmTypeReference.
        /// </summary>
        /// <param name="edmType">The EdmType to convert.</param>
        /// <param name="isNullable">true for a null-able type reference; false otherwise.</param>
        /// <returns>An EdmTypeReference.</returns>
        public static IEdmTypeReference ToEdmTypeReference(this IEdmType edmType, bool isNullable)
        {
            Contract.Assert(edmType != null);

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Collection:
                    return new EdmCollectionTypeReference(edmType as IEdmCollectionType);
                case EdmTypeKind.Complex:
                    return new EdmComplexTypeReference(edmType as IEdmComplexType, isNullable);
                case EdmTypeKind.Entity:
                    return new EdmEntityTypeReference(edmType as IEdmEntityType, isNullable);
                case EdmTypeKind.EntityReference:
                    return new EdmEntityReferenceTypeReference(edmType as IEdmEntityReferenceType, isNullable);
                case EdmTypeKind.Enum:
                    return new EdmEnumTypeReference(edmType as IEdmEnumType, isNullable);
                case EdmTypeKind.Primitive:
                    return _coreModel.GetPrimitive((edmType as IEdmPrimitiveType).PrimitiveKind, isNullable);
                default:
                    throw Error.NotSupported(SRResources.EdmTypeNotSupported, edmType.ToTraceString());
            }
        }

        /// <summary>
        /// Get a EdmCollectionType for a given EdmEntityType.
        /// </summary>
        /// <param name="entityType">The EdmEntityType.</param>
        /// <returns>A EdmCollectionType for a given EdmEntityType.</returns>
        public static IEdmCollectionType GetCollection(this IEdmEntityType entityType)
        {
            return new EdmCollectionType(new EdmEntityTypeReference(entityType, isNullable: false));
        }

        /// <summary>
        /// Get the Type for a given EdmTypeReference using the default assembly resolver.
        /// </summary>
        /// <param name="edmTypeReference">The EdmTypeReference.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <returns>The Type for a given EdmTypeReference.</returns>
        public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel)
        {
            return GetClrType(edmTypeReference, edmModel, _defaultAssemblyResolver);
        }

        /// <summary>
        /// Get the Type for a given EdmTypeReference using a specific assembly resolver.
        /// </summary>
        /// <param name="edmTypeReference">The EdmTypeReference.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="assembliesResolver">The assembly resolver to use.</param>
        /// <returns>The Type for a given EdmTypeReference.</returns>
        public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel, IWebApiAssembliesResolver assembliesResolver)
        {
            if (edmTypeReference == null)
            {
                throw Error.ArgumentNull("edmTypeReference");
            }

            Type primitiveClrType = _builtInTypesMapping
                .Where(kvp => edmTypeReference.Definition.IsEquivalentTo(kvp.Value) && (!edmTypeReference.IsNullable || IsNullable(kvp.Key)))
                .Select(kvp => kvp.Key)
                .FirstOrDefault();

            if (primitiveClrType != null)
            {
                return primitiveClrType;
            }
            else
            {
                Type clrType = GetClrType(edmTypeReference.Definition, edmModel, assembliesResolver);
                if (clrType != null && clrType.GetTypeInfo().IsEnum && edmTypeReference.IsNullable)
                {
                    return clrType.ToNullable();
                }

                return clrType;
            }
        }

        /// <summary>
        /// Get the Type for a given EdmType using the default assembly resolver.
        /// </summary>
        /// <param name="edmType">The EdmType.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <returns>The Type for a given EdmType.</returns>
        public static Type GetClrType(IEdmType edmType, IEdmModel edmModel)
        {
            return GetClrType(edmType, edmModel, _defaultAssemblyResolver);
        }

        /// <summary>
        /// Get the Type for a given EdmType using a specific assembly resolver.
        /// </summary>
        /// <param name="edmType">The EdmType.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="assembliesResolver">The assembly resolver to use.</param>
        /// <returns>The Type for a given EdmType.</returns>
        public static Type GetClrType(IEdmType edmType, IEdmModel edmModel, IWebApiAssembliesResolver assembliesResolver)
        {
            IEdmSchemaType edmSchemaType = edmType as IEdmSchemaType;

            Contract.Assert(edmSchemaType != null);

            ClrTypeAnnotation annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmSchemaType);
            if (annotation != null)
            {
                return annotation.ClrType;
            }

            string typeName = edmSchemaType.FullName();
            IEnumerable<Type> matchingTypes = GetMatchingTypes(typeName, assembliesResolver);

            if (matchingTypes.Count() > 1)
            {
                throw Error.Argument("edmTypeReference", SRResources.MultipleMatchingClrTypesForEdmType,
                    typeName, String.Join(",", matchingTypes.Select(type => type.AssemblyQualifiedName)));
            }

            edmModel.SetAnnotationValue<ClrTypeAnnotation>(edmSchemaType, new ClrTypeAnnotation(matchingTypes.SingleOrDefault()));

            return matchingTypes.SingleOrDefault();
        }

        /// <summary>
        /// Determine if a property is filter-able.
        /// </summary>
        /// <param name="edmProperty">The property to test.</param>
        /// <param name="pathEdmProperty">The property path.</param>
        /// <param name="pathEdmStructuredType">The EdmStructureType to which the properties belongs.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="enableFilter">True to check the filter configuration; false otherwise.</param>
        /// <returns>True if the property is filter-able; false otherwise.</returns>
        public static bool IsNotFilterable(IEdmProperty edmProperty, IEdmProperty pathEdmProperty,
            IEdmStructuredType pathEdmStructuredType,
            IEdmModel edmModel, bool enableFilter)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            if (annotation != null && annotation.Restrictions.NotFilterable)
            {
                return true;
            }
            else
            {
                if (pathEdmStructuredType == null)
                {
                    pathEdmStructuredType = edmProperty.DeclaringType;
                }

                ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(pathEdmProperty,
                    pathEdmStructuredType, edmModel);
                if (!enableFilter)
                {
                    return !querySettings.Filterable(edmProperty.Name);
                }

                bool enable;
                if (querySettings.FilterConfigurations.TryGetValue(edmProperty.Name, out enable))
                {
                    return !enable;
                }
                else
                {
                    return querySettings.DefaultEnableFilter == false;
                }
            }
        }

        /// <summary>
        /// Determine if a property is sortable.
        /// </summary>
        /// <param name="edmProperty">The property to test.</param>
        /// <param name="pathEdmProperty">The property path.</param>
        /// <param name="pathEdmStructuredType">The EdmStructureType to which the properties belongs.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="enableOrderBy">True to check the order by configuration; false otherwise.</param>
        /// <returns>True if the property is sortable; false otherwise.</returns>
        public static bool IsNotSortable(IEdmProperty edmProperty, IEdmProperty pathEdmProperty,
            IEdmStructuredType pathEdmStructuredType, IEdmModel edmModel, bool enableOrderBy)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            if (annotation != null && annotation.Restrictions.NotSortable)
            {
                return true;
            }
            else
            {
                if (pathEdmStructuredType == null)
                {
                    pathEdmStructuredType = edmProperty.DeclaringType;
                }

                ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(pathEdmProperty,
                    pathEdmStructuredType, edmModel);
                if (!enableOrderBy)
                {
                    return !querySettings.Sortable(edmProperty.Name);
                }

                bool enable;
                if (querySettings.OrderByConfigurations.TryGetValue(edmProperty.Name, out enable))
                {
                    return !enable;
                }
                else
                {
                    return querySettings.DefaultEnableOrderBy == false;
                }
            }
        }

        /// <summary>
        /// Determine if a property is selectable.
        /// </summary>
        /// <param name="edmProperty">The property to test.</param>
        /// <param name="pathEdmProperty">The property path.</param>
        /// <param name="pathEdmStructuredType">The EdmStructureType to which the properties belongs.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="enableSelect">True to check the select configuration; false otherwise.</param>
        /// <returns>True if the property is selectable; false otherwise.</returns>
        public static bool IsNotSelectable(IEdmProperty edmProperty, IEdmProperty pathEdmProperty,
            IEdmStructuredType pathEdmStructuredType, IEdmModel edmModel, bool enableSelect)
        {
            if (pathEdmStructuredType == null)
            {
                pathEdmStructuredType = edmProperty.DeclaringType;
            }

            ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(pathEdmProperty,
                pathEdmStructuredType, edmModel);
            if (!enableSelect)
            {
                return !querySettings.Selectable(edmProperty.Name);
            }

            SelectExpandType enable;
            if (querySettings.SelectConfigurations.TryGetValue(edmProperty.Name, out enable))
            {
                return enable == SelectExpandType.Disabled;
            }
            else
            {
                return querySettings.DefaultSelectType == SelectExpandType.Disabled;
            }
        }

        /// <summary>
        /// Determine if a property is navigate-able.
        /// </summary>
        /// <param name="edmProperty">The property to test.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <returns>True if the property is navigate-able; false otherwise.</returns>
        public static bool IsNotNavigable(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            return annotation == null ? false : annotation.Restrictions.NotNavigable;
        }

        /// <summary>
        /// Determine if a property is expandable.
        /// </summary>
        /// <param name="edmProperty">The property to test.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <returns>True if the property is expandable; false otherwise.</returns>
        public static bool IsNotExpandable(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            return annotation == null ? false : annotation.Restrictions.NotExpandable;
        }

        /// <summary>
        /// Determine if a property is auto selectable.
        /// </summary>
        /// <param name="property">The property to test.</param>
        /// <param name="pathProperty">The property path.</param>
        /// <param name="pathStructuredType">The EdmStructureType to which the properties belongs.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="querySettings">The query settings to use.</param>
        /// <returns>True if the property is auto selectable; false otherwise.</returns>
        public static bool IsAutoSelect(IEdmProperty property, IEdmProperty pathProperty,
            IEdmStructuredType pathStructuredType, IEdmModel edmModel, ModelBoundQuerySettings querySettings = null)
        {
            if (querySettings == null)
            {
                querySettings = GetModelBoundQuerySettings(pathProperty, pathStructuredType, edmModel);
            }

            if (querySettings != null && querySettings.IsAutomaticSelect(property.Name))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determine if a property is auto expandable.
        /// </summary>
        /// <param name="navigationProperty">The property to test.</param>
        /// <param name="pathProperty">The property path.</param>
        /// <param name="pathStructuredType">The EdmStructureType to which the properties belongs.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="isSelectPresent">True if select is present; false otherwise.</param>
        /// <param name="querySettings">The query settings to use.</param>
        /// <returns>True if the property is auto expandable; false otherwise.</returns>
        public static bool IsAutoExpand(IEdmProperty navigationProperty,
            IEdmProperty pathProperty, IEdmStructuredType pathStructuredType, IEdmModel edmModel,
            bool isSelectPresent = false, ModelBoundQuerySettings querySettings = null)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(navigationProperty, edmModel);
            if (annotation != null && annotation.Restrictions.AutoExpand)
            {
                return !annotation.Restrictions.DisableAutoExpandWhenSelectIsPresent || !isSelectPresent;
            }

            if (querySettings == null)
            {
                querySettings = GetModelBoundQuerySettings(pathProperty, pathStructuredType, edmModel);
            }

            if (querySettings != null && querySettings.IsAutomaticExpand(navigationProperty.Name))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the auto expand navigation properties for a given property path.
        /// </summary>
        /// <param name="pathProperty">The property path.</param>
        /// <param name="pathStructuredType">The EdmStructureType to which the properties belongs.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="isSelectPresent">True if select is present; false otherwise.</param>
        /// <param name="querySettings">The query settings to use.</param>
        /// <returns>The auto expand navigation properties for a given property path.</returns>
        public static IEnumerable<IEdmNavigationProperty> GetAutoExpandNavigationProperties(
            IEdmProperty pathProperty, IEdmStructuredType pathStructuredType, IEdmModel edmModel,
            bool isSelectPresent = false, ModelBoundQuerySettings querySettings = null)
        {
            List<IEdmNavigationProperty> autoExpandNavigationProperties = new List<IEdmNavigationProperty>();
            IEdmEntityType baseEntityType = pathStructuredType as IEdmEntityType;
            if (baseEntityType != null)
            {
                List<IEdmEntityType> entityTypes = new List<IEdmEntityType>();
                entityTypes.Add(baseEntityType);
                entityTypes.AddRange(GetAllDerivedEntityTypes(baseEntityType, edmModel));
                foreach (var entityType in entityTypes)
                {
                    IEnumerable<IEdmNavigationProperty> navigationProperties = entityType == baseEntityType
                        ? entityType.NavigationProperties()
                        : entityType.DeclaredNavigationProperties();

                    if (navigationProperties != null)
                    {
                        autoExpandNavigationProperties.AddRange(
                            navigationProperties.Where(
                                navigationProperty =>
                                    IsAutoExpand(navigationProperty, pathProperty, entityType, edmModel,
                                        isSelectPresent, querySettings)));
                    }
                }
            }

            return autoExpandNavigationProperties;
        }

        /// <summary>
        /// Get the auto selectable properties for a given path.
        /// </summary>
        /// <param name="pathProperty">The property path.</param>
        /// <param name="pathStructuredType">The EdmStructureType to which the properties belongs.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="querySettings">The query settings to use.</param>
        /// <returns>The auto selectable properties for a given path.</returns>
        public static IEnumerable<IEdmStructuralProperty> GetAutoSelectProperties(
            IEdmProperty pathProperty,
            IEdmStructuredType pathStructuredType,
            IEdmModel edmModel,
            ModelBoundQuerySettings querySettings = null)
        {
            List<IEdmStructuralProperty> autoSelectProperties = new List<IEdmStructuralProperty>();
            IEdmEntityType baseEntityType = pathStructuredType as IEdmEntityType;
            if (baseEntityType != null)
            {
                List<IEdmEntityType> entityTypes = new List<IEdmEntityType>();
                entityTypes.Add(baseEntityType);
                entityTypes.AddRange(GetAllDerivedEntityTypes(baseEntityType, edmModel));
                foreach (var entityType in entityTypes)
                {
                    IEnumerable<IEdmStructuralProperty> properties = entityType == baseEntityType
                        ? entityType.StructuralProperties()
                        : entityType.DeclaredStructuralProperties();
                    if (properties != null)
                    {
                        autoSelectProperties.AddRange(
                            properties.Where(
                                property =>
                                    IsAutoSelect(property, pathProperty, entityType, edmModel,
                                        querySettings)));
                    }
                }
            }
            else if (pathStructuredType != null)
            {
                IEnumerable<IEdmStructuralProperty> properties = pathStructuredType.StructuralProperties();
                if (properties != null)
                {
                    autoSelectProperties.AddRange(
                        properties.Where(
                            property =>
                                IsAutoSelect(property, pathProperty, pathStructuredType, edmModel,
                                    querySettings)));
                }
            }

            return autoSelectProperties;
        }

        /// <summary>
        /// Determine if the top limit has been exceeded.
        /// </summary>
        /// <param name="property">The property to test.</param>
        /// /// <param name="structuredType">The EdmStructureType to which the properties belongs.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="top">The top value to use.</param>
        /// <param name="defaultQuerySettings">The query settings to use.</param>
        /// <param name="maxTop">The maximum top value.</param>
        /// <returns>True if the property is expandable; false otherwise.</returns>
        public static bool IsTopLimitExceeded(IEdmProperty property, IEdmStructuredType structuredType,
            IEdmModel edmModel, int top, DefaultQuerySettings defaultQuerySettings, out int maxTop)
        {
            maxTop = 0;
            ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(property, structuredType, edmModel,
                defaultQuerySettings);
            if (querySettings != null && top > querySettings.MaxTop)
            {
                maxTop = querySettings.MaxTop.Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine if a property is countable.
        /// </summary>
        /// <param name="property">The property to test.</param>
        /// <param name="structuredType">The EdmStructureType to which the properties belongs.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="enableCount">True if count is enabled; false otherwise.</param>
        /// <returns>True if the property is countable; false otherwise.</returns>
        public static bool IsNotCountable(IEdmProperty property, IEdmStructuredType structuredType, IEdmModel edmModel,
            bool enableCount)
        {
            if (property != null)
            {
                QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(property, edmModel);
                if (annotation != null && annotation.Restrictions.NotCountable)
                {
                    return true;
                }
            }

            ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(property, structuredType, edmModel);
            if (querySettings != null &&
                ((!querySettings.Countable.HasValue && !enableCount) ||
                 querySettings.Countable == false))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determine if a property is expandable.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="property">The property to test.</param>
        /// <param name="structuredType">The EdmStructureType to which the properties belongs.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="expandConfiguration">The matching expand configuration.</param>
        /// <returns>True if the property is expandable; false otherwise.</returns>
        public static bool IsExpandable(string propertyName, IEdmProperty property, IEdmStructuredType structuredType,
            IEdmModel edmModel,
            out ExpandConfiguration expandConfiguration)
        {
            expandConfiguration = null;
            ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(property, structuredType, edmModel);
            if (querySettings != null)
            {
                bool result = querySettings.Expandable(propertyName);
                if (!querySettings.ExpandConfigurations.TryGetValue(propertyName, out expandConfiguration) && result)
                {
                    expandConfiguration = new ExpandConfiguration
                    {
                        ExpandType = querySettings.DefaultExpandType.Value,
                        MaxDepth = querySettings.DefaultMaxDepth
                    };
                }

                return result;
            }

            return false;
        }

        /// <summary>
        /// Get the model bound settings.
        /// </summary>
        /// <param name="property">The property to test.</param>
        /// <param name="structuredType">The EdmStructureType to which the properties belongs.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <param name="defaultQuerySettings">The query settings to use.</param>
        /// <returns>The model bound settings.</returns>
        public static ModelBoundQuerySettings GetModelBoundQuerySettings(IEdmProperty property,
            IEdmStructuredType structuredType, IEdmModel edmModel, DefaultQuerySettings defaultQuerySettings = null)
        {
            Contract.Assert(edmModel != null);

            ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(structuredType, edmModel,
                defaultQuerySettings);
            if (property == null)
            {
                return querySettings;
            }
            else
            {
                ModelBoundQuerySettings propertyQuerySettings = GetModelBoundQuerySettings(property, edmModel,
                    defaultQuerySettings);
                return GetMergedPropertyQuerySettings(propertyQuerySettings,
                    querySettings);
            }
        }

        /// <summary>
        /// Get the derived entity types for a given entity type.
        /// </summary>
        /// <param name="entityType">The base entity type.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <returns>The derived entity types for a given entity type.</returns>
        public static IEnumerable<IEdmEntityType> GetAllDerivedEntityTypes(
            IEdmEntityType entityType, IEdmModel edmModel)
        {
            List<IEdmEntityType> derivedEntityTypes = new List<IEdmEntityType>();
            if (entityType != null)
            {
                List<IEdmStructuredType> typeList = new List<IEdmStructuredType>();
                typeList.Add(entityType);
                while (typeList.Count > 0)
                {
                    var head = typeList[0];
                    derivedEntityTypes.Add(head as IEdmEntityType);
                    var derivedTypes = edmModel.FindDirectlyDerivedTypes(head);
                    if (derivedTypes != null)
                    {
                        typeList.AddRange(derivedTypes);
                    }

                    typeList.RemoveAt(0);
                }
            }

            derivedEntityTypes.RemoveAt(0);
            return derivedEntityTypes;
        }

        /// <summary>
        /// Get the EdmType for a TypeReference.
        /// </summary>
        /// <param name="edmTypeReference">The EdmTypeReference to use.</param>
        /// <returns>The EdmType for a TypeReference.</returns>
        public static IEdmType GetElementType(IEdmTypeReference edmTypeReference)
        {
            if (edmTypeReference.IsCollection())
            {
                return edmTypeReference.AsCollection().ElementType().Definition;
            }

            return edmTypeReference.Definition;
        }

        /// <summary>
        /// Get the property and type from a path.
        /// </summary>
        /// <param name="segments">The path segments.</param>
        /// <param name="property">The property found in the path segments.</param>
        /// <param name="structuredType">The type found in the path segments</param>
        /// <param name="name">The name found in the path segments</param>
        public static void GetPropertyAndStructuredTypeFromPath(IEnumerable<ODataPathSegment> segments,
            out IEdmProperty property, out IEdmStructuredType structuredType, out string name)
        {
            property = null;
            structuredType = null;
            name = String.Empty;
            string typeCast = String.Empty;
            if (segments != null)
            {
                IEnumerable<ODataPathSegment> reverseSegments = segments.Reverse();
                foreach (var segment in reverseSegments)
                {
                    NavigationPropertySegment navigationPathSegment = segment as NavigationPropertySegment;
                    if (navigationPathSegment != null)
                    {
                        property = navigationPathSegment.NavigationProperty;
                        if (structuredType == null)
                        {
                            structuredType = navigationPathSegment.NavigationProperty.ToEntityType();
                        }

                        name = navigationPathSegment.NavigationProperty.Name + typeCast;
                        return;
                    }

                    PropertySegment propertyAccessPathSegment = segment as PropertySegment;
                    if (propertyAccessPathSegment != null)
                    {
                        property = propertyAccessPathSegment.Property;
                        if (structuredType == null)
                        {
                            structuredType = GetElementType(property.Type) as IEdmStructuredType;
                        }
                        name = property.Name + typeCast;
                        return;
                    }

                    EntitySetSegment entitySetSegment = segment as EntitySetSegment;
                    if (entitySetSegment != null)
                    {
                        if (structuredType == null)
                        {
                            structuredType = entitySetSegment.EntitySet.EntityType();
                        }
                        name = entitySetSegment.EntitySet.Name + typeCast;
                        return;
                    }

                    TypeSegment typeSegment = segment as TypeSegment;
                    if (typeSegment != null)
                    {
                        structuredType = GetElementType(typeSegment.EdmType.ToEdmTypeReference(false)) as IEdmStructuredType;
                        typeCast = "/" + structuredType;
                    }
                }
            }
        }

        /// <summary>
        /// Get the CLR property name for a given property.
        /// </summary>
        /// <param name="edmProperty">The property to use.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <returns></returns>
        public static string GetClrPropertyName(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (edmModel == null)
            {
                throw Error.ArgumentNull("edmModel");
            }

            string propertyName = edmProperty.Name;
            ClrPropertyInfoAnnotation annotation = edmModel.GetAnnotationValue<ClrPropertyInfoAnnotation>(edmProperty);
            if (annotation != null)
            {
                PropertyInfo propertyInfo = annotation.ClrPropertyInfo;
                if (propertyInfo != null)
                {
                    propertyName = propertyInfo.Name;
                }
            }

            return propertyName;
        }

        /// <summary>
        /// Get the dynamic property info for a given type.
        /// </summary>
        /// <param name="edmType">The EdmStructureType to use.</param>
        /// <param name="edmModel">The EdmModel to use.</param>
        /// <returns>The dynamic property info for a given type.</returns>
        public static PropertyInfo GetDynamicPropertyDictionary(IEdmStructuredType edmType, IEdmModel edmModel)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            if (edmModel == null)
            {
                throw Error.ArgumentNull("edmModel");
            }

            DynamicPropertyDictionaryAnnotation annotation =
                edmModel.GetAnnotationValue<DynamicPropertyDictionaryAnnotation>(edmType);
            if (annotation != null)
            {
                return annotation.PropertyInfo;
            }

            return null;
        }

        /// <summary>
        /// Get the EdmPrimitveType (or null) for a given Type.
        /// </summary>
        /// <param name="clrType">The Type to use.</param>
        /// <returns>The EdmPrimitveType for a given Type.</returns>
        public static IEdmPrimitiveType GetEdmPrimitiveTypeOrNull(Type clrType)
        {
            IEdmPrimitiveType primitiveType;
            return _builtInTypesMapping.TryGetValue(clrType, out primitiveType) ? primitiveType : null;
        }

        /// <summary>
        /// Get the EdmPrimitiveTypeReference (or null) for a given Type.
        /// </summary>
        /// <param name="clrType">The Type to use.</param>
        /// <returns>The EdmPrimitveType for a given Type.</returns>
        public static IEdmPrimitiveTypeReference GetEdmPrimitiveTypeReferenceOrNull(Type clrType)
        {
            IEdmPrimitiveType primitiveType = GetEdmPrimitiveTypeOrNull(clrType);
            return primitiveType != null ? _coreModel.GetPrimitive(primitiveType.PrimitiveKind, IsNullable(clrType)) : null;
        }

        /// <summary>
        /// figures out if the given clr type is nonstandard edm primitive like uint, ushort, char[] etc.
        /// and returns the corresponding clr type to which we map like uint => long.
        /// </summary>
        /// <param name="type">The Type to use.</param>
        /// <param name="isNonstandardEdmPrimitive">True if type is non-standard EDM primitive; false otherwise.</param>
        /// <returns>The corresponding clr type to which we map like uint => long.</returns>
        public static Type IsNonstandardEdmPrimitive(Type type, out bool isNonstandardEdmPrimitive)
        {
            IEdmPrimitiveTypeReference edmType = GetEdmPrimitiveTypeReferenceOrNull(type);
            if (edmType == null)
            {
                isNonstandardEdmPrimitive = false;
                return type;
            }

            Type reverseLookupClrType = GetClrType(edmType, EdmCoreModel.Instance);
            isNonstandardEdmPrimitive = (type != reverseLookupClrType);

            return reverseLookupClrType;
        }

        /// <summary>
        /// Mangle the invalid EDM literal Type.FullName (System.Collections.Generic.IEnumerable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]])
        /// to a valid EDM literal (the C# type name IEnumerable&lt;int&gt;).
        /// </summary>
        /// <param name="clrType">The Type to use.</param>
        /// <returns>A valid EDM literal.</returns>
        public static string EdmName(this Type clrType)
        {
            // We cannot use just Type.Name here as it doesn't work for generic types.
            return MangleClrTypeName(clrType);
        }

        /// <summary>
        /// Get the EDM full name for a Type.
        /// </summary>
        /// <param name="clrType">The Type to use.</param>
        /// <returns>The EDM full name for a Type.</returns>
        public static string EdmFullName(this Type clrType)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", clrType.Namespace, clrType.EdmName());
        }

        /// <summary>
        /// Get the concurrency properties for a given entity set.
        /// </summary>
        /// <param name="model">The EdmModel to use.</param>
        /// <param name="entitySet">The EdmEntitySet to use.</param>
        /// <returns>The concurrency properties for a given entity set.</returns>
        public static IEnumerable<IEdmStructuralProperty> GetConcurrencyProperties(this IEdmModel model, IEdmEntitySet entitySet)
        {
            Contract.Assert(model != null);
            Contract.Assert(entitySet != null);

            IEnumerable<IEdmStructuralProperty> cachedProperties;
            if (_concurrencyProperties != null && _concurrencyProperties.TryGetValue(entitySet, out cachedProperties))
            {
                return cachedProperties;
            }

            IList<IEdmStructuralProperty> results = new List<IEdmStructuralProperty>();
            IEdmEntityType entityType = entitySet.EntityType();
            var annotations = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(entitySet, CoreVocabularyModel.ConcurrencyTerm);
            IEdmVocabularyAnnotation annotation = annotations.FirstOrDefault();
            if (annotation != null)
            {
                IEdmCollectionExpression properties = annotation.Value as IEdmCollectionExpression;
                if (properties != null)
                {
                    foreach (var property in properties.Elements)
                    {
                        IEdmPathExpression pathExpression = property as IEdmPathExpression;
                        if (pathExpression != null)
                        {
                            // So far, we only consider the single path, because only the direct properties from declaring type are used.
                            // However we have an issue tracking on: https://github.com/OData/WebApi/issues/472
                            string propertyName = pathExpression.PathSegments.First();
                            IEdmProperty edmProperty = entityType.FindProperty(propertyName);
                            IEdmStructuralProperty structuralProperty = edmProperty as IEdmStructuralProperty;
                            if (structuralProperty != null)
                            {
                                results.Add(structuralProperty);
                            }
                        }
                    }
                }
            }

            if (_concurrencyProperties == null)
            {
                _concurrencyProperties = new ConcurrentDictionary<IEdmEntitySet, IEnumerable<IEdmStructuralProperty>>();
            }

            _concurrencyProperties[entitySet] = results;
            return results;
        }

        /// <summary>
        /// Determine if a given Type is a dynamic wrapper.
        /// </summary>
        /// <param name="type">The Type to use.</param>
        /// <returns>True if a given Type is a dynamic wrapper; false otherwise.</returns>
        public static bool IsDynamicTypeWrapper(Type type)
        {
            return (type != null && typeof(DynamicTypeWrapper).IsAssignableFrom(type));
        }

        /// <summary>
        /// Determine if a given Type is null-able.
        /// </summary>
        /// <param name="type">The Type to use.</param>
        /// <returns>True if a given Type is null-able; false otherwise.</returns>
        public static bool IsNullable(Type type)
        {
            return !type.GetTypeInfo().IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Get the expected payload type of an OData path.
        /// </summary>
        /// <param name="type">The Type to use.</param>
        /// <param name="path">The path to use.</param>
        /// <param name="model">The EdmModel to use.</param>
        /// <returns>The expected payload type of an OData path.</returns>
        public static IEdmTypeReference GetExpectedPayloadType(Type type, ODataPath path, IEdmModel model)
        {
            IEdmTypeReference expectedPayloadType = null;

            if (typeof(IEdmObject).IsAssignableFrom(type))
            {
                // typeless mode. figure out the expected payload type from the OData Path.
                IEdmType edmType = path.EdmType;
                if (edmType != null)
                {
                    expectedPayloadType = EdmLibHelpers.ToEdmTypeReference(edmType, isNullable: false);
                    if (expectedPayloadType.TypeKind() == EdmTypeKind.Collection)
                    {
                        IEdmTypeReference elementType = expectedPayloadType.AsCollection().ElementType();
                        if (elementType.IsEntity())
                        {
                            // collection of entities cannot be CREATE/UPDATEd. Instead, the request would contain a single entry.
                            expectedPayloadType = elementType;
                        }
                    }
                }
            }
            else
            {
                TryGetInnerTypeForDelta(ref type);
                expectedPayloadType = model.GetEdmTypeReference(type);
            }

            return expectedPayloadType;
        }

        private static bool TryGetInnerTypeForDelta(ref Type type)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Delta<>))
            {
                type = type.GetGenericArguments()[0];
                return true;
            }

            return false;
        }

        private static ModelBoundQuerySettings GetMergedPropertyQuerySettings(
            ModelBoundQuerySettings propertyQuerySettings, ModelBoundQuerySettings propertyTypeQuerySettings)
        {
            ModelBoundQuerySettings mergedQuerySettings = new ModelBoundQuerySettings(propertyQuerySettings);
            if (propertyTypeQuerySettings != null)
            {
                if (!mergedQuerySettings.PageSize.HasValue)
                {
                    mergedQuerySettings.PageSize =
                        propertyTypeQuerySettings.PageSize;
                }

                if (mergedQuerySettings.MaxTop == 0 && propertyTypeQuerySettings.MaxTop != 0)
                {
                    mergedQuerySettings.MaxTop =
                        propertyTypeQuerySettings.MaxTop;
                }

                if (!mergedQuerySettings.Countable.HasValue)
                {
                    mergedQuerySettings.Countable = propertyTypeQuerySettings.Countable;
                }

                if (mergedQuerySettings.OrderByConfigurations.Count == 0 &&
                    !mergedQuerySettings.DefaultEnableOrderBy.HasValue)
                {
                    mergedQuerySettings.CopyOrderByConfigurations(propertyTypeQuerySettings.OrderByConfigurations);
                    mergedQuerySettings.DefaultEnableOrderBy = propertyTypeQuerySettings.DefaultEnableOrderBy;
                }

                if (mergedQuerySettings.FilterConfigurations.Count == 0 &&
                    !mergedQuerySettings.DefaultEnableFilter.HasValue)
                {
                    mergedQuerySettings.CopyFilterConfigurations(propertyTypeQuerySettings.FilterConfigurations);
                    mergedQuerySettings.DefaultEnableFilter = propertyTypeQuerySettings.DefaultEnableFilter;
                }

                if (mergedQuerySettings.SelectConfigurations.Count == 0 &&
                    !mergedQuerySettings.DefaultSelectType.HasValue)
                {
                    mergedQuerySettings.CopySelectConfigurations(propertyTypeQuerySettings.SelectConfigurations);
                    mergedQuerySettings.DefaultSelectType = propertyTypeQuerySettings.DefaultSelectType;
                }

                if (mergedQuerySettings.ExpandConfigurations.Count == 0 &&
                    !mergedQuerySettings.DefaultExpandType.HasValue)
                {
                    mergedQuerySettings.CopyExpandConfigurations(
                        propertyTypeQuerySettings.ExpandConfigurations);
                    mergedQuerySettings.DefaultExpandType = propertyTypeQuerySettings.DefaultExpandType;
                    mergedQuerySettings.DefaultMaxDepth = propertyTypeQuerySettings.DefaultMaxDepth;
                }
            }
            return mergedQuerySettings;
        }

        private static ModelBoundQuerySettings GetModelBoundQuerySettings<T>(T key, IEdmModel edmModel,
            DefaultQuerySettings defaultQuerySettings = null)
            where T : IEdmElement
        {
            Contract.Assert(edmModel != null);

            if (key == null)
            {
                return null;
            }
            else
            {
                ModelBoundQuerySettings querySettings = edmModel.GetAnnotationValue<ModelBoundQuerySettings>(key);
                if (querySettings == null)
                {
                    querySettings = new ModelBoundQuerySettings();
                    if (defaultQuerySettings != null &&
                        (!defaultQuerySettings.MaxTop.HasValue || defaultQuerySettings.MaxTop > 0))
                    {
                        querySettings.MaxTop = defaultQuerySettings.MaxTop;
                    }
                }
                return querySettings;
            }
        }

        private static QueryableRestrictionsAnnotation GetPropertyRestrictions(IEdmProperty edmProperty,
            IEdmModel edmModel)
        {
            Contract.Assert(edmProperty != null);
            Contract.Assert(edmModel != null);

            return edmModel.GetAnnotationValue<QueryableRestrictionsAnnotation>(edmProperty);
        }

        private static IEdmPrimitiveType GetPrimitiveType(EdmPrimitiveTypeKind primitiveKind)
        {
            return _coreModel.GetPrimitiveType(primitiveKind);
        }

        private static bool IsSelectExpandWrapper(Type type, out Type entityType)
        {
            if (type == null)
            {
                entityType = null;
                return false;
            }

            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(SelectExpandWrapper<>))
            {
                entityType = type.GetGenericArguments()[0];
                return true;
            }

            return IsSelectExpandWrapper(type.GetTypeInfo().BaseType, out entityType);
        }

        private static Type ExtractGenericInterface(Type queryType, Type interfaceType)
        {
            Func<Type, bool> matchesInterface = t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == interfaceType;
            return matchesInterface(queryType) ? queryType : queryType.GetInterfaces().FirstOrDefault(matchesInterface);
        }

        private static IEnumerable<Type> GetMatchingTypes(string edmFullName, IWebApiAssembliesResolver assembliesResolver)
        {
            return TypeHelper.GetLoadedTypes(assembliesResolver).Where(t => t.GetTypeInfo().IsPublic && t.EdmFullName() == edmFullName);
        }

        // TODO (workitem 336): Support nested types and anonymous types.
        private static string MangleClrTypeName(Type type)
        {
            Contract.Assert(type != null);

            if (!type.GetTypeInfo().IsGenericType)
            {
                return type.Name;
            }
            else
            {
                return String.Format(
                    CultureInfo.InvariantCulture,
                    "{0}Of{1}",
                    type.Name.Replace('`', '_'),
                    String.Join("_", type.GetGenericArguments().Select(t => MangleClrTypeName(t))));
            }
        }
    }
}
