using System;
using UnityEngine;


[Serializable]
public class TachieDialogue
{
    public TachieDialogue(Sprite tachie,string name)
    {
        this.tachie = tachie;
        this.name = name;
    }

    public Sprite tachie;
    public string name;
    public bool isShadow;
}