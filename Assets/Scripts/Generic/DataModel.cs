using System.Collections.Generic;

public class DataModel
{
    //make json value of 2 string
    private Dictionary<string, string> data = new Dictionary<string, string>();

    public void SetData(string key, string value)
    {
        data[key] = value;
    }

    public string GetData(string key)
    {
        if (data.TryGetValue(key, out string value))
            return value;
        return null;
    }
}

