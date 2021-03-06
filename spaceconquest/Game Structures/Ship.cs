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
    class Ship : Unit
    {
        public readonly ShipType shiptype;
        protected int speed = 8;
        [NonSerialized]
        protected Hex3D ghosthex;
        [NonSerialized]
        protected LineModel line;
        [NonSerialized]
        protected ShipModel shipmodel;
        protected float hoveringHeight = 7;
        protected float hoveringAcc = -0.06f;

        protected String modelstring = "StarCruiser";
        private int miningRobots = 0;

        protected double targetangle = 0;
        protected double currentAngle = 0;
        protected Vector3 oldposition = new Vector3(0, 0, 0);
        protected Queue<Vector3> targetpositions = new Queue<Vector3>();
        protected Queue<double> targetangles = new Queue<double>();
        protected Vector3 targetvector = new Vector3(0, 0, 0);
        private int percenttraveled = 0;

        public bool dead = false;

        public void move(Hex3D target)
        {
            hex.RemoveObject();
            //oldposition = hex.getCenter();
            //percenttraveled = 0;

            if (ghosthex != null) ghosthex.ClearGhostObject();
            line = null;
            targetpositions.Enqueue(target.getCenter());

            double x = target.getCenter().X - hex.getCenter().X;
            double y = hex.getCenter().Y - target.getCenter().Y;
            double newangle = Math.Atan(x / y);
            //Console.WriteLine(" x  " + x);
            //Console.WriteLine(" y " + y);
            if (y < 0) newangle = newangle + Math.PI;
            //Console.WriteLine("angle -> " + newangle);
            targetangles.Enqueue(newangle);

            SetHex(target);

        }

        public Ship(ShipType st)
        {
            modelstring = st.modelstring;
            speed = st.speed;
            shiptype = st;
            health = st.shield;
            buildTime = st.buildTime;
            buildCost = st.cost;
            maxHealth = st.shield;
            miningRobots = st.miningRobots;
            
        }

        public void SetMiningRobots(int n)
        {
            this.miningRobots = n;
        }

        public int GetMiningRobots()
        {
            return this.miningRobots;
        }

        public int GetBuildTime()
        {
            return this.buildTime;
        }

        public ShipType GetShipType()
        {
            return shiptype;
        }

        public int getSpeed()
        {
            return speed;
        }

        public override void upkeep()
        {
            base.upkeep();
        }

        public void HopOn(Ship c)
        {
            throw new NotImplementedException();
        }


        public override void kill()
        {
            hex.RemoveObject();
            if (ghosthex != null) ghosthex.ClearGhostObject();
            if (affiliation != null) { affiliation.army.Remove(this); affiliation = null; }
            Console.WriteLine("killed");
        }

        public List<Hex3D> GetReachable()
        {
            List<Hex3D> hexes = reachable(hex, speed);
            hexes.Add(hex);
            foreach (Hex3D h in hexes)
            {
                h.distance = -1;
                if (h.GetGameObject() != null)
                {
                    //hexes.Remove(h);
                }
            }
            hex.distance = -1;
            return hexes;
        }

        List<Hex3D> reachable(Hex3D startHex, int r)
        {
            startHex.distance = r;
            List<Hex3D> hexes = new List<Hex3D>();
            if (r <= 0)
            {
                return hexes;
            }
            foreach (Hex3D h in startHex.getNeighbors())
            {
                //if (!h.passable) continue;
                int dist = h.distance;
                if (dist == -1 || dist < r - 1)
                {
                    hexes.Add(h);
                    hexes.AddRange(reachable(h, r - 1));
                }
            }

            return hexes;
        }

        public void SetGhost(Hex3D target)
        {
            //Console.WriteLine("setting ghost");

            double x = target.getCenter().X - hex.getCenter().X;
            double y = hex.getCenter().Y - target.getCenter().Y;
            targetangle = Math.Atan(x / y);
            //Console.WriteLine(" x  " + x);
            //Console.WriteLine(" y " + y);
            if (y < 0) targetangle = targetangle + Math.PI;
            //Console.WriteLine("angle -> " + targetangle);

            if (ghosthex != null) ghosthex.SetGhostObject(null);
            ghosthex = target;
            target.SetGhostObject(this);
            if (this.hex.hexgrid == ghosthex.hexgrid)
            {
                //Console.WriteLine("creating new linemodel");
                line = new LineModel(getCenter(), ghosthex.getCenter());
            }
        }

       

        private String getprefix()
        {
            foreach (String s in Game1.Races) { Console.WriteLine(s); }
            String foo = Game1.Races[this.affiliation.id];
            //Console.WriteLine(this.affiliation.id + " " + foo);
            return foo + ".";
           
        }

        public override void Draw(Microsoft.Xna.Framework.Matrix world, Microsoft.Xna.Framework.Matrix view, Microsoft.Xna.Framework.Matrix projection)
        {
            if (shipmodel == null) { Console.WriteLine(getprefix() + modelstring); shipmodel = ShipModel.shipmodels[getprefix() + modelstring]; }

            //Console.WriteLine(angle);

            //update current angle, and hmove it closer to the target angle,
            double angleIncr = (Math.PI / 25.0);

            if (Math.Abs(currentAngle - targetangle) > (Math.PI / 20.0))
            {
                //Then we need to modify depending on angle difference
                if (currentAngle < targetangle)
                {
                    currentAngle += angleIncr;
                }
                else
                {
                    currentAngle -= angleIncr;
                }
            }
            else
            {
                if (targetangles.Count() != 0)
                {
                    if (percenttraveled == 100) targetangle = targetangles.Dequeue();
                }

                if (percenttraveled < 100) { percenttraveled = percenttraveled + 4; }
                else
                {
                    if (targetpositions.Count() != 0)
                    {
                        percenttraveled = 0; oldposition = targetvector; targetvector = targetpositions.Dequeue();
                    }
                    else
                    {
                        targetvector = hex.getCenter();
                    }
                }
            }

            
            //Console.WriteLine("percenttaveled :: {0} ", percenttraveled);

            Vector3 currentvector = (targetvector - oldposition) * (percenttraveled / 100.0f) + oldposition;
            //Console.WriteLine("current vector :: {0} \n target vector :: {1} ", currentvector, targetvector);

            //Create translation gets hex world coordinates 
            if (affiliation != null)
            {
                shipmodel.Draw(Matrix.CreateRotationZ((float)currentAngle) * Matrix.CreateTranslation(currentvector) * world, view, projection, affiliation.color, 1.6f, hoveringHeight);
            }
            else
            {
                shipmodel.Draw(Matrix.CreateRotationZ((float)currentAngle) * Matrix.CreateTranslation(currentvector) * world, view, projection, Color.Black, 1.6f, hoveringHeight);
            }
            //create illusion that ship is hovering in space
            hoveringHeight += hoveringAcc;
            if (hoveringHeight > 13 || hoveringHeight < 6) { hoveringAcc *= -1; }
        }

        public override void DrawGhost(Microsoft.Xna.Framework.Matrix world, Microsoft.Xna.Framework.Matrix view, Microsoft.Xna.Framework.Matrix projection)
        {
            if (shipmodel == null) { shipmodel = ShipModel.shipmodels[getprefix() + modelstring]; }
            if (ghosthex == null) { return; }

            shipmodel.Draw(Matrix.CreateRotationZ((float)targetangle) * Matrix.CreateTranslation(ghosthex.getCenter()) * world, view, projection, Color.Multiply(affiliation.color, .2f), 1.6f, hoveringHeight);
            if (this.hex.hexgrid == ghosthex.hexgrid)
            {
                if (line != null) line.Draw(world, view, projection, affiliation.color);
                else line = new LineModel(getCenter(), ghosthex.getCenter());
            }
        }
    }
}
