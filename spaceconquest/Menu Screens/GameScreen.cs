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
    class GameScreen : Screen
    {
        bool host;
        Map map;
        Galaxy galaxy;
        //SolarSystem3D solar; //current solar system
        Space space; //what we're currently looking at, can be galaxy or solarsystem
        Player player; //the player using this screen

        List<MenuComponent> components = new List<MenuComponent>();
        //List<Command> commands = new List<Command>();
        MenuList shipmenu;
        MenuList planetmenu;
        Command.Action clickedaction = Command.Action.None;
        SlaveDriver driver;
        MiddleMan middleman;
        bool waiting = false;
        TextLine waitingmessage;
        IconButton galaxybutton;

        Color selectedcolor = Color.Green;
        Color movecolor = new Color(0,255,0);
        Color mousecolor = new Color(0,255,255);
        public Hex3D selectedhex;
        Hex3D nullhex = new Hex3D(0, 0, null, Color.Gray); //i use this because i dont feel like checking for null on the selected hex

        MouseState mousestate;
        KeyboardState keystate;
        MouseState oldmousestate = Mouse.GetState();
        KeyboardState oldkeystate = Keyboard.GetState();
        Hex3D mousehex;
        Hex3D oldmousehex;
        SolarSystem3D oldmousesystem;

        public static float scrollspeed = 6f;
        public static float rotatespeed = .01f;
        public static float zoomspeed = 10f;
        float xr = 0;
        float yr = 0;
        float zr = 0;
        float height = 700;
        Vector3 offset = new Vector3(0,0,0);

        public GameScreen(bool h, String ipstring, int numclients)
        {
            host = h;
            selectedhex = nullhex;
            if (h) { map = new Map(2, 0, "test galaxy", (long)1); }
            else { map = new Map(2, numclients, "test galaxy", (long)1); }
            galaxy = map.galaxy;
            player = map.GetInstancePlayer();
            space = map.GetHomeSystem();
            driver = new SlaveDriver(map);
            if (host) middleman = new Host(driver, numclients);
            else middleman = new Client(ipstring,driver);

            galaxybutton = new IconButton(new Rectangle(40, Game1.device.Viewport.Height-40, 120, 40), "GalaxyButton.png", "SystemButton.png", GalaxyClick);
            components.Add(galaxybutton);

            shipmenu = new MenuList(new Rectangle(600, 400, 200, 200));
            components.Add(shipmenu);
            shipmenu.Add(new IconButton(new Rectangle(605, 405, 60, 60), "MoveButton.png", MoveClick));
            shipmenu.Add(new IconButton(new Rectangle(670, 405, 60, 60), "AttackButton.png", FireClick));
            //shipmenu.Add(new MenuButton(new Rectangle(605, 470, 60, 60), "Enter", EnterClick));
            shipmenu.Add(new IconButton(new Rectangle(670, 470, 60, 60), "JumpButton.png", JumpClick));
            //shipmenu.Add(new MenuButton(new Rectangle(735, 405, 60, 60), "Upgrade", UpgradeClick));
            shipmenu.Add(new IconButton(new Rectangle(605, 535, 60, 60), "ColonizeButton.png", ColonizeClick));
            //shipmenu.Add(new MenuButton(new Rectangle(605, 405, 60, 60), MenuManager.batch, MenuManager.font, "Build", BuildClick));

            planetmenu = new MenuList(new Rectangle(600, 400, 200, 200));
            components.Add(planetmenu);
            planetmenu.Add(new IconButton(new Rectangle(605, 405, 60, 60), "BuildButton.png", BuildClick));

            waitingmessage = new TextLine(new Rectangle(0, 0, 400, 20), "Waiting for other players.");

            planetmenu.showbackround = false;
            shipmenu.showbackround = false;
        }

        void GalaxyClick(Object o, EventArgs e)
        {
            ((IconButton)o).toggle = true;
            if (space is SolarSystem3D) {space = galaxy;}
            else 
            {
                if (oldmousesystem != null) {space = oldmousesystem;}
                else {space = map.GetHomeSystem();}
            }
        }

        void MoveClick(Object o, EventArgs e) { clickedaction = Command.Action.Move; Console.WriteLine("clicked move"); }
        void FireClick(Object o, EventArgs e) { clickedaction = Command.Action.Fire; }
        void EnterClick(Object o, EventArgs e) { clickedaction = Command.Action.Enter; }
        void JumpClick(Object o, EventArgs e) { clickedaction = Command.Action.Jump; space = galaxy; }
        void UpgradeClick(Object o, EventArgs e) { clickedaction = Command.Action.Upgrade; }
        void ColonizeClick(Object o, EventArgs e) { clickedaction = Command.Action.Colonize; }
        //void BuildClick(Object o, EventArgs e) { clickedaction = Command.Action.Build; }

        void BuildClick(Object o, EventArgs e) { clickedaction = Command.Action.Build; middleman.AddCommand(new Command(selectedhex, selectedhex, Command.Action.Build, new Warship(new Hex3D(0, 0, null, Color.AliceBlue)))); clickedaction = Command.Action.None; }

        public void Update()
        {
            if (middleman.DriverReady()) { middleman.DriverReset(); driver.Execute(); waiting = false; }
            mousestate = Mouse.GetState();
            keystate = Keyboard.GetState();

            if (keystate.IsKeyDown(Keys.Space) && oldkeystate.IsKeyUp(Keys.Space)) { waiting = true;  middleman.EndTurn(); }
            //////camera controls////
            if (keystate.IsKeyDown(Keys.Left)) { offset.X = offset.X + scrollspeed; }
            if (keystate.IsKeyDown(Keys.Right)) { offset.X = offset.X - scrollspeed; }
            if (keystate.IsKeyDown(Keys.Down)) { offset.Y = offset.Y + scrollspeed; }
            if (keystate.IsKeyDown(Keys.Up)) { offset.Y = offset.Y - scrollspeed; }

            //if (keystate.IsKeyDown(Keys.Z)) { xr += rotatespeed; }
            if (keystate.IsKeyDown(Keys.S)) { yr += rotatespeed; }
            if (keystate.IsKeyDown(Keys.D)) { zr += rotatespeed; }
            //if (keystate.IsKeyDown(Keys.A)) { xr -= rotatespeed; }
            if (keystate.IsKeyDown(Keys.W)) { yr -= rotatespeed; }
            if (keystate.IsKeyDown(Keys.A)) { zr -= rotatespeed; }

            if (keystate.IsKeyDown(Keys.Q)) { height += zoomspeed; }
            if (keystate.IsKeyDown(Keys.E)) { height -= zoomspeed; }

            if (yr > 0) { yr = 0; }
            if (yr < (-Math.PI / (float)2)) { yr = (float)(-Math.PI / (float)2); }


            space.Update(); //just resets the color of hexes
            if (space is SolarSystem3D) UpdateSolar();
            if (space is Galaxy) UpdateGalaxy();

            //update the 2d stuff
            foreach (MenuComponent c in components)
            {
                c.Update(mousestate, oldmousestate);
            }

            oldmousestate = mousestate;
            oldmousehex = mousehex;
            oldkeystate = keystate;
        }

        public void UpdateSolar()
        {
            selectedhex.color = selectedcolor;
            ((SolarSystem3D)space).sun.color = ((SolarSystem3D)space).sun.defaultcolor;

            ////////mouse stuff////////
            if (oldmousehex != null && !oldmousehex.Equals(selectedhex)) oldmousehex.color = oldmousehex.defaultcolor;

            mousehex = ((SolarSystem3D)space).GetMouseOverHex();


            if ((mousestate.LeftButton == ButtonState.Pressed) && (oldmousestate.LeftButton == ButtonState.Released) && !shipmenu.Contains(mousestate.X, mousestate.Y) && !planetmenu.Contains(mousestate.X, mousestate.Y) && !galaxybutton.Contains(mousestate.X, mousestate.Y) && mousehex != null)
            {
                //selecting a hex
                if (clickedaction == Command.Action.None)
                {
                    selectedhex = mousehex;
                }

                //selecting a target of a action
                if (clickedaction != Command.Action.None && !waiting) 
                {
                    Console.WriteLine("a command");
                    if (selectedhex.GetGameObject() is Ship && clickedaction == Command.Action.Move)
                    {
                        if (!((Ship)(selectedhex.GetGameObject())).GetReachable().Contains(mousehex)) { return; }
                        ((Ship)(selectedhex.GetGameObject())).SetGhost(mousehex);
                    }
                    if (selectedhex.GetGameObject() is Warship && clickedaction == Command.Action.Fire)
                    {
                        if (!((Warship)(selectedhex.GetGameObject())).GetShootable().Contains(mousehex)) { return; }
                        //((Ship)(selectedhex.GetGameObject())).SetGhost(mousehex);
                    }

                    if (selectedhex.GetGameObject() is Warship && clickedaction == Command.Action.Colonize)
                    {
                        if (!((Warship)(selectedhex.GetGameObject())).GetReachable().Contains(mousehex)) { return; }
                        //((Ship)(selectedhex.GetGameObject())).SetGhost(mousehex);
                    }

                    if (selectedhex.GetGameObject() is Ship && clickedaction == Command.Action.Jump)
                    {
                        if (!((SolarSystem3D)(space)).GetWarpable().Contains(mousehex)) { return; }
                        ((Ship)(selectedhex.GetGameObject())).SetGhost(mousehex);
                    }

                    middleman.AddCommand(new Command(selectedhex, mousehex, clickedaction));
                    clickedaction = Command.Action.None;
                    selectedhex = nullhex;
                }
            }


            /////stuff to do if a ship is selected/////
            GameObject selectedobject = selectedhex.GetGameObject();

            if (selectedhex.GetGameObject() is Ship && clickedaction == Command.Action.Jump)
            {
                foreach (Hex3D h in ((SolarSystem3D)space).GetWarpable())
                {
                    h.color = Color.Blue;
                }
            }

            if (selectedobject != null && selectedobject is Ship && ((Ship)selectedobject).getAffiliation().Equals(player)) { shipmenu.Show(); }
            else { shipmenu.Hide(); }

            if (selectedobject != null && selectedobject is Warship)
            {
                foreach (Hex3D h in ((Warship)selectedobject).GetShootable())
                {
                    h.color = Color.Red;
                }
            }
            if (selectedobject != null && selectedobject is Ship)
            {
                foreach (Hex3D h in ((Ship)selectedobject).GetReachable())
                {
                    h.color = movecolor;
                }
            }

            //planetmenu
            if (selectedobject != null && selectedobject is Planet && ((Planet)selectedobject).getAffiliation() != null && ((Planet)selectedobject).getAffiliation().Equals(player)) { planetmenu.Show(); }
            else { planetmenu.Hide(); }


            //Color the mousedover hex
            if (mousehex != null && !mousehex.Equals(selectedhex) && !shipmenu.Contains(mousestate.X, mousestate.Y)) { mousehex.color = mousecolor; }

        }

        public void UpdateGalaxy()
        {
            SolarSystem3D mousesystem = galaxy.GetMouseOverSystem();
            if (oldmousesystem != null) oldmousesystem.sun.color = oldmousesystem.sun.defaultcolor;
            if (mousesystem != null) mousesystem.sun.color = Color.Green;

            if ((mousestate.LeftButton == ButtonState.Pressed) && (oldmousestate.LeftButton == ButtonState.Released) && !shipmenu.Contains(mousestate.X, mousestate.Y) && mousesystem != null)
            {
                if (clickedaction == Command.Action.Jump)
                {
                    //commands.Add(new Command(selectedhex, mousesystem.getHex(-2, -2), Command.Action.Jump));
                    space = mousesystem;
                    //selectedhex = nullhex;
                    clickedaction = Command.Action.Jump;
                }
                else { space = mousesystem; }
            }

            oldmousesystem = mousesystem;
        }

        public void Draw()
        {
            
            space.Draw(offset,xr,yr,zr,height);

            //2D stuff
            foreach (MenuComponent c in components)
            {
                c.Draw();
            }

            if (waiting) { waitingmessage.Draw(); }
        }
    }
}