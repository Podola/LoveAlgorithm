using UnityEngine;
using LoveAlgo.Data;

namespace LoveAlgo.Core
{
    [DefaultExecutionOrder(-500)]
    public sealed class LoveAlgoBootstrapper : MonoBehaviour
    {
        [SerializeField] private LoveAlgoConfiguration configuration;

        private bool ownsContext;

        private void Awake()
        {
            if (configuration == null)
            {
                Debug.LogError("LoveAlgoConfiguration is not assigned", this);
                enabled = false;
                return;
            }

            if (LoveAlgoContext.Exists)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            LoveAlgoContext.Create(configuration);
            ownsContext = true;
        }

        private void OnDestroy()
        {
            if (ownsContext)
            {
                LoveAlgoContext.DisposeInstance();
            }
        }
    }
}
