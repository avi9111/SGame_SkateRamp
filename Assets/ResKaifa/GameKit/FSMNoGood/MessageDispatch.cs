public class MessageDisPatcher {
    /// <summary>
    /// msg: 消息类型
    /// senderId: 发送者ID
    /// receiverId: 接受者ID
    /// delay: 延迟时间,例如,delay = 2 表示延迟 2 秒再发送
    /// info: 该消息所附带的额外信息,可以是任何类型
    /// </summary>
    public void DispatchMesssage(MessageTypes msg, int senderId, int receiverId, float delay, object info = null)
    {
        //npc给自己发一条逃跑的消息，额外信息为null
        //MessageDisPatcher.Instance.DisPatchMessage(Run_Away, npc, npc, 0f, null);
    }

}