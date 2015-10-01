using System;
using System.Linq.Expressions;

namespace MeshInspector.Utils
{
    public static class Extensions
    {
        /// <summary>
        /// get the property name as string
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string GetPropertySymbol <TResult>(Expression <Func <TResult>> expr)
        {
            MemberExpression memex = (MemberExpression) expr.Body;
            return memex.Member.Name;
        }
    }
}
