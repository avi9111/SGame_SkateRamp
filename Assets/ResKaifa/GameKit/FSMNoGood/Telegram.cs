using System.Collections.Generic;

public class Telegram {
    //发送消息的实体(entity)的id
    public int Sender;
    //处理消息的实体(entity)的id
    public int Receiver;
    //一个enum类型的消息类型，实体(entity)根据不同的消息类型进行不同的处理
    public MessageTypes Msg;
    //时间戳，当时间到达该时间戳，消息才会分发出去
    public float DispatchTime;
    //消息附带的额外信息，这个可以根据需求自定义，也可以为null
    public object ExtraInfo;

    public Telegram()
    {
            
    }
    
    public Telegram(MessageTypes msg, int s_id, int r_id, float send_time, object info)
    {
        Msg = msg;
        Sender = s_id;
        Receiver = r_id;
        DispatchTime = send_time;
        ExtraInfo = info;
    }
}

//Telegram的比较规则类，消息队列里的Telegram按照时间戳进行排序
public class TelegramCompare : IComparer<Telegram>
{
    public int Compare(Telegram a, Telegram b)
    {
        return a.DispatchTime.CompareTo(b.DispatchTime);
    }
}

//以下是我自定义的一些消息类型
public enum MessageTypes
{
    Msg_Shooted, // 被子弹击中
    Msg_HeroHurt,
    Msg_EnemeyAnimationHurt,
    Msg_PlayerNormalAttack,//玩家普通攻击
    Msg_PlayerSkillHurt, //玩家技能一伤害
    Msg_PlayerAddHp,  // 使用药品
    Msg_PlayerAddMp,
    Msg_KnockBack,      // 震退信息
//    Msg_PlayerSkillHurt_2,  //玩家技能二伤害
    Msg_UseNuQi,        //使用怒气
    Msg_EnemyDie        // 敌人死亡
}