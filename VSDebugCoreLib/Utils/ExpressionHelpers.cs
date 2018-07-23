using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSDebugCoreLib.Utils
{
    public class ExpressionTypeHelpers
    {
        static public bool isArray(string expr)
        {
            return expr.EndsWith("]");
        }

        static public bool isPointer(string expr)
        {
            return expr.EndsWith("*");
        }

        static public bool isReference(string expr)
        {
            return expr.EndsWith("&");
        }

        static public string GetType(string expr)
        {
            if (isArray(expr))
                return expr.Substring(0, expr.IndexOf('['));
            else if (isPointer(expr) || isReference(expr))
                return expr.TrimEnd("&*".ToCharArray()).Trim(' ');

            return expr;
        }

        static public string GetTypeMember(string expr, string exprType, string strMember)
        {
            string strExprMember = "";
            string strAccessor = ".";

            if (isPointer(exprType))
                strAccessor = "->";

            if (isArray(exprType))
                strExprMember = "(("+GetType(exprType)+"*)" + expr + ")" + "->" + strMember;
            else
                strExprMember = expr + strAccessor + strMember;

            return strExprMember;
        }
    }
}
