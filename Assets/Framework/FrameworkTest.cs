using Framework.Core.Singleton;
using UnityEngine;


public class TestConfigManager
    : Singleton<TestConfigManager>
{

    public int Value;


    protected override void OnInit()
    {
        Debug.Log("Config Init");
    }
}
