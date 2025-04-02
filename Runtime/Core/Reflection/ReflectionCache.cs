using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace THEBADDEST.SimpleDependencyInjection
{
    /// <summary>
    /// Caches reflection information for improved performance
    /// </summary>
    internal static class ReflectionCache
    {
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> _constructorCache = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
        private static readonly ConcurrentDictionary<Type, MethodInfo[]> _methodCache = new();
        private static readonly ConcurrentDictionary<Type, Func<object>> _factoryCache = new();

        /// <summary>
        /// Gets the constructor for the specified type
        /// </summary>
        public static ConstructorInfo GetConstructor(Type type)
        {
            return _constructorCache.GetOrAdd(type, t =>
            {
                var constructors = t.GetConstructors();
                if (constructors.Length == 0)
                    throw new InvalidOperationException($"No public constructor found for type {t}");

                // Prefer constructor with most parameters
                return constructors.OrderByDescending(c => c.GetParameters().Length).First();
            });
        }

        /// <summary>
        /// Gets the injectable properties for the specified type
        /// </summary>
        public static PropertyInfo[] GetInjectableProperties(Type type)
        {
            return _propertyCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetCustomAttribute<InjectAttribute>() != null)
                    .ToArray());
        }

        /// <summary>
        /// Gets the injectable methods for the specified type
        /// </summary>
        public static MethodInfo[] GetInjectableMethods(Type type)
        {
            return _methodCache.GetOrAdd(type, t =>
                t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttribute<InjectAttribute>() != null)
                    .ToArray());
        }

        /// <summary>
        /// Gets or creates a factory function for the specified type
        /// </summary>
        public static Func<object> GetFactory(Type type)
        {
            return _factoryCache.GetOrAdd(type, t =>
            {
                var constructor = GetConstructor(t);
                var parameters = constructor.GetParameters();

                if (parameters.Length == 0)
                    return Expression.Lambda<Func<object>>(Expression.New(constructor)).Compile();

                // Create a factory that throws an exception if called
                // This will be replaced with a proper factory when dependencies are available
                return () => throw new InvalidOperationException($"Cannot create instance of {t} without dependencies");
            });
        }

        /// <summary>
        /// Creates a factory function for the specified type with the given dependencies
        /// </summary>
        public static Func<object> CreateFactory(Type type, Func<Type, object> dependencyResolver)
        {
            var constructor = GetConstructor(type);
            var parameters = constructor.GetParameters();

            if (parameters.Length == 0)
                return Expression.Lambda<Func<object>>(Expression.New(constructor)).Compile();

            // Create parameter expressions
            var parameterExpressions = new List<ParameterExpression>();
            var argumentExpressions = new List<Expression>();

            foreach (var parameter in parameters)
            {
                var paramExpr = Expression.Parameter(parameter.ParameterType, parameter.Name);
                parameterExpressions.Add(paramExpr);

                var resolveCall = Expression.Call(
                    Expression.Constant(dependencyResolver.Target),
                    dependencyResolver.Method,
                    Expression.Constant(parameter.ParameterType)
                );
                argumentExpressions.Add(Expression.Convert(resolveCall, parameter.ParameterType));
            }

            // Create new expression
            var newExpr = Expression.New(constructor, argumentExpressions);
            var convertExpr = Expression.Convert(newExpr, typeof(object));

            // Create lambda
            var lambda = Expression.Lambda<Func<object>>(convertExpr);
            return lambda.Compile();
        }

        /// <summary>
        /// Clears all caches
        /// </summary>
        public static void Clear()
        {
            _constructorCache.Clear();
            _propertyCache.Clear();
            _methodCache.Clear();
            _factoryCache.Clear();
        }
    }
}