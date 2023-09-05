using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface IView
{
    public void UpdateView<T>(string key, T value);
    public string DataKey { get; }
}

public class DataView : MonoBehaviour, IView
{
    [Header("Data Key")]
    [SerializeField]
    private string dataKey; // Set this via the Unity Inspector
    
    // Add this property to expose the dataKey
    public string DataKey => dataKey;
    
    public void UpdateView<T>(string key, T value)
    {
        if (key != dataKey) return;
        
        if (value is string textValue)
            SetTextData(textValue);
        else if (value is Sprite spriteValue) 
            SetImageData(spriteValue);
    }

    private void SetTextData(string value)
    {
        GetComponent<TextMeshProUGUI>().text = value;
    }

    private void SetImageData(Sprite value)
    {
        GetComponent<Image>().sprite = value;
    }
}
