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
    class TitleScreen : Screen
    {   
        public List<MenuButton> buttons;
        private MouseState mousestateold;

        public TitleScreen(SpriteBatch sb,SpriteFont sf)
        {
            buttons = new List<MenuButton>();
            buttons.Add(new MenuButton(new Rectangle(Game1.x / 2 - 75, Game1.y / 2 - 100, 150, 40), "Singleplayer", MenuManager.ClickMapSelect));
            buttons.Add(new MenuButton(new Rectangle(Game1.x / 2 - 75, Game1.y / 2 - 50, 150, 40), "Multiplayer", MenuManager.ClickGlobalLobby));
            buttons.Add(new MenuButton(new Rectangle(Game1.x / 2 - 75, Game1.y / 2 + 0, 150, 40), "Sound Options", MenuManager.ClickMusicOptions));
            buttons.Add(new MenuButton(new Rectangle(Game1.x / 2 - 75, Game1.y / 2 + 50, 150, 40), "Exit", Quit));
        }

        public void Quit(Object o, EventArgs e)
        {
            Game1.thisgame.Exit();
        }

        public void Update()
        {
            MouseState mousestate = Mouse.GetState();
            foreach (MenuButton mb in buttons)
            {
                mb.Update(mousestate, mousestateold);
            }
            mousestateold = mousestate;
        }

        public void Draw()
        {
            foreach (MenuButton mb in buttons)
            {
                mb.Draw();
            }


            //h
            DrawBackgroundAction();
            

        }


        //variables for moving asteroid around background
        float zr = 0.0f;
        float x = 0.0f;
        float y = 0.0f;
        int xAcc = 1;
        int yAcc = 1;

        private void DrawBackgroundAction()
        {

            
            Vector3 cameraPosition = new Vector3(0, 0, 30);

            float aspect = Game1.device.Viewport.AspectRatio;

            Matrix world = Matrix.Identity;
            //world = Matrix.CreateTranslation(offset);

           // Matrix view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
            Matrix view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10000);
            AsteroidModel.Draw(world, view, projection, Color.Gray, 10, true, zr);


            //update

             // z rotate for model
            zr += 0.3f;
            if (zr > 360.0f) zr = 0.0f;

            //x position of camera
            if (x < 0) xAcc = 1;
            if (x > 200) xAcc = -1;
            x += xAcc*0.9f;

            if (y < 0) yAcc = 1;
            if (y > 200) yAcc = -1;
            y += yAcc * 0.9f;

        }

    }
}
