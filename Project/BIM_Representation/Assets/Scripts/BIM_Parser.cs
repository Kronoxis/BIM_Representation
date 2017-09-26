using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.Playables;

public class BIM_Parser
{
    public const string SupportedEncoding = "ISO-10303-21";
    public const string SupportedExtension = ".ifc";

    private bool _isValid = false;

    private StreamReader _sr;
    private string _line;
    private string _encoding;
    private HeaderBlock _headerBlock;
    private static Dictionary<uint, IfcTopologicalRepresentationItem> _items = new Dictionary<uint, IfcTopologicalRepresentationItem>();

    #region Constructor
    public BIM_Parser(string filePath)
    {
        if (!CheckExtension(filePath)) return;
        _sr = new StreamReader(filePath);
        ReadEncoding();
        if (!CheckEncoding()) return;
        ReadHeader();
        if (!CheckHeader()) return;
        _isValid = true;
    }
    #endregion

    #region Destructor
    ~BIM_Parser()
    {
        Cleanup();
    }
    #endregion

    #region Extension
    private bool CheckExtension(string filePath)
    {
        if (new FileInfo(filePath).Extension != SupportedExtension)
        {
            Debug.LogError("File format not supported!\nSupported Extension: " + SupportedExtension);
            return false;
        }
        return true;
    }
    #endregion

    #region Encoding
    private void ReadEncoding()
    {
        _line = _sr.ReadLine();
        _encoding = _line.Substring(0, _line.Length - 1);
    }

    private bool CheckEncoding()
    {
        if (_encoding != SupportedEncoding)
        {
            Debug.LogError("Encoding not supported!\nFile's Version: " + _encoding + "\nSupported Version: " + SupportedEncoding);
            return false;
        }
        return true;
    }
    #endregion

    #region Header
    private void ReadHeader()
    {
        // Find Section Start
        var tryCount = 0;
        var isValid = false;
        while (!isValid)
        {
            if (!GetLine()) return;
            if (_line == "HEADER;")
            {
                isValid = true;
                continue;
            }
            ++tryCount;
            if (tryCount > 10)
            {
                Debug.LogError("Missing Header!");
                return;
            }
        }

        // Read Header Block
        _headerBlock = new HeaderBlock(_sr);

        // Find Section End
        tryCount = 0;
        isValid = false;
        while (!isValid)
        {
            if (!GetLine()) return;
            if (_line == "ENDSEC;")
            {
                _headerBlock.Valid = true;
                isValid = true;
                continue;
            }
            ++tryCount;
            if (tryCount > 10)
            {
                Debug.LogError("Missing Header End!");
                return;
            }
        }
    }

    private bool CheckHeader()
    {
        if (!_headerBlock.Valid)
        {
            Debug.LogError("Header invalid!");
            return false;
        }
        return true;
    }
    #endregion

    #region Data
    public void GetNextIfcTopologicalRepresentationItem()
    {
        // Get next data line
        do
        {
            if (!GetLine()) return;

        } while (_line[0] != '#');

        // Create Topological Representation Item
        var item = GetTopologicalRepresentationItem(_line);
        if (item == IfcTopologicalRepresentationItems.Null) return;
        var id = ParseHelpers.GetId(_line);
        Debug.Log("ID: " + id); 
        switch (item)
        {
            case IfcTopologicalRepresentationItems.IfcCartesianPoint:
                _items.Add(id, new IfcCartesionPoint(id, _line));
                break;
            case IfcTopologicalRepresentationItems.IfcPolyLoop:
                _items.Add(id, new IfcPolyLoop(id, _line));
                break;
            case IfcTopologicalRepresentationItems.IfcFaceOuterBound:
                _items.Add(id, new IfcFaceOuterBound(id, _line));
                break;
            case IfcTopologicalRepresentationItems.IfcFace:
                _items.Add(id, new IfcFace(id, _line));
                break;
            case IfcTopologicalRepresentationItems.IfcClosedShell:
                _items.Add(id, new IfcClosedShell(id, _line));
                break;
            case IfcTopologicalRepresentationItems.Null:
                break;
        }
    }

    private IfcTopologicalRepresentationItems GetTopologicalRepresentationItem(string line)
    {
        var begin = line.IndexOf('=') + 2;
        var length = line.IndexOf('(') - begin;
        var lineType = line.Substring(begin, length);
        var items = Enum.GetValues(typeof(IfcTopologicalRepresentationItems)).Cast<IfcTopologicalRepresentationItems>().ToList();
        foreach (var item in items)
        {
            if (lineType.ToUpper().Equals(item.ToString().ToUpper()))
            {
                return item;
            }
        }
        return IfcTopologicalRepresentationItems.Null;
    }
    #endregion

    #region Cleanup
    private void Cleanup()
    {
        _sr.Dispose();
    }
    #endregion

    #region Get Line
    private bool GetLine()
    {
        do
        {
            if (IsEnd()) return false;
            _line = _sr.ReadLine();
        } while (string.IsNullOrEmpty(_line));
        return true;
    }
    #endregion

    #region Checks
    public bool IsEnd()
    {
        return _sr.EndOfStream || _line == ("END-" + _encoding + ";");
    }

    public bool IsValid()
    {
        return _isValid;
    }
    #endregion

    public static T GetItem<T>(uint id) where T : IfcTopologicalRepresentationItem
    {
        return _items[id] as T;
    }


    //public string IfcFilePath;
    //
    //private string _encoding;
    //[SerializeField]
    //private IfcSectionTypes _currentSection = IfcSectionTypes.Null;
    //
    //private Dictionary<uint, IfcEntity> _entityList = new Dictionary<uint, IfcEntity>();
    //
    //// Use this for initialization
    //private void Start()
    //{
    //    if (!string.IsNullOrEmpty(IfcFilePath))
    //    {
    //        Parse(IfcFilePath);
    //    }
    //    Debug.Log(_headerBlock.ToString());
    //    Debug.Log("Entities Count: " + _entityList.Count);
    //}
    //
    //
    //private void Parse(string file)
    //{
    //    // Open File
    //    StreamReader sr = new StreamReader(file);
    //
    //    // Read Lines
    //    bool isEof = sr.EndOfStream;
    //    while (!isEof)
    //    {
    //        // Read Line
    //        var line = sr.ReadLine();
    //        isEof = sr.EndOfStream;
    //
    //        // Skip empty lines
    //        if (string.IsNullOrEmpty(line)) continue;
    //
    //        #region Read Encoding
    //        if (string.IsNullOrEmpty(_encoding))
    //        {
    //            _encoding = line.Substring(0, line.Length - 1);
    //            if (_encoding != SupportedEncoding)
    //            {
    //                Debug.LogError("IFC Version not supported!\nFile's Version: " + _encoding + "\nSupported Version: " + SupportedEncoding);
    //                isEof = true;
    //            }
    //            continue;
    //        }
    //        #endregion
    //
    //        #region Read IFC Section Type
    //        if (_currentSection == IfcSectionTypes.Null)
    //        {
    //            _currentSection = GetSectionType(line);
    //            Debug.Log("Opening Section " + _currentSection);
    //            continue;
    //        }
    //        #endregion
    //
    //        #region Read IFC Header
    //        if (_currentSection == IfcSectionTypes.Header && !_headerBlock.IsValid())
    //        {
    //            _headerBlock = new HeaderBlock(line, sr.ReadLine(), sr.ReadLine());
    //            continue;
    //        }
    //        #endregion
    //
    //        #region Check for End Section
    //        if (line == "ENDSEC;")
    //        {
    //            // Reset Section so it can be reassigned
    //            Debug.Log("Closing Section " + _currentSection);
    //            _currentSection = IfcSectionTypes.Null;
    //            continue;
    //        }
    //        #endregion
    //
    //        #region Read Data Type
    //        if (_currentSection == IfcSectionTypes.Data)
    //        {
    //            _entityList.AddEntity(line);
    //            continue;
    //        }
    //        #endregion
    //
    //        #region Check End File
    //        if (_currentSection == IfcSectionTypes.End)
    //        {
    //            Debug.Log("Closing Section " + _currentSection);
    //            isEof = true;
    //            continue;
    //        }
    //        #endregion
    //    }
    //
    //    // Close File
    //    Debug.Log("Closing File");
    //    sr.Close();
    //}
    //
    //private IfcSectionTypes GetSectionType(string line)
    //{
    //    if (line == "HEADER;")
    //        return IfcSectionTypes.Header;
    //    if (line == "DATA;")
    //        return IfcSectionTypes.Data;
    //    if (line == "END-" + _encoding + ";")
    //        return IfcSectionTypes.End;
    //
    //    // Fallback, will cause entire section to be skipped if section is not defined in enum
    //    Debug.LogWarning("Unknown Section! \n" + line);
    //    return IfcSectionTypes.Null;
    //}
}
