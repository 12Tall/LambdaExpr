using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LambdaExpr
{
    class Program
    {
        static void Main(string[] args)
        {
            ScanExpressionTrre();
        }

        /// <summary>
        /// 简单的表达式树
        /// </summary>
        static void SimpleExpressionTree()
        {
            // 左边是表达式树 <-----> 右边是lambda 表达式
            Expression<Func<int, int>> expr = x => x + 1;
            Console.WriteLine(expr.ToString()); // x=> (x + 1)

            var lambdaExpression = expr as LambdaExpression;
            Console.WriteLine(lambdaExpression.Body);
            Console.WriteLine(lambdaExpression.ReturnType.ToString());

            foreach (var parameter in lambdaExpression.Parameters)
            {
                Console.WriteLine($"Name:{parameter.Name},Type:{parameter.Type}");
            }

            // 无法将具有语句体的lambda 表达式转化为表达式树
            // Expression<Func<int, int, int>> expr2 = (x, y) => { return x + y; };
            // Expression<Action<int>> expr3 = x => { };
            Console.ReadKey();
        }

        /// <summary>
        /// 构建简单的表达式树
        /// </summary>
        static void BuildExpressionTree()
        {
            /* 无法将具有语句体的lambda 表达式转化为表达式树
            Expression<Action> expr = () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine("Hello World!");
                }
            };*/

            LoopExpression loop = Expression.Loop(
                Expression.Call(
                    // 实例
                    null,
                    // 反射获取的方法
                    typeof(Console).GetMethod("WriteLine", new Type[] {typeof(string)}),
                    // 参数
                    Expression.Constant("Hello World!")
                )
            );

            BlockExpression block = Expression.Block(loop);

            Expression<Action> lambdaExpression = Expression.Lambda<Action>(block);
            // 编译执行
            lambdaExpression.Compile().Invoke();
            Console.ReadKey();
        }

        /// <summary>
        /// 遍历表达式树
        /// </summary>
        static void ScanExpressionTrre()
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.linq.expressions.expressiontype?view=netframework-4.8

            List<User> users = new List<User>();
            var sql = users.AsQueryable().Where(u => u.Age > 2);
            Console.WriteLine(sql);

            Console.ReadKey();
        }
    }
}