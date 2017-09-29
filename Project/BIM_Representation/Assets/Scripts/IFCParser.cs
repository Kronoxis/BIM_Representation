using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class IFCParser
{
    // Properties
    public static readonly string SupportedExtension = ".ifc";
    public static readonly string SupportedISO = "ISO-10303-21";
    public static readonly string[] SupportedSchemas = {"IFC2X3", "IFC4"};

    // Private Variables
    private StreamReader _sr;
    private string _filePath;
    private bool _isEof = false;
    private bool _isValid = false;
    private bool _isFirstDataLine = true;

    private int _propertyStartOffset;

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
            Debug.LogError(_filePath + ": Schema not supported! (" + schema + ")\nSupported Schemas: " + ArrayToString(SupportedSchemas));
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
    public void ReadDataLine()
    {
        var line = GetLine();
        // Format:
        // #ID=ENTITY(PROPERTY,PROPERTY,...)
        // When reading first line, define format (Possibly spaces between = and Property
        if (_isFirstDataLine)
        {
            _propertyStartOffset = 1;
            if (line.IndexOf("= ") != -1)
                _propertyStartOffset = 2;
        }
        var id = line.Substring(1, line.IndexOf('='));

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
            char c = (char) _sr.Read();

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

        //do
        //{
        //    line = _sr.ReadBlock()
        //    line = _sr.ReadLine();
        //} while (string.IsNullOrEmpty(line) || IsComment(line, ref isBlock));

        // Trim ; at end
        //return line.TrimEnd(';');
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

    private string ArrayToString<T>(T[] array, char delim = ',')
    {
        string toRet = "";
        for (int i = 0; i < array.Length; ++i)
        {
            toRet += array[i];
            if (i < array.Length - 1)
                toRet += delim;
        }
        return toRet;
    }
    #endregion

    #region Getters
    public bool IsValid()
    {
        return _isValid;
    }
    #endregion
}
