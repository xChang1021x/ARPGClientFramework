namespace ARPG.Framework.Core.Singleton
{
    /// <summary>
    /// 泛型单例基类
    /// 用于纯C#对象
    /// </summary>
    public abstract class Singleton<T>
        where T : Singleton<T>, new()
    {

        private static T _instance;


        private static readonly object _lock = new();


        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();

                            _instance.OnInit();
                        }
                    }
                }


                return _instance;
            }
        }



        protected virtual void OnInit()
        {

        }


        public virtual void Dispose()
        {

        }
    }
}