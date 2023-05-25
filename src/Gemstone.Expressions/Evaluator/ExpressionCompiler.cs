//******************************************************************************************************
//  ExpressionCompiler.cs - Gbtc
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
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Gemstone.Expressions.Evaluator
{
    /// <summary>
    /// Represents a runtime C# expression evaluator.
    /// </summary>
    public class ExpressionCompiler : ExpressionCompiler<object>
    {
        /// <summary>
        /// Creates a new <see cref="ExpressionCompiler"/>.
        /// </summary>
        /// <param name="expression">C# expression to compile.</param>
        public ExpressionCompiler(string expression) : base(expression)
        {
        }
    }

    /// <summary>
    /// Represents a runtime C# expression evaluator, strongly typed for a specific return value <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">Return value <see cref="Type"/> for function based expressions.</typeparam>
    public class ExpressionCompiler<TResult> : ExpressionCompiler<TResult, object>
    {
        /// <summary>
        /// Creates a new <see cref="ExpressionCompiler{TResult}"/>.
        /// </summary>
        /// <param name="expression">C# expression to compile.</param>
        public ExpressionCompiler(string expression) : base(expression)
        {
        }

        /// <summary>
        /// Gets <see cref="Action"/> delegate for compiled expression.
        /// </summary>
        /// <remarks>
        /// Calling this property will automatically compile <see cref="Expression"/>, if not already compiled.
        /// </remarks>
        public new Action CompiledAction => () => base.CompiledAction?.Invoke(null);

        /// <summary>
        /// Gets <see cref="Func{TResult}"/> delegate for compiled expression.
        /// </summary>
        /// <remarks>
        /// Calling this property will automatically compile <see cref="Expression"/>, if not already compiled.
        /// </remarks>
        public new Func<TResult> CompiledFunction => () =>
        {
            Func<object?, TResult>? compiledFunction = base.CompiledFunction;
            return compiledFunction is not null ? compiledFunction(null) : default!;
        };

        /// <summary>
        /// Executes compiled <see cref="Action"/> based expression.
        /// </summary>
        /// <remarks>
        /// Calling this method will automatically compile <see cref="Expression"/>, if not already compiled.
        /// </remarks>
        public void ExecuteAction() => CompiledAction();

        /// <summary>
        /// Executes compiled <see cref="Func{TResult}"/> based expression.
        /// </summary>
        /// <returns>Evaluated expression result.</returns>
        /// <remarks>
        /// Calling this method will automatically compile <see cref="Expression"/>, if not already compiled.
        /// </remarks>
        public TResult ExecuteFunction() => CompiledFunction();
    }

    /// <summary>
    /// Represents a runtime C# expression evaluator, strongly typed for a specific return value <typeparamref name="TResult"/>
    /// and instance parameter values <typeparamref name="TInstanceParameter"/>.
    /// </summary>
    /// <typeparam name="TResult">Return value <see cref="Type"/> for function based expressions.</typeparam>
    /// <typeparam name="TInstanceParameter">Instance parameter <see cref="Type"/> used to define expression accessible field values.</typeparam>
    public class ExpressionCompiler<TResult, TInstanceParameter> where TInstanceParameter : class
    {
        private TypeRegistry m_typeRegistry;
        private Action<TInstanceParameter?>? m_compiledAction;
        private Func<TInstanceParameter?, TResult>? m_compiledFunction;

        /// <summary>
        /// Creates a new <see cref="ExpressionCompiler{TResult, TInstanceParameter}"/>.
        /// </summary>
        /// <param name="expression">C# expression to compile.</param>
        public ExpressionCompiler(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentNullException(nameof(expression));
            
            m_typeRegistry = new TypeRegistry();
            Expression = expression;
        }

        /// <summary>
        /// Gets or sets the instance parameter <see cref="Type"/> used for defining expression accessible
        /// field values, defaults to <typeparamref name="TInstanceParameter"/>.
        /// </summary>
        public Type InstanceParameterType { get; set; } = typeof(TInstanceParameter);

        /// <summary>
        /// Gets the C# code expression to compile.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Gets the compiled <see cref="System.Linq.Expressions.Expression">Linq Expression</see> after <see cref="Expression">C# Code Expression</see> is compiled.
        /// </summary>
        public Expression? CompiledExpression { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="Evaluator.TypeRegistry"/> used for compilation.
        /// </summary>
        public TypeRegistry TypeRegistry
        {
            get => m_typeRegistry;
            set => m_typeRegistry = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets <see cref="Action{TInstanceParameter}"/> delegate for compiled expression.
        /// </summary>
        /// <remarks>
        /// Calling this property will automatically compile <see cref="Expression"/>, if not already compiled.
        /// </remarks>
        public Action<TInstanceParameter?>? CompiledAction
        {
            get
            {
                Action<TInstanceParameter?>? getCompiledAction()
                {
                    if (CompiledExpression is null)
                        Compile(true);

                    return (CompiledExpression as Expression<Action<TInstanceParameter?>>)?.Compile();
                }

                return m_compiledAction ??= getCompiledAction();
            }
        }

        /// <summary>
        /// Gets <see cref="Func{TInstanceParameter, TResult}"/> delegate for compiled expression.
        /// </summary>
        /// <remarks>
        /// Calling this property will automatically compile <see cref="Expression"/>, if not already compiled.
        /// </remarks>
        public Func<TInstanceParameter?, TResult>? CompiledFunction
        {
            get
            {
                Func<TInstanceParameter?, TResult>? getCompiledFunction()
                {
                    if (CompiledExpression is null)
                        Compile();

                    return (CompiledExpression as Expression<Func<TInstanceParameter?, TResult>>)?.Compile();
                }

                return m_compiledFunction ??= getCompiledFunction();
            }
        }

        /// <summary>
        /// Compiles C# <see cref="Expression"/>.
        /// </summary>
        /// <param name="isMethodCall">
        /// <c>true</c> for <see cref="Action{TInstanceParameter}"/> based expressions; otherwise,
        /// <c>false</c> for <see cref="Func{TInstanceParameter, TResult}"/> based expressions.
        /// </param>
        public void Compile(bool isMethodCall = false)
        {
            TypeRegistry typeRegistry = TypeRegistry;

            ScriptOptions options = ScriptOptions.Default
                .WithReferences(typeRegistry.Assemblies)
                .WithImports(typeRegistry.Namespaces)
                .WithOptimizationLevel(OptimizationLevel.Release);

            object context = typeRegistry.GetNewContext(typeof(TResult), InstanceParameterType ?? typeof(TInstanceParameter));

            if (isMethodCall)
                CompiledExpression = CSharpScript.EvaluateAsync<Expression<Action<TInstanceParameter?>>>($"(instance) => ExecuteAction(instance, () => {Expression})", options, context).Result;
            else
                CompiledExpression = CSharpScript.EvaluateAsync<Expression<Func<TInstanceParameter?, TResult>>>($"(instance) => ExecuteFunc(instance, () => {Expression})", options, context).Result;
        }

        /// <summary>
        /// Executes compiled <see cref="Action{TInstanceParameter}"/> based expression.
        /// </summary>
        /// <param name="instance">Instance parameter with current field values as needed by expression.</param>
        /// <remarks>
        /// Calling this method will automatically compile <see cref="Expression"/>, if not already compiled.
        /// </remarks>
        public void ExecuteAction(TInstanceParameter? instance) => CompiledAction?.Invoke(instance);

        /// <summary>
        /// Executes compiled <see cref="Func{TInstanceParameter, TResult}"/> based expression.
        /// </summary>
        /// <param name="instance">Instance parameter with current field values as needed by expression.</param>
        /// <returns>Evaluated expression result.</returns>
        /// <remarks>
        /// Calling this method will automatically compile <see cref="Expression"/>, if not already compiled.
        /// </remarks>
        public TResult ExecuteFunction(TInstanceParameter? instance)
        {
            Func<TInstanceParameter?, TResult>? compiledFunction = CompiledFunction;
            return compiledFunction is not null ? compiledFunction(instance) : default!;
        }

        /// <summary>
        /// Returns a string that represents the <see cref="ExpressionCompiler{TResult, TInstanceParameter}"/>, i.e., the <see cref="Expression"/> value.
        /// </summary>
        /// <returns>The <see cref="Expression"/> value.</returns>
        public override string ToString() => Expression;
    }
}
