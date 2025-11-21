using System.Linq.Expressions;

namespace PlainBytes.LiteDB
{
    /// <summary>
    /// Class used to test in an Expression member expression is based on parameter `x => x.Name` or variable `x => externalVar`
    /// </summary>
    internal class ParameterExpressionVisitor : ExpressionVisitor
    {
        private bool IsParameter { get; set; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            IsParameter = true;

            return base.VisitParameter(node);
        }

        public static bool Test(Expression node)
        {
            var instance = new ParameterExpressionVisitor();

            instance.Visit(node);

            return instance.IsParameter;
        }
    }
}