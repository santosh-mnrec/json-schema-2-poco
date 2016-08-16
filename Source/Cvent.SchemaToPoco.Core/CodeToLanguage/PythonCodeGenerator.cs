﻿using Cvent.SchemaToPoco.Core.Util;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Cvent.SchemaToPoco.Core.CodeToLanguage
{
    public class PythonCodeGenerator : ICodeGenerator
    {
        private IndentedTextWriter _output;
        private CodeGeneratorOptions _options;
        private readonly Dictionary<string, string> _typeMapping = new Dictionary<string, string>();
        private const string INDENTED = "    ";

        public bool IsValidIdentifier(string value)
        {
            throw new NotImplementedException();
        }

        public void ValidateIdentifier(string value)
        {
            throw new NotImplementedException();
        }

        public string CreateEscapedIdentifier(string value)
        {
            throw new NotImplementedException();
        }

        public string CreateValidIdentifier(string value)
        {
            throw new NotImplementedException();
        }

        public string GetTypeOutput(CodeTypeReference type)
        {
            throw new NotImplementedException();
        }

        public bool Supports(GeneratorSupport supports)
        {
            throw new NotImplementedException();
        }

        public void GenerateCodeFromExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            throw new NotImplementedException();
        }

        public void GenerateCodeFromStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o)
        {
            throw new NotImplementedException();
        }

        public void GenerateCodeFromNamespace(CodeNamespace e, TextWriter w, CodeGeneratorOptions o)
        {
            throw new NotImplementedException();
        }


        public void GenerateCodeFromCompileUnit(CodeCompileUnit e, TextWriter w, CodeGeneratorOptions o)
        {
            _options = o ?? new CodeGeneratorOptions();
            if (_output == null)
            {
                _output = new IndentedTextWriter(w, "    ");
            }

            if (HasEnumTypes(e))
            {
                _output.WriteLine("from enum import Enum");
                _output.WriteLine("");
                _output.WriteLine("");
            }
            GeneratePythonFileStart();
            GeneratePythonNameSpaces(e);
        }

        private static bool HasEnumTypes(CodeCompileUnit e)
        {

            foreach (CodeNamespace ns in e.Namespaces)
            {
                foreach (CodeTypeDeclaration t in ns.Types)
                {
                    if (t.IsEnum)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void GeneratePythonNameSpaces(CodeCompileUnit codeCompileUnit)
        {
            foreach (CodeNamespace ns in codeCompileUnit.Namespaces)
            {
                GeneratePythonNameTypes(ns);
            }
        }

        private void GeneratePythonNameTypes(CodeNamespace ns)
        {
            foreach (CodeTypeDeclaration type in ns.Types)
            {
                GenerateClass(type);
                GenerateEnum(type);
                _output.WriteLineNoTabs(string.Empty);
            }
        }

        private void GenerateEnum(CodeTypeDeclaration type)
        {
            if (type.IsEnum)
            {
                _output.Write("class {0}(Enum):", type.Name);
                _output.WriteLine("");
                _output.Indent++;
                GeneratePythonMembers(type);
                _output.Indent--;
                _output.WriteLine("");
            }
        }

        private void GenerateClass(CodeTypeDeclaration type)
        {
            if (type.IsClass)
            {
                _output.WriteLine("class {0}:", type.Name);
                _output.Indent++;
                GeneratePythonConstuctor(type);
                GeneratePythonMembers(type);
                _output.Indent--;
            }
        }

        private void GeneratePythonConstuctor(CodeTypeDeclaration type)
        {
            _output.Write("def __init__(self");
            string paramSep = "";
            foreach (CodeTypeMember member in type.Members)
            {
                if (member.Name == ".ctor")
                    continue;

                if (member is CodeMemberField)
                {
                    paramSep = ", ";
                    var propertyName = member.Name.Replace(" { get; set; } //", string.Empty);

                    //getter
                    _output.Write("{0}{1}=None", paramSep, StringUtils.LowerFirst(propertyName));
                }
            }
            _output.WriteLine("):");
        }

        private void GeneratePythonMembers(CodeTypeDeclaration type)
        {
            foreach (CodeTypeMember member in type.Members)
            {
                if(member.Name == ".ctor")
                    continue;
                if (type.IsEnum)
                {
                    _output.WriteLine("{0}{1} = '{2}'",INDENTED, member.Name, member.Name);
                }
                else
                {
                    if (member is CodeMemberField)
                    {

                        var propertyName = member.Name.Replace(" { get; set; } //", string.Empty);

                        //getter
                        _output.WriteLine("    self.{0} = {0}", StringUtils.LowerFirst(propertyName));
                        //_output.WriteLine("@property");
                        //_output.WriteLine("def {0}(self):", StringUtils.LowerFirst(propertyName));
                        //_output.WriteLine("{0}return self.__{1}", INDENTED, StringUtils.LowerFirst(propertyName));
                        //_output.WriteLine("");

                        //setter
                        //_output.WriteLine("@{0}.setter", StringUtils.LowerFirst(propertyName));
                        //_output.WriteLine("def {0}(self, value):", StringUtils.LowerFirst(propertyName));
                        //_output.WriteLine("{0}self.__{1} = value", INDENTED, StringUtils.LowerFirst(propertyName));

                    }
                }
            }
        }

        private string GetPhpType(string declaringType)
        {
            string phpType = declaringType;
            if (_typeMapping.ContainsKey(declaringType.ToLower()))
            {
                phpType = _typeMapping[declaringType.ToLower()];
            }
            else if (phpType.StartsWith("List<"))
            {
                phpType = phpType.Replace("List<", "").Replace(">", "[]");
            }
            Debug.WriteLine("Declaring Type: " + declaringType + " new type: "  + phpType);
            return phpType;
        }

        private void OutputStartingBrace()
        {
            if (_options.BracingStyle == "C")
            {
                _output.WriteLine("");
                _output.WriteLine("{");
            }
            else
            {
                _output.WriteLine(" {");
            }
        }

        private void GeneratePhpFileEnd()
        {
            _output.WriteLineNoTabs(string.Empty);
            _output.WriteLineNoTabs("?>");
        }

        private void GeneratePythonFileStart()
        {
            
            //_output.Indent = 1;
            _output.WriteLine("\"\"\"");
            _output.WriteLine(@" <auto-generated>");
            _output.WriteLine(@"     This code was generated by a tool.");
            _output.WriteLine(@"     Runtime Version: " + Environment.Version);
            _output.WriteLine(@"");
            _output.WriteLine(@"     Changes to this file may cause incorrect behavior and will be lost if");
            _output.WriteLine(@"     the code is regenerated.");
            _output.WriteLine(@" </auto-generated>");
            _output.WriteLine("\"\"\"");
            
            _output.WriteLineNoTabs(string.Empty);
            _output.WriteLineNoTabs(string.Empty);
            
        }

        public void GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o)
        {
            throw new NotImplementedException();
        }


    }
}