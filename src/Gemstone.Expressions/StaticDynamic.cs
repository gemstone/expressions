//******************************************************************************************************
//  StaticDynamic.cs - Gbtc
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
using System.Dynamic;
using System.Reflection;

namespace Gemstone.Expressions
{
    /// <summary>
    /// Defines a <see cref="DynamicObject"/> wrapper for static elements and classes.
    /// </summary>
    public class StaticDynamic : DynamicObject
    {
        private readonly Type m_type;

        /// <summary>
        /// Creates a new <see cref="StaticDynamic"/>.
        /// </summary>
        /// <param name="type">Class with static elements.</param>
        public StaticDynamic(Type type) => m_type = type;

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"></see> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"></see> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to result.</param>
        /// <returns><c>true</c> if the operation is successful; otherwise, <c>false</c>. If this method returns <c>false</c>, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            PropertyInfo? property = m_type.GetProperty(binder.Name, BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public);
            
            if (property is null)
            {
                FieldInfo? field = m_type.GetField(binder.Name, BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public);

                if (field is null)
                {
                    result = null;
                    return false;
                }

                result = field.GetValue(null);
                return true;
            }

            result = property.GetValue(null, null);
            return true;
        }

        /// <summary>
        /// Provides the implementation for operations that invoke a member. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"></see> class can override this method to specify dynamic behavior for operations such as calling a method.
        /// </summary>
        /// <param name="binder">Provides information about the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the statement sampleObject.SampleMethod(100), where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"></see> class, binder.Name returns "SampleMethod". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="args">The arguments that are passed to the object member during the invoke operation. For example, for the statement sampleObject.SampleMethod(100), where sampleObject is derived from the <see cref="T:System.Dynamic.DynamicObject"></see> class, args[0] is equal to 100.</param>
        /// <param name="result">The result of the member invocation.</param>
        /// <returns><c>true</c> if the operation is successful; otherwise, <c>false</c>. If this method returns <c>false</c>, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)</returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            MethodInfo? method = m_type.GetMethod(binder.Name, BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public);
            
            if (method is null)
            {
                result = null;
                return false;
            }

            result = method.Invoke(null, args);
            return true;
        }
    }
}
