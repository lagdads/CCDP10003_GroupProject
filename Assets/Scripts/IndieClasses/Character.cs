using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


//该语句可以让自定义类中显示其成员
[Serializable]
public class Character
{
    //人物名称
    public string name;

    //人物立绘是否存在
    public bool havePortrait = true;

    // 人物立绘list
    public List<TachieDialogue> TachieList { get; private set; } = new() ;

    public Character(string name)
    {
        this.name = name;

    }
    //添加立绘
    public void FindTachie(){


        if (!havePortrait )return;
        if (TachieList == null) TachieList = new List<TachieDialogue>();
        var tachieSpriteList = Resources.LoadAll<Sprite>($"Roles/{name}/Tachies/");
        if (tachieSpriteList.Length == 0)
        {
            Debug.LogError($"{name}: No Tachies found in Resources folder!");
            havePortrait = false;
            return;
        }

        foreach (Sprite tachieSprite in tachieSpriteList)
        {
            Debug.LogWarning("Adding Tachie: " + tachieSprite.name);
            TachieList.Add(new TachieDialogue(tachieSprite, tachieSprite.name));
        }
    }

}