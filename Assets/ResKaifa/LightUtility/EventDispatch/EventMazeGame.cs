using System.Collections;
using System.Collections.Generic;
using AmplifyShaderEditor;
using UnityEngine;

public class EventMazeGame
{
    public const int OnGameComplete = 0;
    public const int OnGameFail = 1;
    public const int OnGameStart = 2;
    public const int OnGameRotate = 3;//Update()
    public const int OnShopBallItemClick = 4;
    public const int OnPlayerAssetUpdate = 5;
    public const int OnBallAdd = 6;
    public const int OnStarChestValueUpdate = 7;
    /// <summary>
    /// 强制过关
    /// </summary>
    public const int OnGameForceComplete = 8;

    public const int OnHitGround = 9;
    public const int OnLeaveGround = 10;
    public const int OnVelocityUpdating = 11;
}
