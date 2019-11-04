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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gemstone.expressions;
using gemstone.expressions.evaluator;

namespace UnitTests
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
            ExpressionCompiler<bool> boolTrue = new ExpressionCompiler<bool>("true");
            ExpressionCompiler<bool> boolFalse = new ExpressionCompiler<bool>("false");

            Assert.IsTrue(boolTrue.ExecuteFunction());
            Assert.IsFalse(boolFalse.ExecuteFunction());

            Console.WriteLine($"Context Type for '{nameof(boolTrue)}': {boolTrue.TypeRegistry.GetContextType<bool, object>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(boolTrue)}': {boolTrue}");

            Console.WriteLine($"Context Type for '{nameof(boolFalse)}': {boolFalse.TypeRegistry.GetContextType<bool, object>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(boolFalse)}': {boolFalse}");
            
            Console.WriteLine($"Generated Context Type Count = {TypeRegistry.GeneratedContextTypeCount:N0}");
        }

        [TestMethod]
        public void DoubleExpressionTests()
        {
            ExpressionCompiler<double> piConst = new ExpressionCompiler<double>("Math.PI");
            ExpressionCompiler<double> cosExpr = new ExpressionCompiler<double>("Math.Cos(Math.PI)");

            Assert.IsTrue(piConst.ExecuteFunction() == Math.PI);
            Assert.IsTrue(cosExpr.ExecuteFunction() == -1.0D);

            Console.WriteLine($"Context Type for '{nameof(piConst)}': {piConst.TypeRegistry.GetContextType<double, object>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(piConst)}': {piConst}");

            Console.WriteLine($"Context Type for '{nameof(cosExpr)}': {cosExpr.TypeRegistry.GetContextType<double, object>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(cosExpr)}': {cosExpr}");

            Console.WriteLine($"Generated Context Type Count = {TypeRegistry.GeneratedContextTypeCount:N0}");
        }

        [TestMethod]
        public void RegisteredSymbolInstanceAccessTests()
        {
            ExpressionCompiler regPropTest = new ExpressionCompiler($"Testing.RadarProperty == \"{RadarConst}\"");
            ExpressionCompiler regMethodTest = new ExpressionCompiler($"Testing.RadarMethod() == \"{RadarConst}\"");
            
            regPropTest.TypeRegistry.RegisterSymbol("Testing", this);
            regMethodTest.TypeRegistry.RegisterSymbol("Testing", this);

            Assert.IsTrue((bool)regPropTest.ExecuteFunction());
            Assert.IsTrue((bool)regMethodTest.ExecuteFunction());

            Console.WriteLine($"Context Type for '{nameof(regPropTest)}': {regPropTest.TypeRegistry.GetContextType<object, object>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(regPropTest)}': {regPropTest}");

            Console.WriteLine($"Context Type for '{nameof(regMethodTest)}': {regMethodTest.TypeRegistry.GetContextType<object, object>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(regMethodTest)}': {regMethodTest}");

            Console.WriteLine($"Generated Context Type Count = {TypeRegistry.GeneratedContextTypeCount:N0}");
        }

        [TestMethod]
        public void RegisteredTypeStaticAccessTests()
        {
            ExpressionCompiler regConstText = new ExpressionCompiler($"EvalutatorTests.RadarConst == \"{RadarConst}\"");
            ExpressionCompiler regStaticFieldText = new ExpressionCompiler($"EvalutatorTests.StaticRadarField == \"{RadarConst}\"");
            ExpressionCompiler regStaticPropText = new ExpressionCompiler($"EvalutatorTests.StaticRadarProperty == \"{RadarConst}\"");
            ExpressionCompiler regStaticMethodTest = new ExpressionCompiler($"EvalutatorTests.StaticRadarMethod() == \"{RadarConst}\"");

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

            Console.WriteLine($"Context Type for '{nameof(regConstText)}': {regConstText.TypeRegistry.GetContextType<object, object>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(regConstText)}': {regConstText}");

            Console.WriteLine($"Context Type for '{nameof(regStaticFieldText)}': {regStaticFieldText.TypeRegistry.GetContextType<object, object>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(regStaticFieldText)}': {regStaticFieldText}");

            Console.WriteLine($"Context Type for '{nameof(regStaticPropText)}': {regStaticPropText.TypeRegistry.GetContextType<object, object>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(regStaticPropText)}': {regStaticPropText}");

            Console.WriteLine($"Context Type for '{nameof(regStaticMethodTest)}': {regStaticMethodTest.TypeRegistry.GetContextType<object, object>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(regStaticMethodTest)}': {regStaticMethodTest}");

            Console.WriteLine($"Generated Context Type Count = {TypeRegistry.GeneratedContextTypeCount:N0}");
        }

        [TestMethod]
        public void RegisteredTypeExtensionMethodTests()
        {
            ExpressionCompiler<string> regExtTest = new ExpressionCompiler<string>($"\"{RadarConst}\".PadLeft(12)");

            regExtTest.TypeRegistry.RegisterType(typeof(StringExtensions));

            Assert.IsTrue(regExtTest.ExecuteFunction().Trim().Equals(RadarConst));

            Console.WriteLine($"Context Type for '{nameof(regExtTest)}': {regExtTest.TypeRegistry.GetContextType<string, object>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(regExtTest)}': {regExtTest}");

            Console.WriteLine($"Generated Context Type Count = {TypeRegistry.GeneratedContextTypeCount:N0}");
        }

        public class TestParams
        {
            public EvalutatorTests Instance;
        }

        public void ShowData()
        {
            Console.WriteLine("Text from EvalutatorTests class");
        }

        [TestMethod]
        public void InstanceParameterTests()
        {
            ExpressionCompiler<string, TestParams> instPropTest = new ExpressionCompiler<string, TestParams>("Instance.RadarProperty.Substring(0, 5)");
            ExpressionCompiler instMethodTest = new ExpressionCompiler("Instance.ShowData()");

            TestParams testParams = new TestParams { Instance = this };

            Assert.IsTrue(instPropTest.ExecuteFunction(testParams).Equals("RADAR"));

            instMethodTest.InstanceParameterType = typeof(TestParams);
            instMethodTest.ExecuteAction(testParams);

            // When instance parameter type is defined, expressions expecting parameter should fail:
            Assert.ThrowsException<TargetException>(() => instMethodTest.ExecuteAction()); // This creates a new context type

            Console.WriteLine($"Context Type for '{nameof(instPropTest)}': {instPropTest.TypeRegistry.GetContextType<string, TestParams>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(instPropTest)}': {instPropTest}");

            Console.WriteLine($"Context Type for '{nameof(instMethodTest)}': {instMethodTest.TypeRegistry.GetContextType<object, TestParams>()?.FullName}");
            Console.WriteLine($"Expression for '{nameof(instMethodTest)}': {instMethodTest}");

            Console.WriteLine($"Generated Context Type Count = {TypeRegistry.GeneratedContextTypeCount:N0}");
        }
    }
}
