/// <summary>
/// 状态类的基类；can参考：https://www.cnblogs.com/xsxjin/p/6329711.html
/// 所有的角色状态都继承于它
/// </summary>
public class State<T> {

    /// <summary>
    /// 进入改状态时,所做的初始化动作
    /// </summary>
    /// <param name="entity"></param>
    public virtual void IntoState(T entity) { }

    /// <summary>
    /// 处于该状态时,要做的游戏逻辑
    /// </summary>
    /// <param name="entity"></param>
    public virtual void Execute(T entity){}

    /// <summary>
    /// 退出该状态时,做的收尾工作
    /// </summary>
    /// <param name="entity"></param>
    public virtual void Exit(T entity) { }

    /// <summary>
    /// 获取消息,并对消息进行处理
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public virtual bool OnMessage(T entity, Telegram msg)
    {
        return true;
    }

}