/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
#if CODEPLEX_40
using System.Dynamic;
using System.Dynamic.Utils;
#else
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
#endif
using System.Runtime.CompilerServices;
#if !CODEPLEX_40
using Microsoft.Runtime.CompilerServices;
#endif


#if CODEPLEX_40
namespace System.Linq.Expressions.Compiler {
#else
namespace Microsoft.Linq.Expressions.Compiler {
#endif
    internal static partial class DelegateHelpers {
        private static TypeInfo _DelegateCache = new TypeInfo();

        #region Generated Maximum Delegate Arity

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_max_delegate_arity from: generate_dynsites.py

        private const int MaximumArity = 17;

        // *** END GENERATED CODE ***

        #endregion

        internal class TypeInfo {
            public Type DelegateType;
            public Dictionary<Type, TypeInfo> TypeChain;

            public Type MakeDelegateType(Type retType, params Expression[] args) {
                return MakeDelegateType(retType, (IList<Expression>)args);
            }

            public Type MakeDelegateType(Type retType, IList<Expression> args) {
                // nope, go ahead and create it and spend the
                // cost of creating the array.
                Type[] paramTypes = new Type[args.Count + 2];
                paramTypes[0] = typeof(CallSite);
                paramTypes[paramTypes.Length - 1] = retType;
                for (int i = 0; i < args.Count; i++) {
                    paramTypes[i + 1] = args[i].Type;
                }

                return DelegateType = MakeNewDelegate(paramTypes);
            }
        }


        /// <summary>
        /// Finds a delegate type using the types in the array. 
        /// We use the cache to avoid copying the array, and to cache the
        /// created delegate type
        /// </summary>
        internal static Type MakeDelegateType(Type[] types) {
            lock (_DelegateCache) {
                TypeInfo curTypeInfo = _DelegateCache;

                // arguments & return type
                for (int i = 0; i < types.Length; i++) {
                    curTypeInfo = NextTypeInfo(types[i], curTypeInfo);
                }

                // see if we have the delegate already
                if (curTypeInfo.DelegateType == null) {
                    // clone because MakeCustomDelegate can hold onto the array.
                    curTypeInfo.DelegateType = MakeNewDelegate((Type[])types.Clone());
                }

                return curTypeInfo.DelegateType;
            }
        }

        /// <summary>
        /// Finds a delegate type for a CallSite using the types in the ReadOnlyCollection of Expression. 
        /// 
        /// We take the readonly collection of Expression explicitly to avoid allocating memory (an array 
        /// of types) on lookup of delegate types.
        /// </summary>
        internal static Type MakeCallSiteDelegate(ReadOnlyCollection<Expression> types, Type returnType) {
            lock (_DelegateCache) {
                TypeInfo curTypeInfo = _DelegateCache;

                // CallSite
                curTypeInfo = NextTypeInfo(typeof(CallSite), curTypeInfo);

                // arguments
                for (int i = 0; i < types.Count; i++) {
                    curTypeInfo = NextTypeInfo(types[i].Type, curTypeInfo);
                }

                // return type
                curTypeInfo = NextTypeInfo(returnType, curTypeInfo);

                // see if we have the delegate already
                if (curTypeInfo.DelegateType == null) {
                    curTypeInfo.MakeDelegateType(returnType, types);
                }

                return curTypeInfo.DelegateType;
            }
        }

        /// <summary>
        /// Finds a delegate type for a CallSite using the MetaObject array. 
        /// 
        /// We take the array of MetaObject explicitly to avoid allocating memory (an array of types) on
        /// lookup of delegate types.
        /// </summary>
        internal static Type MakeDeferredSiteDelegate(DynamicMetaObject[] args, Type returnType) {
            lock (_DelegateCache) {
                TypeInfo curTypeInfo = _DelegateCache;

                // CallSite
                curTypeInfo = NextTypeInfo(typeof(CallSite), curTypeInfo);

                // arguments
                for (int i = 0; i < args.Length; i++) {
                    DynamicMetaObject mo = args[i];
                    Type paramType = mo.Expression.Type;
                    if (IsByRef(mo)) {
                        paramType = paramType.MakeByRefType();
                    }
                    curTypeInfo = NextTypeInfo(paramType, curTypeInfo);
                }

                // return type
                curTypeInfo = NextTypeInfo(returnType, curTypeInfo);

                // see if we have the delegate already
                if (curTypeInfo.DelegateType == null) {
                    // nope, go ahead and create it and spend the
                    // cost of creating the array.
                    Type[] paramTypes = new Type[args.Length + 2];
                    paramTypes[0] = typeof(CallSite);
                    paramTypes[paramTypes.Length - 1] = returnType;
                    for (int i = 0; i < args.Length; i++) {
                        DynamicMetaObject mo = args[i];
                        Type paramType = mo.Expression.Type;
                        if (IsByRef(mo)) {
                            paramType = paramType.MakeByRefType();
                        }
                        paramTypes[i + 1] = paramType;
                    }

                    curTypeInfo.DelegateType = MakeNewDelegate(paramTypes);
                }

                return curTypeInfo.DelegateType;
            }
        }

        private static bool IsByRef(DynamicMetaObject mo) {
            ParameterExpression pe = mo.Expression as ParameterExpression;
            return pe != null && pe.IsByRef;
        }

        internal static TypeInfo NextTypeInfo(Type initialArg) {
            lock (_DelegateCache) {
                return NextTypeInfo(initialArg, _DelegateCache);
            }
        }

        internal static TypeInfo GetNextTypeInfo(Type initialArg, TypeInfo curTypeInfo) {
            lock (_DelegateCache) {
                return NextTypeInfo(initialArg, curTypeInfo);
            }
        }

        private static TypeInfo NextTypeInfo(Type initialArg, TypeInfo curTypeInfo) {
            Type lookingUp = initialArg;
            TypeInfo nextTypeInfo;
            if (curTypeInfo.TypeChain == null) {
                curTypeInfo.TypeChain = new Dictionary<Type, TypeInfo>();
            }

            if (!curTypeInfo.TypeChain.TryGetValue(lookingUp, out nextTypeInfo)) {
                curTypeInfo.TypeChain[lookingUp] = nextTypeInfo = new TypeInfo();
            }
            return nextTypeInfo;
        }

        /// <summary>
        /// Creates a new delegate, or uses a func/action
        /// Note: this method does not cache
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static Type MakeNewDelegate(Type[] types) {
            Debug.Assert(types != null && types.Length > 0);

            // Can only used predefined delegates if we have no byref types and
            // the arity is small enough to fit in Func<...> or Action<...>
            if (types.Length > MaximumArity || types.Any(t => t.IsByRef)) {
                return MakeNewCustomDelegate(types);
            }

            Type result;
            if (types[types.Length - 1] == typeof(void)) {
                result = GetActionType(types.RemoveLast());
            } else {
                result = GetFuncType(types);
            }
            Debug.Assert(result != null);
            return result;
        }

        internal static Type GetFuncType(Type[] types) {
            switch (types.Length) {
                #region Generated Delegate Func Types

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_delegate_func from: generate_dynsites.py

                case 1: return typeof(Func<>).MakeGenericType(types);
                case 2: return typeof(Func<,>).MakeGenericType(types);
                case 3: return typeof(Func<,,>).MakeGenericType(types);
                case 4: return typeof(Func<,,,>).MakeGenericType(types);
                case 5: return typeof(Func<,,,,>).MakeGenericType(types);
                case 6: return typeof(Func<,,,,,>).MakeGenericType(types);
                case 7: return typeof(Func<,,,,,,>).MakeGenericType(types);
                case 8: return typeof(Func<,,,,,,,>).MakeGenericType(types);
                case 9: return typeof(Func<,,,,,,,,>).MakeGenericType(types);
                case 10: return typeof(Func<,,,,,,,,,>).MakeGenericType(types);
                case 11: return typeof(Func<,,,,,,,,,,>).MakeGenericType(types);
                case 12: return typeof(Func<,,,,,,,,,,,>).MakeGenericType(types);
                case 13: return typeof(Func<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14: return typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15: return typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16: return typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 17: return typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(types);

                // *** END GENERATED CODE ***

                #endregion

                default: return null;
            }
        }

        internal static Type GetActionType(Type[] types) {
            switch (types.Length) {
                case 0: return typeof(Action);
                #region Generated Delegate Action Types

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_delegate_action from: generate_dynsites.py

                case 1: return typeof(Action<>).MakeGenericType(types);
                case 2: return typeof(Action<,>).MakeGenericType(types);
                case 3: return typeof(Action<,,>).MakeGenericType(types);
                case 4: return typeof(Action<,,,>).MakeGenericType(types);
                case 5: return typeof(Action<,,,,>).MakeGenericType(types);
                case 6: return typeof(Action<,,,,,>).MakeGenericType(types);
                case 7: return typeof(Action<,,,,,,>).MakeGenericType(types);
                case 8: return typeof(Action<,,,,,,,>).MakeGenericType(types);
                case 9: return typeof(Action<,,,,,,,,>).MakeGenericType(types);
                case 10: return typeof(Action<,,,,,,,,,>).MakeGenericType(types);
                case 11: return typeof(Action<,,,,,,,,,,>).MakeGenericType(types);
                case 12: return typeof(Action<,,,,,,,,,,,>).MakeGenericType(types);
                case 13: return typeof(Action<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14: return typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15: return typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16: return typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(types);

                // *** END GENERATED CODE ***

                #endregion

                default: return null;
            }
        }
    }
}
