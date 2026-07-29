using UnityEngine;


namespace ARPG.Framework.Core.Singleton
{

    public abstract class MonoSingleton<T>
        : MonoBehaviour
        where T : MonoSingleton<T>
    {


        private static T _instance;


        public static T Instance
        {
            get
            {

                if (_instance == null)
                {

                    var obj =
                        new GameObject(typeof(T).Name);


                    _instance =
                        obj.AddComponent<T>();


                    DontDestroyOnLoad(obj);

                }


                return _instance;
            }
        }



        protected virtual void Awake()
        {

            if (_instance == null)
            {

                _instance = this as T;

                DontDestroyOnLoad(gameObject);

            }

            else
            {

                Destroy(gameObject);

            }

        }

    }

}