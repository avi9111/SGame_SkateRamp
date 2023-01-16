using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ResKaifa.GameKit
{


    public class VecFactory
    {
        public static VecBase CreateVec(string key)
        {
            VecBase vb = null;
            switch (key)
            {
                
                case "rookie":
                case "Rookie":
                    vb = new VecRookie();
                    break;
                case "new":
                case "New":
                case "2023":
                    vb = new Vec2023();
                    break;
                case "skate":
                case "Skate":
                    vb = new VecSkate();//滑板游戏现在用的是这个。。。。。。。
                    break;
                case "hist":
                    vb = new VecHistoryBk();
                    break;
                case "2022":
                default:
                    vb = new VecDefault2022();
                    break;
            }

            return vb;
        }
    }
}