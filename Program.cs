
using System.Text;
using System.Text.RegularExpressions;

namespace constructor.sqr;
internal class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: csqr <file.cs>");
            return;
        }

        try
        {
            string filePath = args[0];

            // Read class file content
            string classCode = ReadFile(filePath);

            // Parse class name
            string className = ParseClassName(classCode);
            Console.WriteLine($"Class Name: {className}");

            if (ConstructorExists(classCode, className))
            {
                Console.WriteLine("Constructor already exists");
                return;
            }
            // Parse class fields
            var fields = ParseClassFields(classCode);
            Console.WriteLine("Class Fields:");
            foreach (var field in fields)
            {
                Console.WriteLine($"{field.Item1} {field.Item2}");
            }

            // Generate constructor method
            string constructorMethod = GenerateConstructorMethod(className, fields);
            Console.WriteLine("\nGenerated Constructor Method:");
            Console.WriteLine(constructorMethod);

            // Modify the class file
            ModifyClassFile(filePath, constructorMethod);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }


    }

    static string ParseClassName(string classCode)
    {
        var classNameMatch = Regex.Match(classCode, @"class\s+([\w_]+)\s*{");
        if (classNameMatch.Success && classNameMatch.Groups.Count == 2)
        {
            return classNameMatch.Groups[1].Value;
        }
        else
        {
            throw new Exception("Unable to parse class name.");
        }
    }

    static List<Tuple<string, string>> ParseClassFields(string classCode)
    {
        try
        {
            var fieldMatches = Regex.Matches(classCode, @"(?:public|private|protected|internal|static)?\s+(\w+)\s+(\w+)\s*;");
            var fields = new List<Tuple<string, string>>();

            foreach (Match match in fieldMatches)
            {
                if (match.Success && match.Groups.Count == 3)
                {
                    var fieldType = match.Groups[1].Value;
                    var fieldName = match.Groups[2].Value;
                    fields.Add(new Tuple<string, string>(fieldType, fieldName));
                }
            }

            return fields;
        }
        catch (Exception ex)
        {
            throw new Exception("Unable to parse class fields.", ex);
        }
    }

    static string GenerateConstructorMethod(string className, List<Tuple<string, string>> fields)
    {
        StringBuilder constructorCode = new StringBuilder();

        // Capitalize the first letter of the class name
        string capitalizedClassName = char.ToUpper(className[0]) + className.Substring(1);

        constructorCode.AppendLine($"public {capitalizedClassName}(");
        for (int i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            constructorCode.Append("    ");

            constructorCode.Append($"    {field.Item1} {field.Item2}");

            // Add a comma and newline if it's not the last field
            if (i < fields.Count - 1)
            {
                constructorCode.Append(",\n");
            }

        }

        constructorCode.AppendLine(")");

        constructorCode.AppendLine("{");

        foreach (var field in fields)
        {
            constructorCode.Append("    ");
            constructorCode.AppendLine($"    this.{field.Item2} = {field.Item2};");
        }

        constructorCode.AppendLine("}");

        return constructorCode.ToString();
    }

    static void ModifyClassFile(string filePath, string constructorMethod)
    {
        try
        {
            // Read the content of the file
            string fileContent = ReadFile(filePath);

            // Find the position to insert the constructor method
            int index = FindInsertionIndex(fileContent);

            // Insert the constructor method at the appropriate position
            string modifiedContent = fileContent.Insert(index, constructorMethod);

            // Write the modified content back to the file
            WriteToFile(filePath, modifiedContent);

            Console.WriteLine("Class file modified successfully.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error modifying class file: {ex.Message}", ex);
        }
    }

    static int FindInsertionIndex(string fileContent)
    {
        // Find the position before the 2nd to last closing brace '}'
        int lastIndex = fileContent.LastIndexOf('}');
        int secondToLastIndex = fileContent.LastIndexOf('}', lastIndex - 1);

        // If the second-to-last closing brace is found, return its index
        // Otherwise, return the end of the file content
        return (secondToLastIndex >= 0) ? secondToLastIndex : fileContent.Length;
    }


    static string ReadFile(string filePath)
    {
        try
        {
            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading file: {ex.Message}", ex);
        }
    }

    static void WriteToFile(string filePath, string content)
    {
        try
        {
            File.WriteAllText(filePath, content);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error writing to file: {ex.Message}", ex);
        }
    }
    static bool ConstructorExists(string classCode, string className)
    {
        string constructorPattern = $"public {className}\\(";
        return Regex.IsMatch(classCode, constructorPattern);
    }
}