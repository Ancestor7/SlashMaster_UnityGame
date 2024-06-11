using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public int id;
    public string itemName;
    public string itemDesc;
    public int itemCost;
    public Sprite sprite;
    public bool active = false;

    public Item(int id)
    {
        this.id = id;

        switch (id)
        {
            case 0:
                itemName = "Empty";
                itemDesc = "";
                itemCost = 0;
                break;
            case 1:
                itemName = "Potion";
                itemDesc = "Heal Hp";
                itemCost = 3;
                sprite = Resources.Load<Sprite>("Potion");
                break;
            case 2:
                itemName = "Attack+";
                itemDesc = "Atk+ for 10s";
                itemCost = 5;
                sprite = Resources.Load<Sprite>("AtkPotion");
                break;
            case 3:
                itemName = "Defence+";
                itemDesc = "Def+ for 10s";
                itemCost = 5;
                sprite = Resources.Load<Sprite>("DefPotion");
                break;
            case 4:
                itemName = "Shield";
                itemDesc = "Gain shield";
                itemCost = 10;
                sprite = Resources.Load<Sprite>("Protection");
                break;
            case 5:
                itemName = "Escape";
                itemDesc = "Run away!";
                itemCost = 10;
                sprite = Resources.Load<Sprite>("Run");
                break;
            case 6:
                itemName = "Time Magic";
                itemDesc = "Slows Time";
                itemCost = 25;
                sprite = Resources.Load<Sprite>("TimeMagic");
                break;
            case 7:
                itemName = "Calm Spell";
                itemDesc = "Calms enemy";
                itemCost = 25;
                sprite = Resources.Load<Sprite>("CalmMagic");
                break;
            case 8:
                itemName = "Insta DMG";
                itemDesc = "Deal damage";
                itemCost = 35;
                sprite = Resources.Load<Sprite>("InstaDamage");
                break;
            case 9:
                itemName = "Insta Kill";
                itemDesc = "Kills enemy!";
                itemCost = 50;
                sprite = Resources.Load<Sprite>("InstaKill");
                break;
            case 10:
                itemName = "Revive";
                itemDesc = "Revives on death";
                itemCost = 50;
                sprite = Resources.Load<Sprite>("ReviveCrystal");
                break;
            case 11:
                itemName = "Immunity Spell";
                itemDesc = "Immune to damage for 10s";
                itemCost = 50;
                sprite = Resources.Load<Sprite>("ImmunitySpell");
                break;
        }
    }
}
