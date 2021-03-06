﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spaceconquest
{
    [Serializable]
    class Transport : ShipType
    {
        public static Transport creator = new Transport(); //i realize this is silly, but i couldnt figure out a compile time way to pass classes as parameters

        public override Ship CreateShip()
        {
            this.modelstring = "Transport";
            
            this.speed = 4;
            this.range = 1;
            this.damage = 1;
            this.cost = 100;
            this.shield = 2;
            this.capacity = 6;
            this.buildTime = 3;
           
            this.canenter = false;
            this.canjump = true;

            Carrier newship = new Carrier(this);
            return newship;
        }

        public override void PlaySelectSound()
        {
            Game1.soundEffectBox.PlaySound("Transport");
        }
    }
}
