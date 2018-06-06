using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynTest
{
    public class Class1
    {
        public static void Main()
        {
            var work = MSBuildWorkspace.Create();
            var solution = work.OpenSolutionAsync("../../../DynamicWebApi.sln").Result;
            var proj = solution.Projects.FirstOrDefault(p => p.Name.EndsWith("SampleLib"));
            if (proj == null)
                throw new ApplicationException("WebApi project not found in solution ");
            var comp = proj.GetCompilationAsync().Result;
            var targetAttr = comp.GetTypeByMetadataName(typeof(DynamicWebApi.AccessModelAttribute).FullName);
            
            foreach (var doc in proj.Documents)
            {
                var tree = doc.GetSyntaxTreeAsync().Result;
                var model = comp.GetSemanticModel(tree);

                foreach (var type in tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>())
                {
                    var attr = type.AttributeLists.SelectMany(list => list.Attributes).Where(t => model.GetTypeInfo(t).Type == targetAttr).FirstOrDefault();
                    if (attr != null)
                    {
                        var implType = model.GetTypeInfo(((TypeOfExpressionSyntax)attr.ArgumentList.Arguments[0].Expression).Type);

                        var methods = type.Members.Where(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.MethodDeclaration)).OfType<MethodDeclarationSyntax>();

                        var ns = model.GetDeclaredSymbol(type).ContainingNamespace.Name;

                        var sb = new StringBuilder();
                        sb.AppendLine("using DynamicWebApi.Client;");
                        sb.AppendLine("using SampleLib;");
                        sb.AppendLine("using System;");
                        sb.AppendLine("using System.Threading.Tasks;");
                        sb.Append("namespace ");
                        sb.Append(ns);
                        sb.AppendLine(" {");
                        sb.AppendLine("public class SampleWebImpl : WebApiBase, ISample {");

                        foreach (var method in methods)
                        {
                            var name = method.Identifier.ValueText;
                            var typeText = method.ReturnType.ToString();

                            sb.Append("public ");
                            sb.Append(typeText);
                            sb.Append(" ");
                            sb.Append(name);
                            sb.Append("(");

                            bool firstParam = true;
                            foreach (var param in method.ParameterList.Parameters)
                            {
                                if (firstParam)
                                {
                                    firstParam = false;
                                }
                                else
                                {
                                    sb.Append(",");
                                }

                                var paramName = param.Identifier.ValueText;
                                var paramTypeText = param.Type.ToString();
                                sb.Append(paramTypeText);
                                sb.Append(" ");
                                sb.Append(paramName);
                            }

                            sb.AppendLine(") {");

                            sb.Append("return PostAsync(this, \"");
                            sb.Append("Sample/");
                            sb.Append(name);
                            sb.Append("\", new { ");

                            firstParam = true;
                            foreach (var param in method.ParameterList.Parameters)
                            {
                                if (firstParam)
                                {
                                    firstParam = false;
                                }
                                else
                                {
                                    sb.Append(",");
                                }

                                sb.Append(param.Identifier.ValueText);
                            }

                            sb.Append(" }).AsResult<");
                            sb.Append(typeText);
                            sb.AppendLine(">();");

                            sb.AppendLine("}");
                        }

                        sb.AppendLine("}");
                        sb.AppendLine("}");
                    }
                }
            }
        }

        private static T Gen<T>(bool web, Func<T> gen)
        {
            if (web)
            {
            }

            return gen();
        }
    }
}
