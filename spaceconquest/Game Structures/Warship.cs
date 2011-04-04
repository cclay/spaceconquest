﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace spaceconquest
{
    [Serializable]
    class Warship : Ship
    {
        protected int damage = 1;
        protected int range = 3;

        public Warship(String s) : base(s)
        {
            modelstring = s;
            shipmodel = ShipModel.shipmodels[modelstring];
        }

        public void Attack(Unit u)
        {
            u.hit(damage);
        }

        public List<Hex3D> GetShootable()
        {
            HashSet<Hex3D> hexes = new HashSet<Hex3D>();
            Shootable(hexes, hex, range);
            hexes.Remove(hex);
            return hexes.ToList();
        }

        private void Shootable(HashSet<Hex3D> hexes, Hex3D h, int r)
        {
            if (r <= 0) return;
            foreach (Hex3D n in h.getNeighbors())
            {
                hexes.Add(n);
                Shootable(hexes, n, r - 1);
            }
        }
        
        //public override void Draw(Microsoft.Xna.Framework.Matrix world, Microsoft.Xna.Framework.Matrix view, Microsoft.Xna.Framework.Matrix projection)
        //{
        //    if (shipmodel == null) { shipmodel = ShipModel.shipmodels[modelstring]; }

        //    shipmodel.Draw(Matrix.CreateTranslation(getCenter()) * world, view, projection, affiliation.color, 1.6f, hoveringHeight);

        //     //create illusion that ship is hovering in space
        //     hoveringHeight += hoveringAcc;
        //     if (hoveringHeight > 13 || hoveringHeight < 6) { hoveringAcc *= -1; }
        //}

        
    }
}
