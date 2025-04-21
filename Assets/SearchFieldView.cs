using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SearchFieldView : MonoBehaviour
{
    UIDocument _document;

    void Start()
    {
        _document = GetComponent<UIDocument>();
        VisualElement root = _document.rootVisualElement;
        var searchLable = root.Query<Label>("SearchLable").First();
        var searchField = root.Query<TextField>("InputSearchField").First();
        searchLable.visible = false;
        searchField.style.flexGrow = 1;
        
    }
}
