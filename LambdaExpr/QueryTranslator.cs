using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LambdaExpr
{
    public class QueryTranslator : ExpressionVisitor
    {
        private StringBuilder sb;

        internal string Translate(Expression expression)
        {
            this.sb = new StringBuilder();
            this.Visit(expression);
            return this.sb.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            sb.Append("(");
            this.Visit(node.Left);
            switch (node.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.Or:
                    sb.Append(" OR");
                    break;
                case ExpressionType.Equal:
                    sb.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException($"二元运算符{node.NodeType}不支持");
            }

            this.Visit(node.Right);
            sb.Append(")");
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return base.VisitMethodCall(node);
            if (node.Method.DeclaringType == typeof(QueryableExtensions) && node.Method.Name == "Where")
            {
                sb.Append("select * from (");
                this.Visit(node.Arguments[0]);
                sb.Append(") as T where ");
                LambdaExpression lambda = (LambdaExpression) StripQuotes(node.Arguments[1]);
                this.Visit(lambda.Body);
                return node;
            }

            throw new NotSupportedException($"方法{node.Method.Name}不支持");
        }
        public Expression StripQuotes(Expression e)
        {
            //如果为参数应用表达式
            while (e.NodeType == ExpressionType.Quote)
            {
                //将其转为一元表达式即可获取真正的值
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {
                // 我们假设我们那个Queryable就是对应的表
                sb.Append("SELECT * FROM ");
                sb.Append(q.ElementType.Name);
            }
            else if (c.Value == null)
            {
                sb.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        break;
                    case TypeCode.String:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException($"The constant for '{c.Value}' is not supported");
                    default:
                        sb.Append(c.Value);
                        break;
                }
            }
            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                sb.Append(m.Member.Name);
                return m;
            }
            throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
        }
    }
}