using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Succintly.Common
{
	public static class QueryableExtensions
	{
		#region ThenBy
		public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, String path)
		{
			Type currentType = typeof(T);
			String[] parts = path.Split(',');

			foreach (String part in parts)
			{
				PropertyInfo propInfo = currentType.GetProperty(part);
				Type propType = propInfo.PropertyType;
				String propFetchFunctionName = "ThenBy";
				Type delegateType = typeof(Func<,>).MakeGenericType(currentType, propType);

				ParameterExpression exprParam = Expression.Parameter(currentType, "it");
				MemberExpression exprProp = Expression.Property(exprParam, part);
				LambdaExpression exprLambda = Expression.Lambda(delegateType, exprProp, new ParameterExpression[] { exprParam });

				Type orderExtensionMethodsType = typeof(Queryable);

				List<Type> fetchMethodTypes = new List<Type>();
				fetchMethodTypes.AddRange(query.GetType().GetGenericArguments().Take(2));
				fetchMethodTypes.Add(propType);

				MethodInfo fetchMethodInfo = orderExtensionMethodsType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).Where(x => x.Name == propFetchFunctionName && x.GetParameters().Length == 2).Single();
				fetchMethodInfo = fetchMethodInfo.MakeGenericMethod(fetchMethodTypes.ToArray());

				Object[] args = new Object[] { query, exprLambda };

				query = fetchMethodInfo.Invoke(null, args) as IOrderedQueryable<T>;

				currentType = propType;
			}

			return (query);
		}

		public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> query, String path)
		{
			Type currentType = typeof(T);
			String[] parts = path.Split(',');

			foreach (String part in parts)
			{
				PropertyInfo propInfo = currentType.GetProperty(part);
				Type propType = propInfo.PropertyType;
				String propFetchFunctionName = "ThenByDescending";
				Type delegateType = typeof(Func<,>).MakeGenericType(currentType, propType);

				ParameterExpression exprParam = Expression.Parameter(currentType, "it");
				MemberExpression exprProp = Expression.Property(exprParam, part);
				LambdaExpression exprLambda = Expression.Lambda(delegateType, exprProp, new ParameterExpression[] { exprParam });

				Type orderExtensionMethodsType = typeof(Queryable);

				List<Type> fetchMethodTypes = new List<Type>();
				fetchMethodTypes.AddRange(query.GetType().GetGenericArguments().Take(2));
				fetchMethodTypes.Add(propType);

				MethodInfo fetchMethodInfo = orderExtensionMethodsType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).Where(x => x.Name == propFetchFunctionName && x.GetParameters().Length == 2).Single();
				fetchMethodInfo = fetchMethodInfo.MakeGenericMethod(fetchMethodTypes.ToArray());

				Object[] args = new Object[] { query, exprLambda };

				query = fetchMethodInfo.Invoke(null, args) as IOrderedQueryable<T>;

				currentType = propType;
			}

			return (query);
		}
		#endregion

		#region OrderBy
		public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> query, String propertyName, Boolean ascending = true)
		{
			Type type = typeof(TSource);
			ParameterExpression parameter = Expression.Parameter(type, "p");
			MemberExpression propertyReference = Expression.Property(parameter, propertyName);
			MethodCallExpression sortExpression = Expression.Call(typeof(Queryable), (ascending == true ? "OrderBy" : "OrderByDescending"), new Type[] { type }, null, Expression.Lambda<Func<TSource, Boolean>>(propertyReference, new ParameterExpression[] { parameter }));
			return (query.Provider.CreateQuery<TSource>(sortExpression) as IOrderedQueryable<TSource>);
		}

		public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, String path)
		{
			Type currentType = typeof(T);
			String[] parts = path.Split(',');
			Boolean isFirstFetch = true;

			foreach (String part in parts)
			{
				PropertyInfo propInfo = currentType.GetProperty(part);
				Type propType = propInfo.PropertyType;
				String propFetchFunctionName = (isFirstFetch ? "OrderBy" : "ThenBy");
				Type delegateType = typeof(Func<,>).MakeGenericType(currentType, propType);

				ParameterExpression exprParam = Expression.Parameter(currentType, "it");
				MemberExpression exprProp = Expression.Property(exprParam, part);
				LambdaExpression exprLambda = Expression.Lambda(delegateType, exprProp, new ParameterExpression[] { exprParam });

				Type orderExtensionMethodsType = typeof(Queryable);

				List<Type> fetchMethodTypes = new List<Type>();
				fetchMethodTypes.AddRange(query.GetType().GetGenericArguments().Take(isFirstFetch ? 1 : 2));
				fetchMethodTypes.Add(propType);

				MethodInfo fetchMethodInfo = orderExtensionMethodsType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).Where(x => x.Name == propFetchFunctionName && x.GetParameters().Length == 2).Single();
				fetchMethodInfo = fetchMethodInfo.MakeGenericMethod(fetchMethodTypes.ToArray());

				Object[] args = new Object[] { query, exprLambda };

				query = fetchMethodInfo.Invoke(null, args) as IOrderedQueryable<T>;

				currentType = propType;

				isFirstFetch = false;
			}

			return (query as IOrderedQueryable<T>);
		}

		public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, String path)
		{
			Type currentType = typeof(T);
			String[] parts = path.Split(',');
			Boolean isFirstFetch = true;

			foreach (String part in parts)
			{
				PropertyInfo propInfo = currentType.GetProperty(part);
				Type propType = propInfo.PropertyType;
				String propFetchFunctionName = (isFirstFetch ? "OrderByDescending" : "ThenByDescending");
				Type delegateType = typeof(Func<,>).MakeGenericType(currentType, propType);

				ParameterExpression exprParam = Expression.Parameter(currentType, "it");
				MemberExpression exprProp = Expression.Property(exprParam, part);
				LambdaExpression exprLambda = Expression.Lambda(delegateType, exprProp, new ParameterExpression[] { exprParam });

				Type orderExtensionMethodsType = typeof(Queryable);

				List<Type> fetchMethodTypes = new List<Type>();
				fetchMethodTypes.AddRange(query.GetType().GetGenericArguments().Take(isFirstFetch ? 1 : 2));
				fetchMethodTypes.Add(propType);

				MethodInfo fetchMethodInfo = orderExtensionMethodsType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).Where(x => x.Name == propFetchFunctionName && x.GetParameters().Length == 2).Single();
				fetchMethodInfo = fetchMethodInfo.MakeGenericMethod(fetchMethodTypes.ToArray());

				Object[] args = new Object[] { query, exprLambda };

				query = fetchMethodInfo.Invoke(null, args) as IOrderedQueryable<T>;

				currentType = propType;

				isFirstFetch = false;
			}

			return (query as IOrderedQueryable<T>);
		}
		#endregion

		#region Between
		public static IQueryable<TSource> Between<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, TKey low, TKey high) where TKey : IComparable<TKey>
		{
			// Get a ParameterExpression node of the TSource that is used in the expression tree 
			ParameterExpression sourceParameter = Expression.Parameter(typeof(TSource));

			// Get the body and parameter of the lambda expression 
			Expression body = keySelector.Body;
			ParameterExpression parameter = null;

			if (keySelector.Parameters.Count > 0)
			{
				parameter = keySelector.Parameters[0];
			}

			// Get the Compare method of the type of the return value 
			MethodInfo compareMethod = typeof(TKey).GetMethod("CompareTo", new Type[] { typeof(TKey) });

			ConstantExpression zero = Expression.Constant(0, typeof(Int32));

			// Expression.LessThanOrEqual and Expression.GreaterThanOrEqual method are only used in 
			// the numeric comparision. If we want to compare the non-numeric type, we can't directly  
			// use the two methods.  
			// So we first use the Compare method to compare the objects, and the Compare method  
			// will return a int number. Then we can use the LessThanOrEqual and GreaterThanOrEqua method. 
			// For this reason, we ask all the TKey type implement the IComparable<> interface. 
			Expression upper = Expression.LessThanOrEqual(Expression.Call(body, compareMethod, Expression.Constant(high)), zero);
			Expression lower = Expression.GreaterThanOrEqual(Expression.Call(body, compareMethod, Expression.Constant(low)), zero);

			Expression andExpression = Expression.AndAlso(upper, lower);

			// Get the Where method expression. 
			MethodCallExpression whereCallExpression = Expression.Call
			(
				typeof(Queryable),
				"Where",
				new Type[] { source.ElementType },
				source.Expression,
				Expression.Lambda<Func<TSource, Boolean>>(andExpression, new ParameterExpression[] { parameter })
			);

			return (source.Provider.CreateQuery<TSource>(whereCallExpression));
		}
		#endregion
	}
}
