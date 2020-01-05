﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public abstract class ArrowReturnBuff : StackableBuff
    {
        public new bool stackable = true;

        public override void ApplyEffect(BuffHandler bh)
        {
            // Add arrow to caster
        }
    }
}

