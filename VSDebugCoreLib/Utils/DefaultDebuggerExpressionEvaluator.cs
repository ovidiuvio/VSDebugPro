using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;

namespace VSDebugCoreLib.Utils
{
    public class DefaultDebuggerExpressionEvaluator : DebuggerExpressionEvaluator
    {
        public DefaultDebuggerExpressionEvaluator(DTE2 dte) : base(dte)
        {
        }

        protected override string FullyExpandExpressionJson(Expression rootExpr)
        {
            throw new NotSupportedException("JSON serialization is not supported for the default evaluator.");
        }

        protected override bool IsPrimitiveType(Expression expr)
        {
            // Implement logic to determine if the expression is a primitive type
            // For simplicity, let's assume all expressions are non-primitive in this default evaluator
            return false;
        }

        protected override bool IsCollection(Expression expr)
        {
            // Implement logic to determine if the expression is a collection
            // For simplicity, let's assume no expressions are collections in this default evaluator
            return false;
        }

        protected override bool IsDictionary(Expression expr)
        {
            // Implement logic to determine if the expression is a dictionary
            // For simplicity, let's assume no expressions are dictionaries in this default evaluator
            return false;
        }
    }
}
