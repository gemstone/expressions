//******************************************************************************************************
//  IValueExpressionAttribute.cs - Gbtc
//
//  Copyright � 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/04/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using gemstone.expressions.evaluator;

namespace gemstone.expressions.model
{
    /// <summary>
    /// Defines an interface for value expression attributes.
    /// </summary>
    public interface IValueExpressionAttribute
    {
        /// <summary>
        /// Gets C# expression that will evaluate to the desired value.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="GetPropertyUpdateValue"/> method to get a per-property
        /// based value for use with the <see cref="ValueExpressionParser"/>. Use
        /// <see cref="ValueExpressionParser.DeriveExpression"/> to automatically
        /// replace any <c>this</c> keywords with <c>Instance</c> so as to properly
        /// reference the modeled <see cref="ValueExpressionScopeBase{T}.Instance"/>.
        /// </remarks>
        string Expression { get; }

        /// <summary>
        /// Gets or sets value that determines if value should be cached after first evaluation.
        /// Defaults to <c>false</c>.
        /// </summary>
        bool Cached { get; set; }

        /// <summary>
        /// Gets or sets the numeric evaluation order for this expression. Defaults to zero.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is useful for providing an order of operations to evaluations of
        /// <see cref="ValueExpressionAttributeBase"/> attributes where one expression may
        /// be dependent on another. Note that properties are normally evaluated in the order
        /// in which they are defined in the class, but this is not guaranteed, using this
        /// attribute allows the order of evaluation to be changed. 
        /// </para>
        /// <para>
        /// When no <see cref="EvaluationOrder"/> is specified, the sort order for a property
        /// will be zero. Properties will be ordered numerically based on this value.
        /// </para>
        /// <para>
        /// For any <see cref="Expression"/> value that references the <c>this</c> keyword,
        /// a positive evaluation order will be required.
        /// </para>
        /// <para>
        /// See <see cref="ValueExpressionParser{T}.CreateInstance{TExpressionScope}"/>.
        /// </para>
        /// </remarks>
        int EvaluationOrder { get; set; }

        /// <summary>
        /// Gets or sets any <see cref="evaluator.TypeRegistry"/> to use if the attribute
        /// <see cref="Expression"/> needs to be pre-parsed.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "TypeRegistry needs to be assigned when needed - default value is null")]
        TypeRegistry TypeRegistry { get; set; }

        /// <summary>
        /// Gets the <see cref="Expression"/> based value used to update a modeled property.
        /// </summary>
        /// <param name="property">Property from which attribute was derived.</param>
        /// <returns>Expression based on source property.</returns>
        /// <remarks>
        /// <para>
        /// This function allows derived attribute implementations to adjust the expression based
        /// on property information, e.g., property type.
        /// </para>
        /// <para>
        /// The property update value is typically used to assign expression values to a modeled type.
        /// </para>
        /// <para>
        /// Default implementation should return <c>Expression</c>.
        /// </para>
        /// </remarks>
        string GetPropertyUpdateValue(PropertyInfo property);

        /// <summary>
        /// Gets the modeled property based value used to update the <see cref="Expression"/>.
        /// </summary>
        /// <param name="property">Property from which attribute was derived.</param>
        /// <returns>Assignment expression based on source property.</returns>
        /// <remarks>
        /// <para>
        /// This function allows derived attribute implementations to adjust the update value
        /// based on property information, e.g., property type.
        /// </para>
        /// <para>
        /// The expression update value is typically used to assign modeled property values back
        /// to expressions allowing synchronization of a model with an external source, e.g., a
        /// user interface element.
        /// </para>
        /// <para>
        /// Default implementation should return <c>$"Instance.{property.Name}"</c>.
        /// </para>
        /// </remarks>
        string GetExpressionUpdateValue(PropertyInfo property);
    }
}