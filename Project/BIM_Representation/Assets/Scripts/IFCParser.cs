using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class IFCParser
{
    // Properties
    public static readonly string SupportedExtension = ".ifc";
    public static readonly string SupportedISO = "ISO-10303-21";
    public static readonly string[] SupportedSchemas = { "IFC2X3", "IFC4" };

    // Private Variables
    private StreamReader _sr;
    private string _filePath;
    private bool _isEof = false;
    private bool _isValid = false;

    private int _parsedLineCount = 0;

    private static IEnumerable<Type> _entityTypeClasses = typeof(IFCEntity).Assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(IFCEntity)));

    #region Constructor
    public IFCParser(string filePath)
    {
        _filePath = filePath;
        if (!CheckExtension()) return;
        _sr = new StreamReader(filePath);
        if (!CheckISO()) return;
        if (!CheckHeader()) return;
        if (!CheckData()) return;
        _isValid = true;
    }
    #endregion

    #region Cleanup
    ~IFCParser()
    {
        Close();
    }

    public void Close()
    {
        _sr.Dispose();
    }
    #endregion

    #region Checks
    private bool CheckExtension()
    {
        if (SupportedExtension != new FileInfo(_filePath).Extension)
        {
            Debug.LogError(_filePath + ": Invalid Extension! Expected " + SupportedExtension);
            return false;
        }
        return true;
    }

    private bool CheckISO()
    {
        var line = GetLine();
        return line == SupportedISO;
    }

    private bool CheckHeader()
    {
        // Open Header Section
        var line = GetLine();
        if (line != "HEADER")
        {
            Debug.LogError(_filePath + ": Header Invalid! (Section not opened)");
            return false;
        }

        // Read Description, Name and Schema
        bool hasDescription = false, hasName = false, hasSchema = false;
        string schemaLine = null;
        for (int i = 0; i < 3; ++i)
        {
            line = GetLine();
            if (!hasDescription)
            {
                hasDescription = line.Contains("FILE_DESCRIPTION");
            }
            if (!hasName)
            {
                hasName = line.Contains("FILE_NAME");
            }
            if (!hasSchema)
            {
                hasSchema = line.Contains("FILE_SCHEMA");
                if (hasSchema) schemaLine = line;
            }
        }

        if (!hasDescription)
        {
            Debug.LogError(_filePath + ": File Description missing!");
            return false;
        }

        if (!hasName)
        {
            Debug.LogError(_filePath + ": File Name missing!");
            return false;
        }

        if (!hasSchema)
        {
            Debug.LogError(_filePath + ": File Schema missing!");
            return false;
        }

        // Check Schema
        if (!CheckSupportedSchema(schemaLine))
        {
            return false;
        }

        // Close Header Section
        line = GetLine();
        if (line != "ENDSEC")
        {
            Debug.LogError(_filePath + ": Header Invalid! (Section not closed)");
            return false;
        }

        // Header was valid
        return true;
    }

    private bool CheckSupportedSchema(string line)
    {
        var start = line.IndexOf('\'') + 1;
        var length = line.LastIndexOf('\'') - start;
        var schema = line.Substring(start, length);
        if (!Array.Exists(SupportedSchemas, elem => elem == schema))
        {
            Debug.LogError(_filePath + ": Schema not supported! (" + schema + ")\nSupported Schemas: " + Helpers.ArrayToString(SupportedSchemas));
            return false;
        }
        return true;
    }

    private bool CheckData()
    {
        // Prevent reading entire file if data section was not opened
        for (int i = 0; i < 10; ++i)
        {
            var line = GetLine();
            if (line == "DATA")
                return true;
        }
        Debug.LogError(_filePath + ": No Data Section found!");
        return false;
    }
    #endregion

    #region Data
    public IEnumerator ReadDataBatch(uint batchSize, Action<IFCEntity> readLine)
    {
        while (!_isEof)
        {
            var line = GetLine();
            if (line == "ENDSEC") break;
            readLine(ReadDataLine(line));
            ++_parsedLineCount;
            if (_parsedLineCount % batchSize == 0)
            {
                yield return 0;
            }
        }
        _isEof = true;
    }

    private IFCEntity ReadDataLine(string line)
    {
        // Format:
        // #ID=ENTITY(PROPERTY,PROPERTY,...)
        var equalsIdx = line.IndexOf('=');
        var bracketsIdx = line.IndexOf('(');
        // - Id
        var id = uint.Parse(line.Substring(1, equalsIdx - 1));
        // - Entity
        var entityType = Helpers.GetEntityType(line.Substring(equalsIdx + 1, bracketsIdx - (equalsIdx + 1)));
        // - List of properties
        var propertiesStr = line.Substring(bracketsIdx + 1, line.Length - 1 - (bracketsIdx + 1));

        // Create Entity
        var e = new IFCEntity(id, entityType, propertiesStr, ',');

        // Find matching class
        var matches = _entityTypeClasses
            .Where(t => entityType.ToString().ToUpper()
                        .Equals(t.ToString().ToUpper())).ToList();
        if (matches.Count < 1)
        {
            //Debug.LogWarning("IFCParser.ReadDataLine() > Specified Type has no matching Class!");
            return e;
        }

        // Create a derived class with data from the Entity
        var type = matches[0];
        var inst = Convert.ChangeType(Activator.CreateInstance(type, e), type);
        return (IFCEntity) inst;
    }
    #endregion

    #region Helpers
    private string GetLine(char delim = ';')
    {
        string line = "";

        // Don't get line when end of file
        _isEof = _sr.EndOfStream;
        if (_isEof) return null;

        // Read until delimiter
        bool isComment = false;
        bool isCommentBlock = false;
        bool isString = false;
        var prevChar = '\0';
        while (_sr.Peek() != -1)
        {
            // Read next character
            char c = (char)_sr.Read();

            // Remove comments
            // Single Line Start
            if (!isComment && prevChar == '/' && c == '/')
            {
                isComment = true;
                line = line.Remove(line.Length - 1);
                prevChar = c;
                continue;
            }
            // Single Line End
            if (isComment)
            {
                isComment = !(c == '\n' || c == '\r');
                prevChar = c;
                continue;
            }
            // Multi Line Start
            if (!isCommentBlock && prevChar == '/' && c == '*')
            {
                isCommentBlock = true;
                line = line.Remove(line.Length - 1);
                prevChar = c;
                continue;
            }
            // Multi Line End
            if (isCommentBlock)
            {
                isCommentBlock = !(prevChar == '*' && c == '/');
                prevChar = c;
                continue;
            }

            // Check if char is part of a string-type value
            if (c == '\'' || c == '"')
            {
                isString = !isString;
            }

            // Remove space/newline/carriage/tab, unless string
            if (!isString && (c == ' ' || c == '\n' || c == '\r' || c == '\t'))
            {
                prevChar = c;
                continue;
            }

            // Delimiter marks end of line (Unlessp art of string)
            if (!isString && c == delim)
            {
                break;
            }

            // Add char to list
            line += c;
            prevChar = c;
        }
        return line;
    }

    private bool IsComment(string line, ref bool isBlock)
    {
        // Single line
        if (line.IndexOf("//") != -1)
        {
            return true;
        }

        // In comment block
        if (isBlock)
        {
            // Line ends with */
            if (line.LastIndexOf("*/") != -1)
                isBlock = false;
            return true;
        }

        // Comment block
        if (line.IndexOf("/*") != -1)
        {
            if (line.LastIndexOf("*/") == -1)
                isBlock = true;
            return true;
        }

        // Not a comment
        return false;
    }
    #endregion

    #region Getters
    public bool IsValid()
    {
        return _isValid;
    }

    public bool IsEof()
    {
        return _isEof;
    }

    public int GetParsedLineCount()
    {
        return _parsedLineCount;
    }
    #endregion
}
