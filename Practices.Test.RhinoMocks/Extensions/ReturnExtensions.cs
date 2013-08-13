using System;
using Rhino.Mocks.Interfaces;

namespace Practices.Test.RhinoMocks.Extensions
{
    public static class ReturnExtensions
    {
        /// <summary>
        /// Sets a delegate to be called to return a specific value as the return value for the method.
        /// </summary>
        /// <typeparam name="T">The type of the value to be returned.</typeparam>
        /// <typeparam name="TArg0">The type of the first argument.</typeparam>
        /// <param name="self"></param>
        /// <param name="func">a delegate that returns a specified value for the method.</param>
        /// <returns></returns>
        public static IMethodOptions<T> Return<T, TArg0>(this IMethodOptions<T> self, Func<TArg0, T> func)
        {
            self.WhenCalled(
                invocation =>
                {
                    var arg0 = (TArg0)invocation.Arguments[0];
                    invocation.ReturnValue = func(arg0);
                });

            return self;
        }

        /// <summary>
        /// Sets a delegate to be called to return a specific value as the return value for the method.
        /// </summary>
        /// <typeparam name="T">The type of the value to be returned.</typeparam>
        /// <typeparam name="TArg0">The type of the first argument.</typeparam>
        /// <typeparam name="TArg1">The type of the second argument.</typeparam>
        /// <param name="self"></param>
        /// <param name="func">a delegate that returns a specified value for the method.</param>
        /// <returns></returns>
        public static IMethodOptions<T> Return<T, TArg0, TArg1>(this IMethodOptions<T> self, Func<TArg0, TArg1, T> func)
        {
            self.WhenCalled(
                invocation =>
                {
                    var arg0 = (TArg0)invocation.Arguments[0];
                    var arg1 = (TArg1)invocation.Arguments[1];
                    invocation.ReturnValue = func(arg0, arg1);
                });

            return self;
        }

        /// <summary>
        /// Sets a delegate to be called to return a specific value as the return value for the method.
        /// </summary>
        /// <typeparam name="T">The type of the value to be returned.</typeparam>
        /// <typeparam name="TArg0">The type of the first argument.</typeparam>
        /// <typeparam name="TArg1">the type of the second argument.</typeparam>
        /// <typeparam name="TArg2">The type of the third argument.</typeparam>
        /// <param name="self"></param>
        /// <param name="func">a delegate that returns a specified value for the method.</param>
        /// <returns></returns>
        public static IMethodOptions<T> Return<T, TArg0, TArg1, TArg2>(this IMethodOptions<T> self,
                                                                       Func<TArg0, TArg1, TArg2, T> func)
        {
            self.WhenCalled(
                invocation =>
                {
                    var arg0 = (TArg0)invocation.Arguments[0];
                    var arg1 = (TArg1)invocation.Arguments[1];
                    var arg2 = (TArg2)invocation.Arguments[2];
                    invocation.ReturnValue = func(arg0, arg1, arg2);
                });

            return self;
        }

        /// <summary>
        /// Sets a delegate to be called to return a specific value as the return value for the method.
        /// </summary>
        /// <typeparam name="T">The type of the value to be returned.</typeparam>
        /// <typeparam name="TArg0">The type of the first argument.</typeparam>
        /// <typeparam name="TArg1">The type of the second argument.</typeparam>
        /// <typeparam name="TArg2">The type of the third argument.</typeparam>
        /// <typeparam name="TArg3">The type of the fourth argument.</typeparam>
        /// <param name="self"></param>
        /// <param name="func">a delegate that returns a specified value for the method.</param>
        /// <returns></returns>
        public static IMethodOptions<T> Return<T, TArg0, TArg1, TArg2, TArg3>(this IMethodOptions<T> self,
                                                                              Func<TArg0, TArg1, TArg2, TArg3, T> func)
        {
            self.WhenCalled(
                invocation =>
                {
                    var arg0 = (TArg0)invocation.Arguments[0];
                    var arg1 = (TArg1)invocation.Arguments[1];
                    var arg2 = (TArg2)invocation.Arguments[2];
                    var arg3 = (TArg3)invocation.Arguments[3];
                    invocation.ReturnValue = func(arg0, arg1, arg2, arg3);
                });

            return self;
        }
    }
}