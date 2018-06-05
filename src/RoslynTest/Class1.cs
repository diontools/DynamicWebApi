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
            var proj = GetWebApiProject();
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

        private static Project GetWebApiProject()
        {
            var work = MSBuildWorkspace.Create();
            var solution = work.OpenSolutionAsync("../../../DynamicWebApi.sln").Result;
            var project = solution.Projects.FirstOrDefault(p => p.Name.EndsWith("SampleLib"));
            if (project == null)
                throw new ApplicationException("WebApi project not found in solution ");
            return project;
        }
    }
}
