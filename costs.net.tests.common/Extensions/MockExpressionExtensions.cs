namespace costs.net.tests.common.Extensions
{
    using System;
    using System.Linq.Expressions;

    public static class MockExpressionExtensions
    {
        public static bool EqualsExpressionWithConstantValue<T, TV>(this Expression<Func<T, bool>> expression, TV value)
            where TV : IEquatable<TV>
        {
            if (!(expression.Body is BinaryExpression))
            {
                throw new ArgumentException("Only binary expression with constant value is supported");
            }

            var body = (BinaryExpression) expression.Body;
            var rightExpression = body.Right as MemberExpression;
            var leftExpression = body.Right as MemberExpression;
            MemberExpression memberExpression = null;
            if (rightExpression?.Expression is ConstantExpression)
            {
                memberExpression = rightExpression;
            }
            else if (leftExpression?.Expression is ConstantExpression)
            {
                memberExpression = leftExpression;
            }
            return (memberExpression != null) && GetConstantValue<TV>(memberExpression).Equals(value);
        }

        private static T GetConstantValue<T>(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return (T) getter();
        }
    }
}