//******************************************************************************************************
//  TypeRegistry.cs - Gbtc
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

//#define DEBUG_CONTEXT_GENERATION

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Gemstone.IO;
using Gemstone.Reflection.MemberInfoExtensions;
using Gemstone.TypeExtensions;
using ConcurrentTypeHashSet = System.Collections.Concurrent.ConcurrentDictionary<System.Type, int>;

namespace Gemstone.Expressions.Evaluator;

/// <summary>
/// Defines a registry of types and symbols needed for an <see cref="ExpressionCompiler"/>.
/// </summary>
public class TypeRegistry
{
    #region [ Members ]

    // Constants
    private const string ContextTypeClassName = "Globals";
    private const string ContextTypeAssemblyFolder = "DynamicAssemblies";
    private const string InstanceProperties = "_instanceProperties";
    private const string InstanceFields = "_instanceFields";
    private const string InstanceVariables = "_instanceVariables";

    // Fields
    private readonly ConcurrentTypeHashSet m_registeredTypes;
    private readonly ConcurrentTypeHashSet m_registeredStaticTypes;
    private readonly ConcurrentDictionary<string, Symbol> m_registeredSymbols;
    private readonly ConcurrentDictionary<(Type resultType, Type instanceParameterType), (Type contextType, PropertyInfo[] instanceProperties, FieldInfo[] instanceFields, string[] variableNames)> m_contextTypeCache;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="TypeRegistry"/>.
    /// </summary>
    public TypeRegistry()
    {
        m_registeredTypes = new ConcurrentTypeHashSet();
        m_registeredStaticTypes = new ConcurrentTypeHashSet();
        m_registeredSymbols = new ConcurrentDictionary<string, Symbol>(StringComparer.Ordinal);
        m_contextTypeCache = new ConcurrentDictionary<(Type, Type), (Type, PropertyInfo[], FieldInfo[], string[])>();

        RegisterType(typeof(object));
        RegisterType(typeof(Enumerable));
        RegisterType(typeof(Expression));
        RegisterType(typeof(PropertyInfo));
        RegisterType(typeof(Dictionary<,>));
        RegisterType(typeof(ISupportContextVariables));
    }

    // Used to create a cloned TypeRegistry instance
    private TypeRegistry(IEnumerable<KeyValuePair<Type, int>> registeredTypes, IEnumerable<KeyValuePair<Type, int>> registeredStaticTypes, IEnumerable<KeyValuePair<string, Symbol>> registeredSymbols)
    {
        m_registeredTypes = new ConcurrentTypeHashSet(registeredTypes);
        m_registeredStaticTypes = new ConcurrentTypeHashSet(registeredStaticTypes);
        m_registeredSymbols = new ConcurrentDictionary<string, Symbol>(registeredSymbols);
        m_contextTypeCache = new ConcurrentDictionary<(Type, Type), (Type, PropertyInfo[], FieldInfo[], string[])>();
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets registered types including explicitly registered types and types for registered symbols.
    /// </summary>
    public IEnumerable<Type> RegisteredTypes
    {
        get
        {
            // Get explicitly registered types
            HashSet<Type> registeredTypes = [.. m_registeredTypes.Keys];

            // Append types for registered symbols
            registeredTypes.UnionWith(m_registeredSymbols.Select(symbol => symbol.Value.Type));

            return registeredTypes;
        }
    }

    /// <summary>
    /// Gets registered symbols.
    /// </summary>
    public IEnumerable<Symbol> RegisteredSymbols => m_registeredSymbols.Values;

    /// <summary>
    /// Gets value for registered symbol with specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Symbol name.</param>
    /// <returns>Symbol value or <c>null</c> if symbol <paramref name="name"/> does not exist.</returns>
    public object? this[string name] => m_registeredSymbols[name].Value;

    /// <summary>
    /// Sets value for registered symbol with specified <paramref name="name"/>,
    /// new symbol will be registered if symbol does not exist.
    /// </summary>
    /// <param name="name">Symbol name.</param>
    /// <param name="type">Symbol type.</param>
    public object this[string name, Type type]
    {
        set => RegisterSymbol(new Symbol(name, type, value));
    }

    /// <summary>
    /// Gets distinct assemblies for all <see cref="RegisteredTypes"/>.
    /// </summary>
    public IEnumerable<Assembly> Assemblies
    {
        get
        {
            HashSet<Assembly> assemblies = [];

            void addAssembly(string name)
            {
                Assembly? loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().
                    SingleOrDefault(assembly => assembly.GetName().Name == name);

                if (loadedAssembly is not null)
                    assemblies.Add(loadedAssembly);
            }

            addAssembly("netstandard");
            addAssembly("System.Runtime");
            addAssembly("Microsoft.CSharp");

            foreach (AssemblyName assemblyName in typeof(TypeRegistry).Assembly.GetReferencedAssemblies())
                addAssembly(assemblyName.Name!);

            foreach (Type type in RegisteredTypes)
                assemblies.Add(type.Assembly);

            return assemblies;
        }
    }

    /// <summary>
    /// Gets distinct namespaces for all <see cref="RegisteredTypes"/>.
    /// </summary>
    public IEnumerable<string> Namespaces
    {
        get
        {
            HashSet<string> namespaces = [];

            foreach (Type type in RegisteredTypes)
            {
                if (type.Namespace is not null)
                    namespaces.Add(type.Namespace);
            }

            return namespaces;
        }
    }

    /// <summary>
    /// Gets distinct static types for all types registered with <see cref="RegisterStaticType"/>.
    /// </summary>
    public IEnumerable<string> StaticTypes => m_registeredStaticTypes.Keys.Select(type => type.GetReflectedTypeName());

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Creates a cloned instance of this <see cref="TypeRegistry"/>.
    /// </summary>
    /// <returns>Cloned instance of this <see cref="TypeRegistry"/>.</returns>
    public TypeRegistry Clone()
    {
        return new TypeRegistry(m_registeredTypes, m_registeredStaticTypes, m_registeredSymbols);
    }

    /// <summary>
    /// Registers a new <see cref="Type"/>.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to register.</param>
    /// <returns>
    /// <c>true</c> if the <paramref name="type"/> was registered successfully; otherwise,
    /// <c>false</c> if the <paramref name="type"/> was already registered.
    /// </returns>
    public bool RegisterType(Type type)
    {
        return m_registeredTypes.TryAdd(type, 0);
    }

    /// <summary>
    /// Registers a new <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> to register.</typeparam>
    /// <returns>
    /// <c>true</c> if <typeparamref name="T"/> was registered successfully; otherwise,
    /// <c>false</c> if <typeparamref name="T"/> was already registered.
    /// </returns>
    public bool RegisterType<T>()
    {
        return RegisterType(typeof(T));
    }

    /// <summary>
    /// Registers a new static <see cref="Type"/>, i.e., for access in expressions with <c>static using</c>.
    /// </summary>
    /// <param name="type">Type to register as a static type.</param>
    /// <returns>
    /// <c>true</c> if the <paramref name="type"/> was registered successfully; otherwise,
    /// <c>false</c> if the <paramref name="type"/> was already registered.
    /// </returns>
    /// <remarks>
    /// Registering a static type will allow the type to be used in expressions without prefixing the type
    /// with its name, i.e., referenceable in code via a <c>static using</c>. This is useful for types that
    /// are used frequently e.g., <c>Math</c> or <c>Enumerable</c>. Note that adding a static using for a
    /// type will automatically register the type via <see cref="RegisterType"/>.
    /// </remarks>
    public bool RegisterStaticType(Type type)
    {
        RegisterType(type);
        return m_registeredStaticTypes.TryAdd(type, 0);
    }

    /// <summary>
    /// Registers a new static <see cref="Type"/>, i.e., for access in expressions with <c>static using</c>.
    /// </summary>
    /// <typeparam name="T">Type to register as a static type.</typeparam>
    /// <returns>
    /// <c>true</c> if the <typeparamref name="T"/> was registered successfully; otherwise,
    /// <c>false</c> if the <typeparamref name="T"/> was already registered.
    /// </returns>
    /// <remarks>
    /// Registering a static type will allow the type to be used in expressions without prefixing the type
    /// with its name, i.e., referenceable in code via a <c>static using</c>. This is useful for types that
    /// are used frequently e.g., <c>Math</c> or <c>Enumerable</c>. Note that adding a static using for a
    /// type will automatically register the type via <see cref="RegisterType"/>.
    /// </remarks>
    public bool RegisterStaticType<T>()
    {
        return RegisterStaticType(typeof(T));
    }

    /// <summary>
    /// Registers a new or updates an existing <see cref="Symbol"/>.
    /// </summary>
    /// <param name="symbol"><see cref="Symbol"/> to register.</param>
    public void RegisterSymbol(Symbol symbol)
    {
        if (symbol is null)
            throw new ArgumentNullException(nameof(symbol));

        if (symbol.Type == typeof(Type))
            throw new ArgumentException("Cannot register a type as a symbol. Call \"RegisterType\" instead.");

        // Check if symbol name with same type already exists
        if (m_registeredSymbols.TryGetValue(symbol.Name, out Symbol? existingSymbol) && existingSymbol.Type == symbol.Type)
        {
            // Update value of existing symbol
            existingSymbol.Value = symbol.Value;
        }
        else
        {
            // Adding new or updating type of registered symbols necessitates clearing any cached context types
            m_registeredSymbols[symbol.Name] = symbol;
            m_contextTypeCache.Clear();
        }
    }

    /// <summary>
    /// Registers a new or updates an existing <see cref="Symbol"/> with specified <paramref name="name"/> and <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of <paramref name="value"/>.</typeparam>
    /// <param name="name"><see cref="Symbol"/> name.</param>
    /// <param name="value"><see cref="Symbol"/> value.</param>
    public void RegisterSymbol<T>(string name, T value)
    {
        RegisterSymbol(new Symbol(name, typeof(T), value));
    }

    /// <summary>
    /// Gets a new context instance.
    /// </summary>
    /// <param name="resultType">Return value used <see cref="Type"/> for generated context type functions.</param>
    /// <param name="instanceParameterType">Instance parameter <see cref="Type"/> for generated context type functions.</param>
    /// <param name="variableContext">Optional <see cref="ISupportContextVariables"/> instance to use for context variables.</param>
    /// <returns>New context instance.</returns>
    /// <remarks>
    /// This method may generate a new runtime type based on registered types and symbols
    /// that is compiled into its own assembly and loaded into memory. The same cached type
    /// will be returned unless the registered types or symbols change. Since prior compiled
    /// assemblies cannot be unloaded, this property should only be called after all the
    /// desired types and symbols have been registered.
    /// </remarks>
    public object GetNewContext(Type resultType, Type instanceParameterType, ISupportContextVariables? variableContext = null)
    {
        (Type contextType, PropertyInfo[] instanceProperties, FieldInfo[] instanceFields, string[] variableNames) = 
            m_contextTypeCache.GetOrAdd((resultType, instanceParameterType), _ => 
                GenerateContextType(resultType, instanceParameterType, variableContext));

        object instance = Activator.CreateInstance(contextType)!;

        contextType.GetField(InstanceProperties)?.SetValue(instance, instanceProperties);
        contextType.GetField(InstanceFields)?.SetValue(instance, instanceFields);
        contextType.GetField(InstanceVariables)?.SetValue(instance, variableNames);

        foreach (Symbol symbol in m_registeredSymbols.Values)
        {
            FieldInfo? field = contextType.GetField(symbol.Name);

            if (field is null)
                continue;

            if (symbol.Type != typeof(Type) && symbol.Value is not null)
                field.SetValue(instance, symbol.Value);
        }

        return instance;
    }

    /// <summary>
    /// Gets a new context instance.
    /// </summary>
    /// <param name="variableContext">Optional <see cref="ISupportContextVariables"/> instance to use for context variables.</param>
    /// <typeparam name="TResult">Return value used <see cref="Type"/> for generated context type functions.</typeparam>
    /// <typeparam name="TInstanceParameter">Instance parameter <see cref="Type"/> for generated context type functions.</typeparam>
    /// <returns>New context instance.</returns>
    /// <remarks>
    /// This method may generate a new runtime type based on registered types and symbols
    /// that is compiled into its own assembly and loaded into memory. The same cached type
    /// will be returned unless the registered types or symbols change. Since prior compiled
    /// assemblies cannot be unloaded, this property should only be called after all the
    /// desired types and symbols have been registered.
    /// </remarks>
    public object GetNewContext<TResult, TInstanceParameter>(ISupportContextVariables? variableContext = null)
    {
        return GetNewContext(typeof(TResult), typeof(TInstanceParameter), variableContext);
    }

    /// <summary>
    /// Gets a compiled context type based on registered types and symbols.
    /// </summary>
    /// <param name="resultType">Return value <see cref="Type"/> used for generated context type functions.</param>
    /// <param name="instanceParameterType">Instance parameter <see cref="Type"/> for generated context type functions.</param>
    /// <param name="variableContext">Optional <see cref="ISupportContextVariables"/> instance to use for context variables.</param>
    /// <returns>Compiled context type based on registered types and symbols.</returns>
    /// <remarks>
    /// This method may generate a new runtime type based on registered types and symbols
    /// that is compiled into its own assembly and loaded into memory. The same cached type
    /// will be returned unless the registered types or symbols change. Since prior compiled
    /// assemblies cannot be unloaded, this property should only be called after all the
    /// desired types and symbols have been registered.
    /// </remarks>
    public Type GetContextType(Type resultType, Type instanceParameterType, ISupportContextVariables? variableContext = null)
    {
        return m_contextTypeCache.GetOrAdd((resultType, instanceParameterType), _ => GenerateContextType(resultType, instanceParameterType, variableContext)).contextType;
    }

    /// <summary>
    /// Gets a compiled context type based on registered types and symbols.
    /// </summary>
    /// <param name="variableContext">Optional <see cref="ISupportContextVariables"/> instance to use for context variables.</param>
    /// <typeparam name="TResult">Return value <see cref="Type"/> used for generated context type functions.</typeparam>
    /// <typeparam name="TInstanceParameter">Instance parameter <see cref="Type"/> for generated context type functions.</typeparam>
    /// <returns>Compiled context type based on registered types and symbols.</returns>
    /// <remarks>
    /// This method may generate a new runtime type based on registered types and symbols
    /// that is compiled into its own assembly and loaded into memory. The same cached type
    /// will be returned unless the registered types or symbols change. Since prior compiled
    /// assemblies cannot be unloaded, this property should only be called after all the
    /// desired types and symbols have been registered.
    /// </remarks>
    public Type GetContextType<TResult, TInstanceParameter>(ISupportContextVariables? variableContext = null)
    {
        return GetContextType(typeof(TResult), typeof(TInstanceParameter), variableContext);
    }

    private (Type, PropertyInfo[], FieldInfo[], string[]) GenerateContextType(Type resultType, Type instanceParameterType, ISupportContextVariables? variableContext)
    {
        PropertyInfo[] instanceProperties = instanceParameterType.GetProperties().Where(info => !info.AttributeExists<PropertyInfo, NotVisibleToExpressionAttribute>()).ToArray();
        FieldInfo[] instanceFields = instanceParameterType.GetFields().Where(info => !info.AttributeExists<FieldInfo, NotVisibleToExpressionAttribute>()).ToArray();
        Dictionary<string, Type> variablesTypes = variableContext?.GetVariablesTypes() ?? [];

        string generateFieldDefinitions()
        {
            const string FieldTemplate = "\r\n        public {0} {1};";
            StringBuilder fieldDefinitions = new();
            HashSet<string> fieldNames = new(StringComparer.Ordinal);

            void checkForDuplicate(string name, string exceptionPrefix)
            {
                if (fieldNames.Contains(name))
                    throw new AmbiguousMatchException($"{exceptionPrefix} is defined in registered symbols. Names must be unique in order to compile expression context type.");
            }

            // Add field definitions for registered symbols to context type
            foreach (Symbol symbol in m_registeredSymbols.Values.Where(symbol => symbol.Type != typeof(Type)).OrderBy(symbol => symbol.Name))
            {
                fieldDefinitions.AppendFormat(FieldTemplate, symbol.Type.GetReflectedTypeName(), symbol.Name);
                fieldNames.Add(symbol.Name);
            }

            checkForDuplicate(InstanceProperties, $"Reserved name \"{InstanceProperties}\"");
            checkForDuplicate(InstanceFields, $"Reserved name \"{InstanceFields}\"");
            checkForDuplicate(InstanceVariables, $"Reserved name \"{InstanceVariables}\"");

            // Add reserved instance names to field names so they can also be checked
            fieldNames.Add(InstanceProperties);
            fieldNames.Add(InstanceFields);
            fieldNames.Add(InstanceVariables);

            // Add field definitions for instance parameter properties to context type
            fieldDefinitions.AppendFormat(FieldTemplate, instanceProperties.GetType().GetReflectedTypeName(), InstanceProperties);

            foreach (PropertyInfo property in instanceProperties)
            {
                checkForDuplicate(property.Name, $"Property \"{property.Name}\" of \"{instanceParameterType.FullName}\"");
                fieldDefinitions.AppendFormat(FieldTemplate, property.PropertyType.GetReflectedTypeName(), property.Name);
            }

            // Add field definitions for instance parameter fields to context type
            fieldDefinitions.AppendFormat(FieldTemplate, instanceFields.GetType().GetReflectedTypeName(), InstanceFields);

            foreach (FieldInfo field in instanceFields)
            {
                checkForDuplicate(field.Name, $"Field \"{field.Name}\" of \"{instanceParameterType.FullName}\"");
                fieldDefinitions.AppendFormat(FieldTemplate, field.FieldType.GetReflectedTypeName(), field.Name);
            }

            // Add field definitions for context variables to context type
            fieldDefinitions.AppendFormat(FieldTemplate, "string[]", InstanceVariables);

            foreach (KeyValuePair<string, Type> variable in variablesTypes)
            {
                checkForDuplicate(variable.Key, $"Variable \"{variable.Key}\"");
                fieldDefinitions.AppendFormat(FieldTemplate, variable.Value.GetReflectedTypeName(), variable.Key);
            }

            return fieldDefinitions.ToString();
        }

        string contextTypeCodeTemplate =
            $$$"""
               using System;
               using System.Collections.Generic;
               using System.Reflection;
               using Gemstone.Expressions.Evaluator;
               
               namespace {0}
               {{
                   public class {{{ContextTypeClassName}}}
                   {{
                       {{{generateFieldDefinitions()}}}
               
                       public void ExecuteAction(object instance, Action action)
                       {{
                           UpdateFields(instance);
                           action();
                       }}
               
                       public {{{resultType.GetReflectedTypeName()}}} ExecuteFunc(object instance, Func<{{{resultType.GetReflectedTypeName()}}}> func)
                       {{
                           UpdateFields(instance);
                           return func();
                       }}
               
                       private void UpdateFields(object instance)
                       {{
                           Type type = GetType();
               
                           foreach (PropertyInfo property in {{{InstanceProperties}}})
                               type.GetField(property.Name).SetValue(this, property.GetValue(instance));
               
                           foreach (FieldInfo field in {{{InstanceFields}}})
                               type.GetField(field.Name).SetValue(this, field.GetValue(instance));
                               
                           if (!(instance is ISupportContextVariables context))
                               return;
                               
                           Dictionary<string, object?> variables = context.Variables;
               
                           foreach (string variableName in {{{InstanceVariables}}})
                           {{
                               if (variables.TryGetValue(variableName, out object? value))
                                   type.GetField(variableName).SetValue(this, value);
                           }}
                       }}
                   }}
               }}
               """;

        string codeHash = GetSha256(contextTypeCodeTemplate);
        string contextTypeNamespace = $"{nameof(Evaluator)}{codeHash}";

        // For now, we must write assembly to a file in order for Roslyn to use it as in-memory assembly defines
        // no assembly location. There may be a way around this, but this caches the assembly for reuse anyway.
        Assembly cacheAssembly(string _)
        {
            string assemblyDirectory = FilePath.GetAbsolutePath(ContextTypeAssemblyFolder);

            if (!Directory.Exists(assemblyDirectory))
            {
                try
                {
                    Directory.CreateDirectory(assemblyDirectory);
                }
                catch (IOException)
                {
                    // Multiple threads may be attempting same action
                }
            }

            string assemblyFileName = Path.Combine(assemblyDirectory, $"{contextTypeNamespace}.dll");

        #if !DEBUG_CONTEXT_GENERATION
            if (!File.Exists(assemblyFileName))
        #endif
            {
                // Add namespace with hashcode to context type code template
                string contextTypeCode = string.Format(contextTypeCodeTemplate, contextTypeNamespace);

                // Add assemblies for registered types, symbols and expression types
                HashSet<Assembly> assemblies = [.. Assemblies, resultType.Assembly, instanceParameterType.Assembly];

                // Also add assemblies for instance properties, fields and variables
                foreach (Type type in instanceProperties.Select(property => property.PropertyType))
                    assemblies.Add(type.Assembly);

                foreach (Type type in instanceFields.Select(field => field.FieldType))
                    assemblies.Add(type.Assembly);

                foreach (Type type in variablesTypes.Values)
                    assemblies.Add(type.Assembly);

                // Compile context type class
                byte[] assemblyBytes = RuntimeCompiler.Compile(contextTypeCode, assemblies, assemblyName: contextTypeNamespace);

                try
                {
                    File.WriteAllBytes(assemblyFileName, assemblyBytes);
                }
                catch (IOException)
                {
                    // Multiple threads may be attempting same action
                }
            }

            return Assembly.LoadFrom(assemblyFileName);
        }

        Assembly assembly = s_codeAssemblies.GetOrAdd(codeHash, cacheAssembly);

        Type contextType = assembly.GetType($"{contextTypeNamespace}.{ContextTypeClassName}")!;

        RegisterType(contextType);

        return (contextType, instanceProperties, instanceFields, variablesTypes.Keys.ToArray());
    }

    #endregion

    #region [ Static ]

    // Static Fields
    private static readonly ConcurrentDictionary<string, Assembly> s_codeAssemblies = new(StringComparer.Ordinal);

    // Static Properties

    /// <summary>
    /// Gets total count of generated context types.
    /// </summary>
    public static int GeneratedContextTypeCount => s_codeAssemblies.Count;

    // Static Methods
    private static string GetSha256(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        using SHA256 sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

    #endregion
}
