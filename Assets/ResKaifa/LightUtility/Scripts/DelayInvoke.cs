using System;
using System.Threading.Tasks;
/// <summary>
/// 封装后的延时调用函数，可直接使用匿名方法
/// </summary>
public class DelayInvoke
{
    public static async void StartTimmer(float delay, Action action)
    {
        await Task.Delay((int)(delay * 1000));
        action?.Invoke();
    }
    public static async void StartLoop(float delay,int times, float interval, Action action) {
        if (times <= 0) return;
        if (delay > 0) {
            await Task.Delay((int)(delay * 1000));
        }
        for (int i = 0; i < times; i++) {
            await Task.Delay((int)(interval * 1000));
            action?.Invoke();
        }
    }
}
