//******************************************************************************************************
//  ModelTests.cs - Gbtc
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
//  12/30/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Security.Principal;
using Gemstone.Expressions.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gemstone.Expressions.UnitTests
{
    public static class Defaults
    {
        public const double DoubleVal = 5.0;
        public const int IntVal = 10;
        public const long LongVal = 100000L;
        public const bool BoolVal = true;
        public static readonly string CurrentUser = WindowsIdentity.GetCurrent().Name;
    }

    public class SimpleModel
    {
        static SimpleModel()
        {
            ValueExpressionParser.DefaultTypeRegistry.RegisterType<WindowsIdentity>();
        }

        [DefaultValueExpression("Guid.NewGuid()")]
        public Guid ID { get; set; }

        [DefaultValue(Defaults.DoubleVal)]
        public double DoubleVal { get; set; }

        [DefaultValue(Defaults.IntVal)]
        public int IntVal { get; set; }

        [DefaultValue(Defaults.LongVal)]
        public long LongVal { get; set; }

        [DefaultValue(Defaults.BoolVal)]
        public bool BoolVal { get; set; }

        [DefaultValueExpression("DateTime.UtcNow")]
        public DateTime CreatedOn { get; set; }

        [DefaultValueExpression("WindowsIdentity.GetCurrent().Name")]
        public string CreatedBy { get; set; }

        [DefaultValueExpression("this.CreatedOn", EvaluationOrder = 1)]
        [UpdateValueExpression("DateTime.UtcNow")]
        public DateTime UpdatedOn { get; set; }

        [DefaultValueExpression("this.CreatedBy", EvaluationOrder = 1)]
        [UpdateValueExpression("WindowsIdentity.GetCurrent().Name")]
        public string UpdatedBy { get; set; }
    }

    public class AdvancedModel : SimpleModel
    {
        [DefaultValueExpression("CurrentTicks")]
        public long Ticks { get; set; }

    }

    public class AdvancedModelContext : ValueExpressionScopeBase<AdvancedModel>
    {
        public long CurrentTicks { get; } = DateTime.UtcNow.Ticks;
    }

    [TestClass]
    public class ModelTests
    {
        private static readonly Func<SimpleModel> s_createSimpleModel;
        private static readonly Action<SimpleModel> s_applyDefaultsToSimpleModel;

        private static readonly Func<AdvancedModelContext, AdvancedModel> s_createAdvancedModel;
        private static readonly Action<AdvancedModelContext> s_applyDefaultsToAdvancedModel;

        static ModelTests()
        {
            ValueExpressionParser<SimpleModel>.InitializeType();
            ValueExpressionParser<AdvancedModel>.InitializeType();

            s_createSimpleModel = ValueExpressionParser<SimpleModel>.CreateInstance();
            s_applyDefaultsToSimpleModel = ValueExpressionParser<SimpleModel>.ApplyDefaults();

            s_createAdvancedModel = ValueExpressionParser<AdvancedModel>.CreateInstance<AdvancedModelContext>();
            s_applyDefaultsToAdvancedModel = ValueExpressionParser<AdvancedModel>.ApplyDefaults<AdvancedModelContext>();
        }

        [TestMethod]
        public void TestSimpleModel()
        {
            // Create a new model with attributes applied
            SimpleModel simple1 = s_createSimpleModel();

            Assert.IsTrue(simple1.ID != Guid.Empty);
            Assert.AreEqual(simple1.CreatedBy, Defaults.CurrentUser);
            Assert.AreEqual(simple1.UpdatedBy, simple1.CreatedBy);

            // Apply attribute defaults to an existing model
            SimpleModel simple2 = new SimpleModel();
            s_applyDefaultsToSimpleModel(simple2);

            Assert.AreEqual(simple2.CreatedBy, simple1.CreatedBy);
            Assert.AreEqual(simple2.UpdatedBy, simple2.CreatedBy);
            Assert.IsTrue((simple2.CreatedOn - simple1.CreatedOn).TotalSeconds < 1.0D);
        }

        [TestMethod]
        public void TestAdvancedModel()
        {
            AdvancedModelContext context = new AdvancedModelContext();

            // Create a new model with attributes applied
            AdvancedModel advanced1 = s_createAdvancedModel(context);

            Assert.IsTrue(advanced1.ID != Guid.Empty);
            Assert.AreEqual(advanced1.CreatedBy, Defaults.CurrentUser);
            Assert.AreEqual(advanced1.UpdatedBy, advanced1.CreatedBy);
            Assert.IsTrue(advanced1.Ticks > 0L);

            // Apply attribute defaults to an existing model
            AdvancedModel advanced2 = new AdvancedModel();

            context.Instance = advanced2;
            s_applyDefaultsToAdvancedModel(context);

            Assert.AreEqual(advanced2.CreatedBy, advanced1.CreatedBy);
            Assert.AreEqual(advanced2.UpdatedBy, advanced2.CreatedBy);
            Assert.IsTrue((advanced2.CreatedOn - advanced1.CreatedOn).TotalSeconds < 1.0D);
            Assert.IsTrue(advanced2.Ticks > 0L);
        }
    }
}
