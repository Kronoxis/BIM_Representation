﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class IFCDataContainer
{
    private List<IFCEntity> _entities = new List<IFCEntity>();
    private ILookup<Type, IFCEntity> _entitiesByType;
    private ILookup<uint, IFCEntity> _entitiesById;

    private IFCParser _parser;
    private bool _isParsed = false;

    public IFCDataContainer(string filePath, uint batchSize = 500)
    {
        // Create the parser
        _parser = new IFCParser(filePath);

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
    public bool IsParsed()
    {
        return _isParsed;
    }

    public IFCEntity GetEntity(uint id)
    {
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

    public List<T> GetEntities<T>(bool includeSubEntities) where T : IFCEntity
    {
        List<T> toRet = new List<T>();
        foreach (var entity in _entitiesByType[typeof(T)].ToList())
            toRet.Add((T) entity);

        if (includeSubEntities)
        {
            var subEntityTypes = typeof(T).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(T)));

            foreach (var subEntityType in subEntityTypes)
            {
                foreach (var subEntity in _entitiesByType[subEntityType].ToList())
                {
                    toRet.Add((T) subEntity);
                }
            }
        }

        return toRet;
    }

    public void SetBatchSize(uint batchSize)
    {
        if (_parser != null)
            _parser.SetBatchSize(batchSize);
    }
}