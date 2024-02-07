using System.Collections;
using System.Collections.Generic;
using Services;
using UI.Model.Templates;
using UnityEngine;

public class TemplateServer : MonoBehaviour, IService
{
    public GraphElementTemplate GraphElement;
    public void Awake()
    {
        ServiceLocator.RegisterService(this);
    }
    
}
