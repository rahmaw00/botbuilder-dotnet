﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Comparison operators.
    /// A comparison operator returns false if the comparison is false, or there is an error.  This prevents errors from short-circuiting boolean expressions.
    /// </summary>
    public class ComparisonEvaluator : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonEvaluator"/> class.
        /// </summary>
        /// <param name="type">Name of the built-in function.</param>
        /// <param name="function">The comparison function, it takes a list of objects and returns a boolean.</param>
        /// <param name="validator">Validator of input arguments.</param>
        /// <param name="verify">Optional function to verify each child's result.</param>
        public ComparisonEvaluator(string type, Func<IReadOnlyList<object>, bool> function, ValidateExpressionDelegate validator, FunctionUtils.VerifyExpression verify = null)
            : base(type, Evaluator(function, verify), ReturnType.Boolean, validator)
        {
        }

        private enum DataType
        {
            Number,
            Comparable,
            Other
        }

        private static EvaluateExpressionDelegate Evaluator(Func<IReadOnlyList<object>, bool> function, FunctionUtils.VerifyExpression verify)
        {
            return (expression, state, options) =>
            {
                var result = false;
                string error = null;
                IReadOnlyList<object> args;
                (args, error) = FunctionUtils.EvaluateChildren(expression, state, new Options(options) { NullSubstitution = null }, verify);
                if (error == null)
                {
                    try
                    {
                        result = function(args);
                    }
#pragma warning disable CA1031 // Do not catch general exception types (we are capturing the exception and returning it)
                    catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        // NOTE: This should not happen in normal execution
                        error = e.Message;
                    }
                }
                else
                {
                    // Swallow errors and treat as false
                    error = null;
                }

                return (result, error);
            };
        }

        private static DataType DetermineCurArgType(object obj)
        {
            if (obj.IsNumber())
            {
                return DataType.Number;
            }

            if (obj is IComparable comparer)
            {
                return DataType.Comparable;
            }

            return DataType.Other;
        }
    }
}
