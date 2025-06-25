using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using UnityEngine.Rendering;

[Preserve]
public class StartNoLogo
{
    // Start is called before the first frame update
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void Run()
    {
        Task.Run(() =>
        {
            SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
        });
    }
}
