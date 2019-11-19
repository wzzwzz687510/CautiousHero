﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Pattern", menuName = "Wing/Scriptable Patterns/Effect Pattern", order = 1)]
    public class ScriptableEffectPattern : ScriptableObject
    {
        public EffectPattern[] effectPatterns;
    }
}
