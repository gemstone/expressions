//******************************************************************************************************
//  ExpressionContext.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  03/05/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gemstone.Expressions.Evaluator;

/// <inheritdoc/>
public class ExpressionContext : ExpressionContext<object>
{
}

/// <summary>
/// Represents a dynamic expression context for evaluating expressions with variables and imports.
/// </summary>
/// <remarks>
/// All variables that can be used in an expression should be defined before the expression is
/// compiled. For compilation, variable values can simply be initialized with default values and
/// then updated later before the expression is evaluated.
/// </remarks>
public class ExpressionContext<T> : ISupportContextVariables
{
    /// <summary>
    /// Gets the type registry used to resolve imported types for an expression.
    /// </summary>
    [NotVisibleToExpression]
    public TypeRegistry Imports { get; } = new();

    /// <summary>
    /// Gets the dictionary of variables that can be accessed in an expression.
    /// </summary>
    [NotVisibleToExpression]
    public Dictionary<string, T> Variables { get; } = [];

    /// <summary>
    /// Gets or sets the default value to use for undefined variables.
    /// </summary>
    [NotVisibleToExpression]
    public T DefaultValue { get; set; } = default!;

    Dictionary<string, object?> ISupportContextVariables.Variables => 
        Variables.ToDictionary(kvp => kvp.Key, kvp => (object?)(kvp.Value ?? DefaultValue));

    Dictionary<string, Type> ISupportContextVariables.GetVariablesTypes()
    {
        Dictionary<string, object?> variables = ((ISupportContextVariables)this).Variables;
        Dictionary<string, Type> variablesTypes = [];

        foreach (KeyValuePair<string, object?> variable in variables)
            variablesTypes.Add(variable.Key, variable.Value?.GetType() ?? typeof(T));

        return variablesTypes;
    }
}
