using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MyIK))]
public class MyIKCharacterEditor : Editor
{
    private MyIK _target;
    private void Awake()
    {
        _target = target as MyIK;
        
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        //做了一个功能，后来发现 根本已经有的功能


        base.OnInspectorGUI();
    }
}
