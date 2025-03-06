//******************************************************************************************************
//  RuntimeCompiler.cs - Gbtc
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Gemstone.Expressions;

/// <summary>
/// Runtime C# code compiler.
/// </summary>
public static class RuntimeCompiler
{
    /// <summary>
    /// Compiles C# <paramref name="code"/> to a raw <see cref="Assembly"/>.
    /// </summary>
    /// <param name="code">C# code to compile.</param>
    /// <param name="references">References to use for compilation.</param>
    /// <param name="options">Compilation options.</param>
    /// <param name="assemblyName">Assembly name to use.</param>
    /// <returns>Raw <see cref="byte"/> array representing compiled <see cref="Assembly"/>.</returns>
    public static byte[] Compile(string code, IEnumerable<Assembly> references, CSharpCompilationOptions? options = null, string? assemblyName = null)
    {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName ?? Path.GetRandomFileName(),
                [ syntaxTree ],
                references.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)),
                options ?? new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release));

            using MemoryStream stream = new();

            EmitResult result = compilation.Emit(stream);

            if (!result.Success)
            {
                Exception[] exceptions = result.Diagnostics
                    .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                    .Select(diagnostic => new Exception($"{diagnostic.Id}: {diagnostic.GetMessage()}"))
                    .ToArray();

                if (exceptions.Length > 0)
                    throw new AggregateException(exceptions);
            }

            return stream.ToArray();
        }
}
