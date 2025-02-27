﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using Grace.Utilities;

namespace Grace.DependencyInjection.Impl.Wrappers
{
    /// <summary>
    /// Strategy for creating Func with 4 args
    /// </summary>
    public class FuncFourArgWrapperStrategy : BaseWrapperStrategy
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="injectionScope"></param>
        public FuncFourArgWrapperStrategy(IInjectionScope injectionScope) : base(typeof(Func<,,,,>), injectionScope)
        {
        }
        
        /// <summary>
        /// Get the type that is being wrapped
        /// </summary>
        /// <param name="type">requested type</param>
        /// <returns>wrapped type</returns>
        public override Type GetWrappedType(Type type)
        {
            if (!type.IsConstructedGenericType)
            {
                return null;
            }

            var genericType = type.GetGenericTypeDefinition();

            return genericType == typeof(Func<,,,,>) ? type.GetTypeInfo().GenericTypeArguments[4] : null;
        }

        /// <summary>
        /// Get an activation expression for this strategy
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="request"></param>
        public override IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request)
        {
            var closedClass = typeof(FuncExpression<,,,,>).MakeGenericType(request.ActivationType.GenericTypeArguments);

            var closedMethod = closedClass.GetRuntimeMethod(nameof(FuncExpression<object,object,object,object,object>.CreateFunc), new[] { typeof(IExportLocatorScope), typeof(IDisposalScope), typeof(IInjectionContext) });

            var instance = Activator.CreateInstance(closedClass, scope, request, request.Services.InjectionContextCreator, this);

            request.RequireExportScope();
            request.RequireDisposalScope();

            var callExpression =
                Expression.Call(Expression.Constant(instance), closedMethod, request.ScopeParameter,
                    request.DisposalScopeExpression, request.InjectionContextParameter);

            return request.Services.Compiler.CreateNewResult(request, callExpression);
        }

        /// <summary>
        /// Helper class that creates Func with 4 arguements
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        public class FuncExpression<T1, T2, T3, T4, TResult>
        {
            private readonly IInjectionContextCreator _injectionContextCreator;
            private readonly string _t1Id = UniqueStringId.Generate();
            private readonly string _t2Id = UniqueStringId.Generate();
            private readonly string _t3Id = UniqueStringId.Generate();
            private readonly string _t4Id = UniqueStringId.Generate();
            private readonly ActivationStrategyDelegate _action;

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="scope"></param>
            /// <param name="request"></param>
            /// <param name="injectionContextCreator"></param>
            /// <param name="activationStrategy"></param>
            public FuncExpression(IInjectionScope scope, IActivationExpressionRequest request,
                IInjectionContextCreator injectionContextCreator, IActivationStrategy activationStrategy)
            {
                _injectionContextCreator = injectionContextCreator;

                var newRequest = request.NewRequest(typeof(TResult), activationStrategy, typeof(Func<T1,T2,T3,T4,TResult>), RequestType.Other, null, true);

                newRequest.AddKnownValueExpression(CreateKnownValueExpression(request, typeof(T1), _t1Id));
                newRequest.AddKnownValueExpression(CreateKnownValueExpression(request, typeof(T2), _t2Id));
                newRequest.AddKnownValueExpression(CreateKnownValueExpression(request, typeof(T3), _t3Id));
                newRequest.AddKnownValueExpression(CreateKnownValueExpression(request, typeof(T4), _t4Id));

                newRequest.SetLocateKey(request.LocateKey);
                newRequest.DisposalScopeExpression = request.Constants.RootDisposalScope;

                var activationExpression = request.Services.ExpressionBuilder.GetActivationExpression(scope, newRequest);

                _action = request.Services.Compiler.CompileDelegate(scope, activationExpression);
            }

            /// <summary>
            /// Method that creates new Func with 4 args
            /// </summary>
            /// <param name="scope"></param>
            /// <param name="disposalScope"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            public Func<T1, T2, T3, T4, TResult> CreateFunc(IExportLocatorScope scope, IDisposalScope disposalScope,
                IInjectionContext context)
            {
                return (arg1, arg2, arg3, arg4) =>
                {
                    var newContext = context?.Clone() ?? _injectionContextCreator.CreateContext(null);

                    newContext.SetExtraData(_t1Id, arg1);
                    newContext.SetExtraData(_t2Id, arg2);
                    newContext.SetExtraData(_t3Id, arg3);
                    newContext.SetExtraData(_t4Id, arg4);

                    return (TResult)_action(scope, disposalScope, newContext);
                };
            }
        }
    }
}
