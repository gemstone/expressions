//******************************************************************************************************
//  Symbol.cs - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  11/04/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace gemstone.expressions.evaluator
{
    /// <summary>
    /// Represents a symbol consisting of a name, <see cref="Type"/> and value to be accessible for
    /// expressions used with the <see cref="ExpressionCompiler{TResult, TInstanceParameter}"/>.
    /// </summary>
    public class Symbol
    {
        /// <summary>
        /// Creates a new <see cref="Symbol"/>.
        /// </summary>
        /// <param name="name">Name of symbol.</param>
        /// <param name="type"><see cref="Type"/> of symbol.</param>
        public Symbol(string name, Type type)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Creates a new <see cref="Symbol"/>.
        /// </summary>
        /// <param name="name">Name of symbol.</param>
        /// <param name="type"><see cref="Type"/> of symbol.</param>
        /// <param name="value">Initial value for symbol.</param>
        public Symbol(string name, Type type, object value) : this(name, type) => Value = value;

        /// <summary>
        /// Gets the symbol name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the symbol <see cref="Type"/>.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets or sets the symbol value.
        /// </summary>
		public object Value { get; set; }
    }
}
