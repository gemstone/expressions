//******************************************************************************************************
//  EvalutatorTests.cs - Gbtc
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
using System.Reflection;
using Gemstone.Expressions.Evaluator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gemstone.Expressions.UnitTests
{
    public static class StringExtensions
    {
        public static string PadLeft(this string value, int length) => $"{new string(' ', length)}{value}";
    }

    [TestClass]
    public class EvalutatorTests
    {
        public const string RadarConst = "RADAR!TEST";

        public static readonly string StaticRadarField = RadarConst;

        public static string StaticRadarProperty => StaticRadarField;

        public static string StaticRadarMethod() => StaticRadarProperty;

        public string RadarProperty => StaticRadarProperty;

        public string RadarMethod() => RadarProperty;

        public class StaticDynamicTest
        {
            public dynamic Testing = new StaticDynamic(typeof(EvalutatorTests));
        }

        [TestMethod]
        public void StaticDynamicTests()
        {
            Assert.IsTrue(new StaticDynamicTest().Testing.RadarConst == RadarConst);
        }

        [TestMethod]
        public void BoolExpressionTests()
        {
            ExpressionCompiler<bool> boolTrue = new("true");
            ExpressionCompiler<bool> boolFalse = new("false");

            Assert.IsTrue(boolTrue.ExecuteFunction());
            Assert.IsFalse(boolFalse.ExecuteFunction());

            System.Console.WriteLine($"Context Type for '{nameof(boolTrue)}': {boolTrue.TypeRegistry.GetContextType<bool, object>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(boolTrue)}': {boolTrue}");

            System.Console.WriteLine($"Context Type for '{nameof(boolFalse)}': {boolFalse.TypeRegistry.GetContextType<bool, object>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(boolFalse)}': {boolFalse}");
            
            System.Console.WriteLine($"Generated Context Type Count = {TypeRegistry.GeneratedContextTypeCount:N0}");
        }

        [TestMethod]
        public void DoubleExpressionTests()
        {
            ExpressionCompiler<double> piConst = new("Math.PI");
            ExpressionCompiler<double> cosExpr = new("Math.Cos(Math.PI)");

            Assert.IsTrue(piConst.ExecuteFunction() == Math.PI);
            Assert.IsTrue(cosExpr.ExecuteFunction() == -1.0D);

            System.Console.WriteLine($"Context Type for '{nameof(piConst)}': {piConst.TypeRegistry.GetContextType<double, object>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(piConst)}': {piConst}");

            System.Console.WriteLine($"Context Type for '{nameof(cosExpr)}': {cosExpr.TypeRegistry.GetContextType<double, object>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(cosExpr)}': {cosExpr}");

            System.Console.WriteLine($"Generated Context Type Count = {TypeRegistry.GeneratedContextTypeCount:N0}");
        }

        [TestMethod]
        public void RegisteredSymbolInstanceAccessTests()
        {
            ExpressionCompiler regPropTest = new($"Testing.RadarProperty == \"{RadarConst}\"");
            ExpressionCompiler regMethodTest = new($"Testing.RadarMethod() == \"{RadarConst}\"");
            
            regPropTest.TypeRegistry.RegisterSymbol("Testing", this);
            regMethodTest.TypeRegistry.RegisterSymbol("Testing", this);

            Assert.IsTrue((bool)regPropTest.ExecuteFunction());
            Assert.IsTrue((bool)regMethodTest.ExecuteFunction());

            System.Console.WriteLine($"Context Type for '{nameof(regPropTest)}': {regPropTest.TypeRegistry.GetContextType<object, object>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(regPropTest)}': {regPropTest}");

            System.Console.WriteLine($"Context Type for '{nameof(regMethodTest)}': {regMethodTest.TypeRegistry.GetContextType<object, object>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(regMethodTest)}': {regMethodTest}");

            System.Console.WriteLine($"Generated Context Type Count = {TypeRegistry.GeneratedContextTypeCount:N0}");
        }

        [TestMethod]
        public void RegisteredTypeStaticAccessTests()
        {
            ExpressionCompiler regConstText = new($"EvalutatorTests.RadarConst == \"{RadarConst}\"");
            ExpressionCompiler regStaticFieldText = new($"EvalutatorTests.StaticRadarField == \"{RadarConst}\"");
            ExpressionCompiler regStaticPropText = new($"EvalutatorTests.StaticRadarProperty == \"{RadarConst}\"");
            ExpressionCompiler regStaticMethodTest = new($"EvalutatorTests.StaticRadarMethod() == \"{RadarConst}\"");

            regConstText.TypeRegistry.RegisterType(typeof(EvalutatorTests));
            regStaticFieldText.TypeRegistry.RegisterType(typeof(EvalutatorTests));
            regStaticPropText.TypeRegistry.RegisterType(typeof(EvalutatorTests));

            // Verify that type cannot be accessed via a symbol - currently CSharpScript does not allow dynamics, e.g., StaticDynamic
            Assert.ThrowsException<ArgumentException>(() => regStaticMethodTest.TypeRegistry.RegisterSymbol("Testing", typeof(EvalutatorTests)));
            regStaticMethodTest.TypeRegistry.RegisterType(typeof(EvalutatorTests));

            Assert.IsTrue((bool)regConstText.ExecuteFunction());
            Assert.IsTrue((bool)regStaticFieldText.ExecuteFunction());
            Assert.IsTrue((bool)regStaticPropText.ExecuteFunction());
            Assert.IsTrue((bool)regStaticMethodTest.ExecuteFunction());

            System.Console.WriteLine($"Context Type for '{nameof(regConstText)}': {regConstText.TypeRegistry.GetContextType<object, object>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(regConstText)}': {regConstText}");

            System.Console.WriteLine($"Context Type for '{nameof(regStaticFieldText)}': {regStaticFieldText.TypeRegistry.GetContextType<object, object>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(regStaticFieldText)}': {regStaticFieldText}");

            System.Console.WriteLine($"Context Type for '{nameof(regStaticPropText)}': {regStaticPropText.TypeRegistry.GetContextType<object, object>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(regStaticPropText)}': {regStaticPropText}");

            System.Console.WriteLine($"Context Type for '{nameof(regStaticMethodTest)}': {regStaticMethodTest.TypeRegistry.GetContextType<object, object>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(regStaticMethodTest)}': {regStaticMethodTest}");

            System.Console.WriteLine($"Generated Context Type Count = {TypeRegistry.GeneratedContextTypeCount:N0}");
        }

        [TestMethod]
        public void RegisteredTypeExtensionMethodTests()
        {
            ExpressionCompiler<string> regExtTest = new($"\"{RadarConst}\".PadLeft(12)");

            regExtTest.TypeRegistry.RegisterType(typeof(StringExtensions));

            Assert.IsTrue(regExtTest.ExecuteFunction().Trim().Equals(RadarConst));

            System.Console.WriteLine($"Context Type for '{nameof(regExtTest)}': {regExtTest.TypeRegistry.GetContextType<string, object>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(regExtTest)}': {regExtTest}");

            System.Console.WriteLine($"Generated Context Type Count = {TypeRegistry.GeneratedContextTypeCount:N0}");
        }

        public class TestParams
        {
            public EvalutatorTests Instance;
        }

        public void ShowData()
        {
            System.Console.WriteLine("Text from EvalutatorTests class");
        }

        [TestMethod]
        public void InstanceParameterTests()
        {
            ExpressionCompiler<string, TestParams> instPropTest = new("Instance.RadarProperty.Substring(0, 5)");
            ExpressionCompiler instMethodTest = new("Instance.ShowData()");

            TestParams testParams = new() { Instance = this };

            Assert.IsTrue(instPropTest.ExecuteFunction(testParams).Equals("RADAR"));

            instMethodTest.InstanceParameterType = typeof(TestParams);
            instMethodTest.ExecuteAction(testParams);

            // When instance parameter type is defined, expressions expecting parameter should fail:
            Assert.ThrowsException<TargetException>(() => instMethodTest.ExecuteAction()); // This creates a new context type

            System.Console.WriteLine($"Context Type for '{nameof(instPropTest)}': {instPropTest.TypeRegistry.GetContextType<string, TestParams>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(instPropTest)}': {instPropTest}");

            System.Console.WriteLine($"Context Type for '{nameof(instMethodTest)}': {instMethodTest.TypeRegistry.GetContextType<object, TestParams>()?.FullName}");
            System.Console.WriteLine($"Expression for '{nameof(instMethodTest)}': {instMethodTest}");

            System.Console.WriteLine($"Generated Context Type Count = {TypeRegistry.GeneratedContextTypeCount:N0}");
        }
    }
}
