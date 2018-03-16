using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi
{
    static class TypeGenUtility
    {
        public static Type DefineArgsType(ModuleBuilder moduleBuilder, MethodInfo method, ParameterInfo[] parameters)
        {
            var argsTypeBuilder = moduleBuilder.DefineType(method.Name + "Args", TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.BeforeFieldInit, typeof(object));
            argsTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            if (parameters.Length > 0)
            {
                var argsTypeConstructorBuilder = argsTypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameters.Select(p => p.ParameterType).ToArray());
                var il = argsTypeConstructorBuilder.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));

                for (int i = 0; i < parameters.Length; i++)
                {
                    var p = parameters[i];
                    var field = argsTypeBuilder.DefineField(p.Name, p.ParameterType, FieldAttributes.Public);

                    il.Emit(OpCodes.Ldarg_0);

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

                    il.Emit(OpCodes.Stfld, field);
                }

                il.Emit(OpCodes.Ret);
            }

            return argsTypeBuilder.CreateType();
        }
    }
}
