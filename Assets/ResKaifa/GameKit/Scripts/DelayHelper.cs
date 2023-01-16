using System;
using System.Collections.Generic;
using UnityEngine;

namespace ResKaifa.GameKit
{
    public class DelayHelper:SpecSingleton<DelayHelper>
    {
        private Dictionary<string, float> cds = new Dictionary<string, float>();

        /// <summary>
        /// 每n秒执行一次；第一次也会执行。。。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="delay"></param>
        /// <param name="act"></param>
        /// <param name="actAtFirstTime"></param>
        public void Update(string key,float delay,Action act,bool actAtFirstTime = true)
        {
            if (delay <= 0) return;
            if (cds.ContainsKey( key )== false)
            {
                if (actAtFirstTime)
                    act();
                cds.Add(key, delay);
            }
            else
            {
                cds[key] -= Time.deltaTime;
                if (cds[key] <= 0)
                {
                    act();
                    cds.Remove(key);
                }
            }
        }

        public int GetUpdateCount(float delay)
        {
            if (Time.realtimeSinceStartup - _lastUpdateCoun>delay)
            {
                return 1;
            }

            return _updateCount;
        }

        private float _lastUpdateCoun;
        private int _updateCount;
        /// <summary>
        /// 一定事件内计数
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public int UpdateCount(float delay)
        {
            if (Time.realtimeSinceStartup - _lastUpdateCoun > delay)
            {
                _updateCount = 1;
            }
            else
            {
                _updateCount++;
            }

            _lastUpdateCoun = Time.realtimeSinceStartup;
            return _updateCount;
        }

        public void Clear()
        {
            //TODO: 之前有bug才需要清理，现在好像不用的。。。。
            _lastUpdateCoun = 0;
            _updateCount = 0;
        }

    }
}
