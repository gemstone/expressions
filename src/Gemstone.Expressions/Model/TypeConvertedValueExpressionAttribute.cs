﻿//******************************************************************************************************
//  TypeConvertedValueExpressionAttribute.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  05/02/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Gemstone.Expressions.Evaluator;

namespace Gemstone.Expressions.Model
{
    /// <summary>
    /// Defines a C# expression attribute that when evaluated will type convert its value for a property.
    /// </summary>
    /// <remarks>
    /// This attribute is typically used when synchronizing modeled values to an external sources, e.g.,
    /// user interface elements.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TypeConvertedValueExpressionAttribute : ValueExpressionAttributeBase
    {
        /// <summary>
        /// Gets the return <see cref="Type"/> for this <see cref="ValueExpressionAttributeBase.Expression"/>.
        /// </summary>
        /// <remarks>
        /// When value is <c>null</c>, the <see cref="ValueExpressionAttributeBase.Expression"/> will be
        /// pre-parsed in an attempt to auto-derive the return type. Note that when using this attribute,
        /// it often will be necessary to assign the current <see cref="TypeRegistry"/> so that the parser
        /// will have access to needed symbols. If the expression's return type is known in advance, it
        /// is optimal to provide it in the attribute constructor.
        /// </remarks>
        public Type? ReturnType { get; private set; }

        /// <summary>
        /// Creates a new <see cref="TypeConvertedValueExpressionAttribute"/>
        /// </summary>
        /// <param name="expression">C# expression that will evaluate to the type converted property value.</param>
        /// <param name="returnType">
        /// Return type for specified C# <paramref name="expression"/>; defaults to <c>null</c> to auto-derive
        /// the <paramref name="expression"/> return type by pre-parsing expression.
        /// </param>
        /// <remarks>
        /// When the <paramref name="returnType"/> is known in advance, it is optimal to provide it.
        /// </remarks>
        public TypeConvertedValueExpressionAttribute(string expression, Type? returnType = null) : base(expression) => ReturnType = returnType;

        /// <summary>
        /// Gets the <see cref="ValueExpressionAttributeBase.Expression"/> based value used to update a modeled property.
        /// </summary>
        /// <param name="property">Property from which attribute was derived.</param>
        /// <returns>Expression based on source property.</returns>
        /// <remarks>
        /// The property update value is typically used to assign expression values to a modeled type. For example:
        /// <code>
        /// [TypeConvertedValueExpression("Form.maskedTextBoxMessageInterval.Text", typeof(string))]
        /// public int MessageInterval { get; set; }
        /// </code>
        /// Would generate an expression of "Common.TypeConvertFromString(Form.maskedTextBoxMessageInterval.Text, typeof(int))"
        /// which would be executed as part of an overall expression that looked like
        /// <c>Instance.MessageInterval = Common.TypeConvertFromString(Form.maskedTextBoxMessageInterval.Text, typeof(int))</c>
        /// when called from <see cref="ValueExpressionParser{T}.UpdateProperties"/>.
        /// </remarks>
        public override string GetPropertyUpdateValue(PropertyInfo property)
        {
            Type sourceType = property.PropertyType;

            if (ReturnType == null)
                DeriveReturnType();

            if (ReturnType == sourceType)
                return $"{Expression}";

            if (ReturnType == typeof(string))
                return $"Common.TypeConvertFromString({Expression}, typeof({sourceType.FullName}))";

            return $"Convert.ChangeType({Expression}, typeof({sourceType.FullName}))";
        }

        /// <summary>
        /// Gets the modeled property based value used to update the <see cref="ValueExpressionAttributeBase.Expression"/>.
        /// </summary>
        /// <param name="property">Property from which attribute was derived.</param>
        /// <returns>Expression based on source property.</returns>
        /// <remarks>
        /// The expression update value is typically used to assign modeled property values back
        /// to expressions allowing synchronization of a model with an external source, e.g., a
        /// user interface element. For example:
        /// <code>
        /// [TypeConvertedValueExpression("Form.maskedTextBoxMessageInterval.Text", typeof(string))]
        /// public int MessageInterval { get; set; }
        /// </code>
        /// Would generate an update expression of "Common.TypeConvertToString(Instance.MessageInterval)"
        /// which would be executed as part of an overall expression that looked like
        /// <c>Form.maskedTextBoxMessageInterval.Text = Common.TypeConvertToString(Instance.MessageInterval)</c>
        /// when called from <see cref="ValueExpressionParser{T}.UpdateExpressions"/>.
        /// </remarks>
        public override string GetExpressionUpdateValue(PropertyInfo property)
        {
            if (ReturnType == null)
                DeriveReturnType();

            if (ReturnType == property.PropertyType)
                return $"Instance.{property.Name}";

            if (ReturnType == typeof(string))
                return $"Common.TypeConvertToString(Instance.{property.Name})";

            string returnType = ReturnType?.FullName ?? typeof(object).FullName;
            return $"Convert.ChangeType(Instance.{property.Name}, typeof({returnType}))";
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        private void DeriveReturnType()
        {
            try
            {
                ValueExpressionParser parser = new ValueExpressionParser(Expression);

                if (TypeRegistry != null)
                    parser.TypeRegistry = TypeRegistry;

                ReturnType = parser.ExecuteFunction().GetType();
            }
            catch (Exception ex)
            {
                ReturnType = typeof(object);
                LibraryEvents.OnSuppressedException(this, ex);
            }
        }
    }
}
