namespace DSmall.CrmSvcUtil.CleanUpFile
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>The program.</summary>
    public class Program
    {
        /// <summary>The main.</summary>
        /// <param name="arguments">The arguments.</param>
        public static void Main(string[] arguments)
        {
            var filePath = arguments[0];

            var fileContents = ReadFile(filePath);
            fileContents = RemoveUsingStatements(fileContents);
            fileContents = RemoveWhitespace(fileContents);
            fileContents = RemoveUnnecessaryEmptyLines(fileContents);
            fileContents = RemoveTabs(fileContents);
            
            fileContents = RemoveAssemblyAttributes(fileContents);
            fileContents = RemoveSummaryInformation(fileContents);
            fileContents = RemoveFullyQualifiedTypes(fileContents);
            fileContents = RemoveUnnecessaryAttributeTag(fileContents);

            fileContents = RemoveThisKeyword(fileContents);
            fileContents = RemovePartialKeyword(fileContents);

            fileContents = RemoveUnnecessaryGenericTypeSpecification(fileContents);
            fileContents = RemoveUnnecessaryParenthesis(fileContents);

            // fileContents = MoveConstantsToTopOfClass(fileContents);
            
            fileContents = ChangeNullableToQuestionMark(fileContents);
            fileContents = ChangeConstructorLayout(fileContents);

            fileContents = AddUsingStatements(fileContents);
            fileContents = AddSummaryInformationToConstructor(fileContents);
            fileContents = AddSummaryInformationToGetterOnlyProperties(fileContents);
            fileContents = AddSummaryInformationToGetterAndSetters(fileContents);

            SaveFile(filePath, fileContents);
        }

        private static string AddSummaryInformationToGetterAndSetters(string fileContents)
        {
            return fileContents;
        }

        private static string AddSummaryInformationToConstructor(string fileContents)
        {
            const string Pattern = @"\r\n(\s+)public(\s?)(\w+)[\(][\)]\r\n(\s+): base";
            var replacementText = string.Format("\r\n        /// <summary>Initialises a new instance of the <see cref=\"{1}\"/> class.</summary>{0}", "$0", "$3");

            return Regex.Replace(fileContents, Pattern, replacementText);
        }

        private static string ChangeConstructorLayout(string fileContents)
        {
            const string Pattern = @"public[\s](\w+)\(\)[\s]?\:[\s]?\r\n(\s*)base\(EntityLogicalName\)";
            const string ReplacementText = "public $1()\r\n            : base(EntityLogicalName)";

            return Regex.Replace(fileContents, Pattern, ReplacementText);
        }

        private static string RemovePartialKeyword(string fileContents)
        {
            return fileContents.Replace("public partial class", "public class");
        }

        private static string RemoveSummaryInformation(string fileContents)
        {
            const string Pattern = @"([ ]*)/// <summary>[\r\n]?(\s*)///[\s](.*)[\r\n]?(\s*)/// </summary>\r\n";

            return Regex.Replace(fileContents, Pattern, string.Empty);
        }

        private static string AddSummaryInformationToGetterOnlyProperties(string fileContents)
        {
            const string Pattern = @"\}[\r\n]?(\s*)\r\n(\s*)\[(\w+)\(""(\w+)""\)\]\r\n(\s*)public ([\w]+[\?]?) (.*)[\r\n]?(\s*)\{[\r\n]?(\s*)get[\r\n]?(\s*)\{[\r\n]?(\s*)(.+)[\r\n]?(\s*)\}[\r\n](\s*)[\}]";
            var replacementText = @"\r\n\r\n        /// <summary>Gets the $7.</summary>\r\n        [$3\(""$4""\)]\r\n        public $6 $7\r\n        {\r\n            get\r\n            {\r\n                $12\r\n            }\r\n        }".Replace("\r\n", Environment.NewLine);

            return Regex.Replace(fileContents, Pattern, replacementText);
        }

        private static string RemoveUnnecessaryParenthesis(string fileContents)
        {
            var output = fileContents;
            output = output.Replace("((PropertyChanged != null))", "(PropertyChanged != null)");
            output = output.Replace("((PropertyChanging != null))", "(PropertyChanging != null)");
            output = output.Replace("((int)(value))", "(int)value");
            return output;
        }

        private static string RemoveUnnecessaryEmptyLines(string fileContents)
        {
            return fileContents.Replace("{\r\n\r\n", "{\r\n");
        }

        private static string RemoveUnnecessaryGenericTypeSpecification(string fileContents)
        {
            return Regex.Replace(fileContents, @"SetRelatedEntities<(\w+)>", "SetRelatedEntities");
        }

        private static string ChangeNullableToQuestionMark(string fileContents)
        {
            return Regex.Replace(fileContents, @"Nullable<(\w+)>", "$1?");
        }

        private static string AddUsingStatements(string fileContents)
        {
            var output = string.Empty;
            output += "using System;\r\n";
            output += "using System.CodeDom.Compiler;\r\n";
            output += "using System.Collections.Generic;\r\n";
            output += "using System.ComponentModel;\r\n";
            output += "using System.Linq;\r\n";
            output += "using System.Runtime.Serialization;\r\n";
            output += "using Microsoft.Xrm.Sdk;\r\n";
            output += "using Microsoft.Xrm.Sdk.Client;\r\n";
            output += fileContents;
            return output;
        }

        private static string RemoveUsingStatements(string fileContents)
        {
            return Regex.Replace(fileContents, @"using[\s]([\w]*[\.])*[\w]*\;\r\n", string.Empty);
        }

        private static string RemoveThisKeyword(string fileContents)
        {
            return fileContents.Replace("this.", string.Empty);
        }

        private static string RemoveUnnecessaryAttributeTag(string fileContents)
        {
            var output = fileContents;
            output = output.Replace("DataContractAttribute()", "DataContract");
            output = output.Replace("EnumMemberAttribute()", "EnumMember");
            output = output.Replace("ObsoleteAttribute()", "Obsolete");
            output = output.Replace("ProxyTypesAssemblyAttribute()", "ProxyTypesAssembly");

            output = output.Replace("AttributeLogicalNameAttribute(", "AttributeLogicalName(");
            output = output.Replace("GeneratedCodeAttribute(", "GeneratedCode(");
            output = output.Replace("EntityLogicalNameAttribute(", "EntityLogicalName(");
            output = output.Replace("RelationshipSchemaNameAttribute(", "RelationshipSchemaName(");
            return output;
        }

        private static string RemoveFullyQualifiedTypes(string fileContents)
        {
            var output = fileContents;
            output = output.Replace("Target360.Crm.Entities.", string.Empty);
            output = output.Replace("Microsoft.Xrm.Sdk.Client.", string.Empty);
            output = output.Replace("Microsoft.Xrm.Sdk.", string.Empty);
            output = output.Replace("System.Runtime.Serialization.", string.Empty);
            output = output.Replace("System.CodeDom.Compiler.", string.Empty);
            output = output.Replace("System.ComponentModel.", string.Empty);
            output = output.Replace("System.Collections.Generic.", string.Empty);
            output = output.Replace("System.Linq.", string.Empty);
            output = output.Replace("System.", string.Empty);
            return output;
        }

        private static string RemoveAssemblyAttributes(string fileContents)
        {
            return fileContents.Replace("[assembly: Client.ProxyTypesAssemblyAttribute()]\r\n", string.Empty);
        }

        private static string RemoveTabs(string fileContents)
        {
            return fileContents.Replace("\t", "    ");
        }

        private static string RemoveWhitespace(string fileContents)
        {
            var output = fileContents;
            for (var i = 0; i < 4; i++)
            {
                output = output.Replace("    \r\n", "\r\n");
            }

            return output;
        }

        private static string ReadFile(string filePath)
        {
            using (var streamReader = new StreamReader(filePath))
            {
                return streamReader.ReadToEnd();
            }
        }

        private static void SaveFile(string filePath, string fileContents)
        {
            using (var streamWriter = new StreamWriter(filePath))
            {
                streamWriter.Write(fileContents);
            }
        }
    }
}
