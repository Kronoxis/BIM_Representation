using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class IFCDataContainer
{
    public FileInfo File;
    public int Index;

    private List<IFCEntity> _entities = new List<IFCEntity>();
    private ILookup<Type, IFCEntity> _entitiesByType;
    private ILookup<uint, IFCEntity> _entitiesById;

    private IFCParser _parser;
    private bool _isParsed = false;

    public IFCDataContainer(FileInfo file, uint batchSize = 500)
    {
        File = file;
        // Create the parser
        _parser = new IFCParser(file);

        // Check if the parser and file are valid
        if (!_parser.IsValid()) return;

        // Parse and save data, close when done
        _parser.SetBatchSize(batchSize);
        CoroutineManager.Instance().BeginCoroutine(GetData());
    }

    private IEnumerator GetData()
    {
        // Parse and save data
        _entities = new List<IFCEntity>();
        CoroutineManager.Instance().BeginCoroutine(_parser.ReadDataBatch(e => _entities.Add(e)));
        while (!_parser.IsEof()) yield return 0;

        // Sort data
        _entitiesByType = _entities.ToLookup(item => item.GetType(), item => item);
        _entitiesById = _entities.ToLookup(item => item.Id, item => item);

        // Close the parser
        _parser.Close();
        _isParsed = true;
    }

    public int GetParsedLineCount()
    {
        return _parser.GetParsedLineCount();
    }
    public int GetParsedCharCount()
    {
        return _parser.GetParsedCharCount();
    }
    public bool IsParsed()
    {
        return _isParsed;
    }

    public IFCEntity GetEntity(uint id)
    {
        if (id == 0) return null;
        return _entitiesById[id].ToList()[0];
    }

    public T GetEntity<T>(uint id) where T : IFCEntity
    {
        return (T)_entitiesById[id].ToList()[0];
    }

    public List<IFCEntity> GetEntities(Type type, bool includeSubEntities)
    {
        List<IFCEntity> toRet = new List<IFCEntity>();
        toRet.AddRange(_entitiesByType[type].ToList());

        if (includeSubEntities)
        {
            var subEntityTypes = type.Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type));

            foreach (var subEntityType in subEntityTypes)
                toRet.AddRange(_entitiesByType[subEntityType]);
        }

        return toRet;
    }

    public List<IFCEntity> GetEntities(List<uint> ids)
    {
        List<IFCEntity> toRet = new List<IFCEntity>();
        foreach (var id in ids)
        {
            if (id == 0) continue;
            toRet.Add(GetEntity(id));
        }
        return toRet;
    }

    public List<T> GetEntities<T>(bool includeSubEntities) where T : IFCEntity
    {
        List<T> toRet = new List<T>();
        foreach (var e in _entitiesByType[typeof(T)].ToList())
            toRet.Add((T)e);

        if (includeSubEntities)
        {
            foreach (var type in GetSubEntitytypes(typeof(T)))
                foreach (var e in _entitiesByType[type])
                    toRet.Add((T)e);
        }

        return toRet;
    }

    public List<T> GetEntities<T>(List<uint> ids) where T : IFCEntity
    {
        var t = typeof(T);
        var e = GetEntity(ids[0]);
        var et = (T)e;
        return ids.Select(id => GetEntity<T>(id)).ToList();
    }

    private List<Type> GetSubEntitytypes(Type type)
    {
        List<Type> toRet = new List<Type>();
        toRet.AddRange(type.Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)));
        var subEntities = toRet.ToArray();
        foreach (var subEntity in subEntities)
            toRet.AddRange(GetSubEntitytypes(subEntity));
        return toRet;
    }

    public void SetBatchSize(uint batchSize)
    {
        if (_parser != null)
            _parser.SetBatchSize(batchSize);
    }
}
