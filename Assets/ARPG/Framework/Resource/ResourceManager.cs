using Framework.Core.Singleton;
using UnityEngine;


public class ResourceManager
    : MonoSingleton<ResourceManager>
{

    public void Load()
    {

        Debug.Log("Load Resource");

    }

}
