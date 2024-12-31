///-------------------------------------------------------------------
///
///
///   Description:    JSON mapping properties for Unity Mds Updater
///   Author:         Alex Kern, Vincent Uhlmann                  
///  
///
///-------------------------------------------------------------------

using System;

using System.Collections.Generic;

[Serializable]
public class MDSxServerVersion
{
    public Values[] values;
    [Serializable]
    public class Values
    {
        public string path;
    }
}

[Serializable]
public class MDSxAndroidDependency
{
    public Android android;
    [Serializable]
    public class Android
    {
        public string version;
        public List<string> implementation;
    }
}