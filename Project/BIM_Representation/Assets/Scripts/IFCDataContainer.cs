using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IFCDataContainer {

    public IFCDataContainer(string filePath, uint batchSize = 500)
    {
        // Create the parser
        var parser = new IFCParser(filePath);

        // Check if the parser and file are valid
        if (!parser.IsValid()) return;
        Debug.Log(filePath + ": Valid! Let's parse!");

        // Parse and save Data 

        // Close the parser
        parser.Close();
    }
}
