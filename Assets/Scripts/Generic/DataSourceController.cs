using System;
using System.Collections;
using UnityEngine;
using LightJson;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Object = System.Object;

public class DataSourceController : MonoBehaviour
{
    [SerializeField]
    private GameObject dataPrefab;  // Prefab that contains multiple DataView components

    [SerializeField]
    private TextAsset dataSource;  // Drag your TextAsset here through the inspector

    [Header("Data Source Keys")]
    [SerializeField]
    private List<string> dataKeys = new List<string>(); // Add keys via Inspector

    private void Start()
    {
        var rootValue = JsonValue.Parse(dataSource.text);

        if (rootValue.IsJsonObject)
        {
            HandleJsonObject(rootValue.AsJsonObject);
        }
        else if (rootValue.IsJsonArray)
        {
            HandleJsonArray(rootValue.AsJsonArray);
        }
        else
        {
            Debug.LogError("Error.");
        }
    }

    //Handle a JSON object
    private void HandleJsonObject(JsonObject jsonObject)
    {
        bool isDataObject = dataKeys.Exists(key => jsonObject.ContainsKey(key));

        if (isDataObject)
        {
            DataModel modelData = new DataModel();

            foreach (var key in dataKeys)
            {
                modelData.SetData(key, jsonObject[key]);
            }

            CreateAndPopulateDataView(modelData);
        }
        else
        {
            foreach (var pair in jsonObject)
            {
                if (pair.Value.IsJsonObject)
                {
                    HandleJsonObject(pair.Value.AsJsonObject);
                }
                else if (pair.Value.IsJsonArray)
                {
                    HandleJsonArray(pair.Value.AsJsonArray);
                }
            }
        }
    }

    //Handle a JSON array
    private void HandleJsonArray(JsonArray jsonArray)
    {
        foreach (var item in jsonArray)
        {
            HandleJsonObject(item.AsJsonObject);
        }
    }
    
    
    private void CreateAndPopulateDataView(DataModel model)
    {
        // Instantiate the prefab
        GameObject dataObject = Instantiate(dataPrefab, transform);

        // Get the DataView component from the instantiated prefab itself
        IView view = dataObject.GetComponent<IView>();
        if (view != null)
        {
            string value = model.GetData(view.DataKey);
            if (value != null)
            {
                view.UpdateView(view.DataKey, value);
            }
        }

        // Find all DataView components within the children of the instantiated object
        IView[] childViews = dataObject.GetComponentsInChildren<IView>();

        foreach (var dataView in childViews)
        {
            // Avoid processing the main DataView again if it exists
            if (dataView != view)
            {
                string childValue = model.GetData(dataView.DataKey);
               
                TransformData(childValue, value =>
                {
                    if (value != null)
                    {
                        dataView.UpdateView(dataView.DataKey, value);
                    }
                });
            }
        }
    }

    private async void TransformData(string childValue, Action<object> callback = null)
    {   
        if (childValue == null) return;
        
        if(IsValidUrl(childValue))
        {
            //Image Data
            await DownloadImage(childValue, callback);
        }
        else
        {
            //Text Data
            callback?.Invoke(childValue);
        }
    }
    
    private bool IsValidUrl(string childValue)
    {
        return Uri.TryCreate(childValue, UriKind.Absolute, out var uriResult) && 
               (uriResult.Scheme == Uri.UriSchemeHttps) && 
               Uri.IsWellFormedUriString(childValue, UriKind.Absolute);
    }
    
    private async Task DownloadImage(object childValue, Action<object> callback = null)
    {
        var request = UnityWebRequestTexture.GetTexture(childValue as string);
        
        try
        {
            await SendWebRequestAsync(request);

            if(request.result == UnityWebRequest.Result.Success)
            {
                //Create Image and set texture
                var texture = DownloadHandlerTexture.GetContent(request);
                var rect = new Rect(0, 0, texture.width, texture.height);
                var sprite = Sprite.Create(texture, rect, new Vector2());
                
                callback?.Invoke(sprite);
            }
            else
            {
                Debug.LogError(string.Format("Image download failed. Error: {0}", request.error));
                callback?.Invoke(null);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(string.Format("Image download failed with an exception:{0}", ex));
            callback?.Invoke(null);
        }
    }

    private static Task<UnityWebRequest> SendWebRequestAsync(UnityWebRequest www)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();
        www.SendWebRequest().completed += operation => tcs.SetResult(www);
        return tcs.Task;
    }
    
}
