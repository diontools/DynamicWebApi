using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi.Client
{
    public class AccessorFactory
    {
        public static TInterface Create<TInterface>(bool useWeb, Action<WebApiBase> initializeForWeb, bool outputAssembly = false)
            where TInterface : class
        {
            if (!typeof(TInterface).IsInterface) throw new ArgumentException("type '" + typeof(TInterface).FullName + "' is not interface.", nameof(TInterface));
            
            if (!useWeb)
            {
                return GenerateLocalClient<TInterface>(outputAssembly);
            }

            var instance = GenerateHttpClient<TInterface>(outputAssembly);
            var api = (WebApiBase)(object)instance;
            initializeForWeb(api);

            return instance;
        }

        private static TInterface GenerateLocalClient<TInterface>(bool outputAssembly)
        {
            var accessModelAttr = typeof(TInterface).GetCustomAttribute<AccessModelAttribute>(false);
            if (accessModelAttr == null) throw new InvalidOperationException("AccessModelAttribute not found.");

            var localApiBaseType = typeof(LocalApiBase<>).MakeGenericType(accessModelAttr.ImplementType);

            var name = typeof(TInterface).Name;
            if (name.StartsWith("I")) name = name.Remove(0, 1);

            var asmName = name + "LocalImpl";
            var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(asmName), AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = asmBuilder.DefineDynamicModule("MainModule", asmName + ".dll");
            var typeBuilder = moduleBuilder.DefineType(
                asmName,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit,
                localApiBaseType);
            
            typeBuilder.AddInterfaceImplementation(typeof(TInterface));

            var instanceField = localApiBaseType.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance);

            var methods = typeof(TInterface).GetMethods();
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
                var argsParameters = parameters.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
                var staticParameterTypes = new[] { typeof(HttpClient) }.Concat(parameterTypes).ToArray();
                var staticMethod = typeBuilder.DefineMethod(method.Name + "Static", MethodAttributes.Static, method.ReturnType, staticParameterTypes);

                var instanceParameter = Expression.Parameter(typeof(TInterface), "instance");
                var staticParameters = (new[] { instanceParameter }).Concat(argsParameters).ToArray();

                if (method.ReturnType.IsGenericType && typeof(Task<>) == method.ReturnType.GetGenericTypeDefinition())
                {
                    // return Task.Run(() => instance.MethodName(args).Result);

                    var genArgs = method.ReturnType.GetGenericArguments();

                    var funcLambda =
                        Expression.Lambda(
                            Expression.Property(
                                Expression.Call(
                                    instanceParameter,
                                    method,
                                    argsParameters),
                            "Result"));

                    var lambda =
                        Expression.Lambda(
                            Expression.Call(
                                typeof(Task),
                                "Run",
                                genArgs,
                                funcLambda),
                            staticParameters);

                    lambda.CompileToMethod(staticMethod);
                }
                else
                {
                    var lambda =
                        Expression.Lambda(
                            Expression.Call(
                                instanceParameter,
                                method,
                                argsParameters),
                            staticParameters);

                    lambda.CompileToMethod(staticMethod);
                }

                var virtualMethod = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType, parameterTypes);
                ImplementInvokeMethod(virtualMethod, staticMethod, instanceField, parameters);

                typeBuilder.DefineMethodOverride(virtualMethod, method);
            }

            var genType = typeBuilder.CreateType();
            var instance = Activator.CreateInstance(genType);

            if (outputAssembly)
            {
                asmBuilder.Save(asmName + ".dll");
            }

            return (TInterface)instance;
        }

        private static T GenerateHttpClient<T>(bool outputAssembly)
        {
            var accessModelAttr = typeof(T).GetCustomAttribute<AccessModelAttribute>(false);
            if (accessModelAttr == null) throw new InvalidOperationException("AccessModelAttribute not found.");

            var name = typeof(T).Name;
            if (name.StartsWith("I")) name = name.Remove(0, 1);

            var asmName = name + "WebImpl";
            var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(asmName), AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = asmBuilder.DefineDynamicModule("MainModule", asmName + ".dll");
            var typeBuilder = moduleBuilder.DefineType(
                asmName,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit,
                typeof(WebApiBase));

            typeBuilder.AddInterfaceImplementation(typeof(T));
            
            var methods = typeof(T).GetMethods();
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
                var staticParameterTypes = new[] { typeof(HttpClient) }.Concat(parameterTypes).ToArray();
                var staticMethod = typeBuilder.DefineMethod(method.Name + "Static", MethodAttributes.Static, method.ReturnType, staticParameterTypes);

                var argsType = TypeGenUtility.DefineArgsType(moduleBuilder, method, parameters);
                var argsTypeParameters = parameters.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();

                var instanceParameter = Expression.Parameter(typeof(WebApiBase), "instance");
                var urlParameter = Expression.Constant(accessModelAttr.Name + "/" + method.Name);
                var argsNew = Expression.New(argsType.GetConstructor(parameterTypes), argsTypeParameters);
                var staticParameters = new[] { instanceParameter }.Concat(argsTypeParameters).ToArray();

                if (method.ReturnType.IsGenericType && typeof(Task<>) == method.ReturnType.GetGenericTypeDefinition())
                {
                    var lambda =
                        Expression.Lambda(
                            Expression.Call(
                                typeof(WebApiBase),
                                "PostAsync",
                                new Type[] { argsType, method.ReturnType.GetGenericArguments().First() },
                                instanceParameter, urlParameter, argsNew),
                            staticParameters);

                    lambda.CompileToMethod(staticMethod);
                }
                else
                {
                    var lambda =
                        Expression.Lambda(
                            Expression.Call(
                                typeof(WebApiBase),
                                "Post",
                                new Type[] { argsType, method.ReturnType },
                                instanceParameter, urlParameter, argsNew),
                            staticParameters);

                    lambda.CompileToMethod(staticMethod);
                }

                var virtualMethod = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType, parameterTypes);
                ImplementInvokeMethod(virtualMethod, staticMethod, parameters);

                typeBuilder.DefineMethodOverride(virtualMethod, method);
            }

            var genType = typeBuilder.CreateType();
            var instance = Activator.CreateInstance(genType);

            ((WebApiBase)instance).ExceptionJsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Converters = accessModelAttr.JsonConverters.Select(t => (JsonConverter)Activator.CreateInstance(t)).ToArray()
            };

            if (outputAssembly)
            {
                asmBuilder.Save(asmName + ".dll");
            }

            return (T)instance;
        }

        private static void ImplementInvokeMethod(MethodBuilder instanceMethodBuilder, MethodBuilder invokeMethodBuilder, ParameterInfo[] methodParams)
        {
            var il = instanceMethodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);

            for (int i = 0; i < methodParams.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        il.Emit(OpCodes.Ldarg_1);
                        break;
                    case 1:
                        il.Emit(OpCodes.Ldarg_2);
                        break;
                    case 2:
                        il.Emit(OpCodes.Ldarg_3);
                        break;
                    default:
                        il.Emit(OpCodes.Ldarg_S, (byte)(i + 1));
                        break;
                }
            }

            il.EmitCall(OpCodes.Call, invokeMethodBuilder, null);
            il.Emit(OpCodes.Ret);
        }

        private static void ImplementInvokeMethod(MethodBuilder instanceMethodBuilder, MethodBuilder invokeMethodBuilder, FieldInfo targetField, ParameterInfo[] methodParams)
        {
            var il = instanceMethodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, targetField);

            for (int i = 0; i < methodParams.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        il.Emit(OpCodes.Ldarg_1);
                        break;
                    case 1:
                        il.Emit(OpCodes.Ldarg_2);
                        break;
                    case 2:
                        il.Emit(OpCodes.Ldarg_3);
                        break;
                    default:
                        il.Emit(OpCodes.Ldarg_S, (byte)(i + 1));
                        break;
                }
            }

            il.EmitCall(OpCodes.Call, invokeMethodBuilder, null);
            il.Emit(OpCodes.Ret);
        }
    }
}
