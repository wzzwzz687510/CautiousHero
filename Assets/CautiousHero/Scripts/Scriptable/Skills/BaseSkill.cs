using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

public enum DamageType
{
    Physical,
    Magical,
    Pure
}

public enum DamageElement
{
    Fire,
    Water,
    Earth,
    Air,
    Light,
    Dark,
    None
}

[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableSkills/BaseSkill", order = 1)]
public class BaseSkill : ScriptableObject
{
    public string skillName = "New Skill";
    public string description = "A mystical skill";
    public DamageType damageType;
    public DamageElement damageElement;
    public Sprite sprite;

    public int castCost;
    public int affectValue;
    public Location[] castPoints;
    public Location[] affectPoints;
}
