//******************************************************************************************************
//  ExpressionContextCompiler.cs - Gbtc
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
//  03/06/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Linq.Expressions;

namespace Gemstone.Expressions.Evaluator;

/// <summary>
/// Represents a runtime C# expression evaluator, for any return value types and
/// a general purpose <see cref="ExpressionContext"/>.
/// </summary>
public class ExpressionContextCompiler : ExpressionContextCompiler<object>
{
    /// <summary>
    /// Creates a new <see cref="ExpressionContextCompiler"/>.
    /// </summary>
    /// <param name="expression">C# expression to compile.</param>
    /// <param name="context">Expression context used to define expression accessible field values.</param>
    public ExpressionContextCompiler(string expression, ExpressionContext context) : base(expression, context) { }
}

/// <summary>
/// Represents a runtime C# expression evaluator, strongly typed for a specific return value <typeparamref name="TResult"/>
/// and a general purpose <see cref="ExpressionContext"/>.
/// </summary>
/// <typeparam name="TResult">Return value <see cref="Type"/> for function based expressions.</typeparam>
public class ExpressionContextCompiler<TResult> : ExpressionContextCompiler<TResult, object>
{
    /// <summary>
    /// Creates a new <see cref="ExpressionContextCompiler{TResult}"/>.
    /// </summary>
    /// <param name="expression">C# expression to compile.</param>
    /// <param name="context">Expression context used to define expression accessible field values.</param>
    public ExpressionContextCompiler(string expression, ExpressionContext context) : base(expression, context) { }
}

/// <summary>
/// Represents a runtime C# expression evaluator, strongly typed for a specific return value <typeparamref name="TResult"/>
/// and an <see cref="ExpressionContext{T}"/> with <typeparamref name="TContextType"/> variables.
/// </summary>
/// <typeparam name="TResult">Return value <see cref="Type"/> for function based expressions.</typeparam>
/// <typeparam name="TContextType">Expression context <see cref="Type"/> for variables.</typeparam>
public class ExpressionContextCompiler<TResult, TContextType> : ExpressionCompiler<TResult, ExpressionContext<TContextType>>
{
    /// <summary>
    /// Creates a new <see cref="ExpressionContextCompiler{TResult, TContextType}"/>.
    /// </summary>
    /// <param name="expression">C# expression to compile.</param>
    /// <param name="context">Expression context used to define expression accessible field values.</param>
    public ExpressionContextCompiler(string expression, ExpressionContext<TContextType> context) : base(expression)
    {
        VariableContext = context;
        TypeRegistry = context.Imports;
    }

    /// <summary>
    /// Gets or sets the <see cref="ExpressionContext{TContextType}"/> instance to use for context variables.
    /// </summary>
    public new ExpressionContext<TContextType> VariableContext
    {
        get => (ExpressionContext<TContextType>)base.VariableContext!;
        set => base.VariableContext = value;
    }

    /// <summary>
    /// Executes compiled <see cref="Action"/> based expression.
    /// </summary>
    /// <remarks>
    /// Calling this method will automatically compile <see cref="Expression"/>, if not already compiled.
    /// Method automatically applies <see cref="VariableContext"/> variables for execution.
    /// </remarks>
    public void ExecuteAction()
    {
        ExecuteAction(VariableContext);
    }

    /// <summary>
    /// Executes compiled <see cref="Func{TResult}"/> based expression.
    /// </summary>
    /// <returns>Evaluated expression result.</returns>
    /// <remarks>
    /// Calling this method will automatically compile <see cref="Expression"/>, if not already compiled.
    /// Function automatically applies <see cref="VariableContext"/> variables for execution.
    /// </remarks>
    public TResult ExecuteFunction()
    {
        return ExecuteFunction(VariableContext);
    }
}
