using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi.Server
{
    public static class TypeGenerator
    {
        public static MethodGenerator[] GenerateMethods(Type interfaceType)
        {
            var name = interfaceType.Name;
            if (name.StartsWith("I")) name = name.Remove(0, 1);

            var asmName = name + "WebApiImpl";
            var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(asmName), AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = asmBuilder.DefineDynamicModule("MainModule", asmName + ".dll");
            
            var methods = interfaceType.GetMethods();
            var list = new List<MethodGenerator>(methods.Length);
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                var argsType = TypeGenUtility.DefineArgsType(moduleBuilder, method, parameters);
                var argsFields = argsType.GetFields();
                
                var selfParameter = Expression.Parameter(typeof(object), "self");
                var argsParameter = Expression.Parameter(typeof(object), "args");
                var convertedArgs = Expression.Convert(argsParameter, argsType);
                var argsParameterFields = argsFields.Select(f => Expression.Field(convertedArgs, f)).ToArray();

                Func<object, object, object> caller;
                if (method.ReturnType.IsGenericType && typeof(Task<>) == method.ReturnType.GetGenericTypeDefinition())
                {
                    var lambda =
                        Expression.Lambda<Func<object, object, object>>(
                            Expression.Convert(
                                Expression.Property(
                                    Expression.Call(
                                        Expression.Convert(selfParameter, interfaceType),
                                        method,
                                        argsParameterFields),
                                "Result"),
                                typeof(object)),
                            selfParameter, argsParameter);

                    caller = lambda.Compile();
                }
                else
                {
                    var lambda =
                        Expression.Lambda<Func<object, object, object>>(
                            Expression.Convert(
                                Expression.Call(
                                    Expression.Convert(selfParameter, interfaceType),
                                    method,
                                    argsParameterFields),
                                typeof(object)),
                            selfParameter, argsParameter);

                    caller = lambda.Compile();
                }

                list.Add(new MethodGenerator
                {
                    Name = method.Name,
                    ArgsType = argsType,
                    ReturnType = method.ReturnType,
                    Caller = caller,
                });
            }
            
            //asmBuilder.Save(name + ".dll");
            
            return list.ToArray();
        }
    }

    public class MethodGenerator
    {
        public string Name { get; set; }

        public Type ArgsType { get; set; }

        public Type ReturnType { get; set; }

        public Func<object, object, object> Caller { get; set; }
    }
}
