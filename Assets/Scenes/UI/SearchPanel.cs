using System.Collections;

using UnityEngine;
using UnityEngine.UIElements;

public class SearchPanel : MonoBehaviour
{
    [SerializeField] UIDocument _document;
    [SerializeField] StyleSheet _styleSheet;
    VisualElement root;

    VisualElement _container;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(InitializeView());
    }

    IEnumerator InitializeView()
    {
        root = _document.rootVisualElement;
        root.Clear();
        
        root.styleSheets.Add(_styleSheet);
        
        _container = root.CreateChild("container");
        
        _container.CreateChild("searchPanelFrame");
        
        
        yield return null;
    }
}
