﻿//******************************************************************************************************
//  ValueExpressionScopeBase.cs - Gbtc
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
//  04/10/2017 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using Gemstone.Expressions.Evaluator;

namespace Gemstone.Expressions.Model;

/// <summary>
/// Represent a base class used for providing contextual scope when evaluating
/// instances of the <see cref="ValueExpressionAttributeBase"/>.
/// </summary>
/// <remarks>
/// This class should be extended with public instance fields that will be automatically
/// exposed to <see cref="ValueExpressionAttributeBase"/> expressions.
/// </remarks>
/// <typeparam name="T">Type of associated model.</typeparam>
/// <remarks>
/// The <see cref="NotVisibleToExpressionAttribute"/> attribute can be used to hide fields
/// and properties from the expression that may be for internal use only.
/// </remarks>
public abstract class ValueExpressionScopeBase<T> : IValueExpressionScope<T> where T : class
{
    /// <inheritdoc />
    public T? Instance { get; set; }
}
