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
    class SolarSystem3D
    {
        Rectangle screenarea;

        Hex3D[,] hexmap;
        List<Hex3D> hexlist;

        int radius;
        int hexcount;

        Plane plane;

        Matrix world;
        Matrix view;
        Matrix projection;

        public SolarSystem3D(int r, Rectangle sa)
        {
            screenarea = sa;
            radius = r;
            plane = new Plane(0, 0, 1, 1);

            hexmap = new Hex3D[(radius * 2) + 1, (radius * 2) + 1];

            hexcount = HowManyHexes(radius);
            hexlist = new List<Hex3D>(hexcount);

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    if (Math.Abs(i + j) > radius) { continue; }
                    Hex3D temp = new Hex3D(i, j, this);
                    hexlist.Add(temp);
                    hexmap[i + radius, j + radius] = temp;
                }
            }

            //BUIDIN' THA MOTHAF**KIN' SUN. 
            new Warship(getHex(-1, -1));
            new Sun(getHex(0, 0));
            new Planet("Earth",Color.Blue,getHex(3, 3));
            getHex(0,0).passable = false;

            // -1, 1; 0, 1; 0, -1; 1, 0;, -1,0; 1,-1;
            List<Point> positions = new List<Point>(6);
            positions.Add(new Point(-1, 1));
            positions.Add(new Point(1, -1));
            positions.Add(new Point(0, 1));
            positions.Add(new Point(0, -1));
            positions.Add(new Point(-1, 0));
            positions.Add(new Point(1, 0));

            Console.WriteLine("Positions Made");

            foreach (Point pos in positions) {
                getHex(pos.X, pos.Y).passable = false; 
            }

            //Console.WriteLine("Starting HexList Loop");
            //foreach (Hex3D h in hexlist)
            //{
            //    Hex3D neighbor;
            //    Console.WriteLine("Starting HexList SubLoop for Hex " + h.x + ", ", +h.y);
            //    foreach (Point pos in positions)
            //    {
            //        Console.WriteLine("Testing hex " + (pos.X + h.x) + ", " + (pos.Y + h.y) + " as a neighbor of " + h.x + ", " + h.y);
            //        try
            //        {
            //            neighbor = getHex(h.x + pos.X, h.y + pos.Y);
            //            if (neighbor.passable)
            //            {
            //                h.getNeighbors().Add(neighbor);
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            //NOOP
            //        }
            //    }

            //}

        }

        private int HowManyHexes(int x)
        {
            if (x < 0) return 0;
            if (x == 0) return 1;
            else return (x * 6) + HowManyHexes(x - 1);
        }


        public Hex3D getHex(int x, int y)
        {
            if (x+radius >= ((radius * 2) + 1)) { return null; }
            if (y+radius >= ((radius * 2) + 1)) { return null; }
            if (x+radius < 0) { return null; }
            if (y+radius < 0) { return null; }

            if (hexmap[x + radius, y + radius] == null) { return null; }
            return hexmap[x + radius, y + radius];
        }

        public void Update()
        {
            foreach (Hex3D h in hexlist)
            {
                h.color = Hex3D.hexcolor;
            }
        }

        public Hex3D GetMouseOverHex()
        {
            MouseState mousestate = Mouse.GetState();
            Point mouseposition = new Point(mousestate.X, mousestate.Y);
            Viewport viewport = Game1.device.Viewport;

            Vector3 near = new Vector3(mouseposition.X, mouseposition.Y, 0);
            Vector3 far = new Vector3(mouseposition.X, mouseposition.Y, 1);
            near = viewport.Unproject(near, projection, view, world);
            far = viewport.Unproject(far, projection, view, world);


            Vector3 direction = far - near;
            direction.Normalize();
            Ray mouseray = new Ray(near, direction);

            float? nintersection = mouseray.Intersects(plane);
            if (!nintersection.HasValue) { return null; }
            float intersection = nintersection.Value;


            mouseray = new Ray(((direction * intersection) + near), Vector3.Zero); //this "ray" is now just the point where the mouseray intersects the plane

            foreach (Hex3D h in hexlist)
            {
                if (h.IsMouseOver(mouseray)) return h;
            }

            return null;
        }

        public void Draw(Vector3 offset, float xr, float yr, float zr, float height)
        {
            Vector3 cameraPosition = new Vector3(0, 0, height);
            
            float aspect = Game1.device.Viewport.AspectRatio;

            world = Matrix.Identity;
            //world = Matrix.CreateTranslation(offset);

            view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
            view = Matrix.CreateFromYawPitchRoll(xr, yr, zr) * Matrix.CreateTranslation(offset) * view;

            projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10000);
            //projection = Matrix.CreateOrthographic(800, 600, 1, 20);


            foreach (Hex3D h in hexlist)
            {

                h.DrawObject(world, view, projection);
            }


            foreach (Hex3D h in hexlist)
            {
                //if (screenarea.Contains((int)h.getCenter().X, (int)h.getCenter().Y)) { h.Draw(world, view, orthog, color); }
                h.Draw(world, view, projection);
            }

           

            //draw menu stuff here
        }
    }
}
