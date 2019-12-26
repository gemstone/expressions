﻿//******************************************************************************************************
//  UpdateValueExpressionAttribute.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  04/16/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace Gemstone.Expressions.Model
{
    /// <summary>
    /// Defines a C# expression attribute that when evaluated will specify an updated value for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class UpdateValueExpressionAttribute : ValueExpressionAttributeBase
    {
        /// <summary>
        /// Creates a new <see cref="UpdateValueExpressionAttribute"/>
        /// </summary>
        /// <param name="expression">C# expression that will evaluate to the desired default value.</param>
        public UpdateValueExpressionAttribute(string expression) : base(expression)
        {
        }
    }
}