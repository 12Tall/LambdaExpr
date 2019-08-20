using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LambdaExpr
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class QueryableExtensions
    {
        public static string Where<TSource>(this IQueryable<TSource> source,
            // lambda 表达式
            Expression<Func<TSource, bool>> predicate)
        {
            var expression = Expression.Call(
                null, 
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(new Type[] { typeof(TSource) }),
                new Expression[] { source.Expression, Expression.Quote(predicate) });

            var translator = new QueryTranslator();
            return translator.Translate(expression);
        }
    }
}