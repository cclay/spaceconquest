using System;
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
    class Sun : GameObject
    {
        public Color defaultcolor = new Color(CommonRNG.rand.Next(250), CommonRNG.rand.Next(250), CommonRNG.rand.Next(20));
        public Color color;
        public Sun(Hex3D h)
        {
            color = defaultcolor;
            SetHex(h);
            h.passable = false;
            h.defaultcolor = Color.Black;

            foreach (Hex3D n in h.getNeighbors())
            {
                n.passable = false;
                n.defaultcolor = Color.Black;

            }
        }

        public bool IsMouseOver(Ray mouseray,Matrix world)
        {
            if (mouseray.Intersects((SphereModel.sphere.Meshes[0].BoundingSphere.Transform( Matrix.CreateScale(80) * world))) != null) { return true; }
            else return false;
        }

        public override void Draw(Matrix world, Matrix view, Matrix projection)
        {
            SphereModel.Draw(Matrix.CreateTranslation(getCenter()) * world, view, projection, color, 80);
        }
    }
}