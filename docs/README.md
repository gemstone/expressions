<img align="right" src="img/gemstone-wide-600.png" alt="gemstone logo">

# Expressions
### GPA Gemstone Library

The Gemstone Expressions Library organizes all Gemstone functionality related to expressions and expression parsing.

[![GitHub license](https://img.shields.io/github/license/gemstone/expressions?color=4CC61E)](https://github.com/gemstone/expressions/blob/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/5p90y9pujit72lfl?svg=true)](https://ci.appveyor.com/project/ritchiecarroll/expressions)

This library includes helpful expression related classes like the following:

* [ExpressionCompiler](https://gemstone.github.io/expressions/help/html/T_gemstone_expressions_evaluator_ExpressionCompiler.htm):
  * Represents a [Roslyn](https://github.com/dotnet/roslyn) based runtime C# expression evaluator with the ability to specify types and expression accessible variables using a [TypeRegistry](https://gemstone.github.io/expressions/help/html/T_gemstone_expressions_evaluator_TypeRegistry.htm) and an instance parameter type. Provides a [Linq Expression](https://gemstone.github.io/expressions/help/html/P_gemstone_expressions_evaluator_ExpressionCompiler_2_CompiledExpression.htm) as well as [Action](https://gemstone.github.io/expressions/help/html/P_gemstone_expressions_evaluator_ExpressionCompiler_2_CompiledAction.htm) or [Func](https://gemstone.github.io/expressions/help/html/P_gemstone_expressions_evaluator_ExpressionCompiler_2_CompiledFunction.htm) delegates for compiled expression.
* [RuntimeCompiler](https://gemstone.github.io/expressions/help/html/T_gemstone_expressions_RuntimeCompiler.htm):
  * Represents a [Roslyn](https://github.com/dotnet/roslyn) based runtime C# code compiler (emits assembly bytes).
* [StaticDynamic](https://gemstone.github.io/expressions/help/html/T_gemstone_expressions_StaticDynamic.htm):
  * Defines a [DynamicObject](https://docs.microsoft.com/en-us/dotnet/api/system.dynamic.dynamicobject) wrapper for static elements and classes.

Among others.

### Documentation
[Full Library Documentation](https://gemstone.github.io/expressions/help)