﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spaceconquest
{
    [Serializable]
    class FighterShip : ShipType
    {
        
        public override Ship CreateShip()
        {
            this.modelstring = "Fighter";
            this.speed = 3;
            this.range = 4;
            this.damage = 1;
            this.cost = 100;
            this.shield = 2;
            this.capacity = 0;
            this.buildTime = 2;

            Warship newship = new Warship(this);
            return newship;
        }
        public static ShipType creator = new FighterShip();

        public override void PlaySelectSound()
        {
            Game1.soundEffectBox.PlaySound("Fighter");
        }
    }
}
