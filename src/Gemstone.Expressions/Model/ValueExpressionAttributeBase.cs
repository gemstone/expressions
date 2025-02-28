//******************************************************************************************************
//  ValueExpressionAttributeBase.cs - Gbtc
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
//  04/07/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Reflection;
using Gemstone.Expressions.Evaluator;

namespace Gemstone.Expressions.Model
{
    /// <summary>
    /// Represents a base attribute class for C# expressions that when evaluated will specify a new value for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ValueExpressionAttributeBase : Attribute, IValueExpressionAttribute
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ValueExpressionAttributeBase"/>
        /// </summary>
        /// <param name="expression">C# expression that will evaluate to the desired value.</param>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="expression"/> cannot be <c>null</c>.</exception>
        protected ValueExpressionAttributeBase(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentNullException(nameof(expression));

            Expression = expression;
        }

        #endregion

        #region [ Properties ]

        /// <inheritdoc />
        public string Expression { get; }

        /// <inheritdoc />
        public bool Cached { get; set; }

        /// <inheritdoc />
        public int EvaluationOrder { get; set; }

        /// <inheritdoc />
        public TypeRegistry? TypeRegistry { get; set; }

        #endregion

        #region [ Methods ]

        /// <inheritdoc />
        public virtual string GetPropertyUpdateValue(PropertyInfo property)
        {
            return Expression;
        }

        /// <inheritdoc />
        public virtual string GetExpressionUpdateValue(PropertyInfo property)
        {
            return $"Instance.{property.Name}";
        }

        #endregion
    }
}
