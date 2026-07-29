using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        var a = TestConfigManager.Instance;

        var b = TestConfigManager.Instance;

        Debug.Log(a == b);

        ResourceManager.Instance.Load();

    }

    // Update is called once per frame
    void Update()
    {

    }
}
