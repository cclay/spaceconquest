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
using System.Net;
using System.IO;

namespace spaceconquest
{
    class GlobalLobbyScreen : Screen
    {
        public List<MenuComponent> buttons;
        private MouseState mousestateold = Mouse.GetState();
        private GlobalChatClient chatClient;
        MenuList chatlist;
        TextInput ipbox;

        public GlobalLobbyScreen(SpriteBatch sb, SpriteFont sf)
        {
            chatClient = new GlobalChatClient("http://openportone.appspot.com/");
            buttons = new List<MenuComponent>();
            ipbox = new TextInput(new Rectangle(Game1.x / 2 + 50, Game1.y / 2 - 200, 150, 40), Nothing); //not addded to the list

            buttons.Add(new MenuButton(new Rectangle(Game1.x / 2 + 50, Game1.y / 2 - 150, 150, 40), "Join Game", JoinGame));
            buttons.Add(new MenuButton(new Rectangle(Game1.x / 2 + 50, Game1.y / 2 - 100, 150, 40), "Host Game", HostGame));
            buttons.Add(new MenuButton(new Rectangle(Game1.x / 2 + 50, Game1.y / 2 - 50, 150, 40), "Quit", MenuManager.ClickTitle));


            //buttons.Add(new TextInput(new Rectangle(50,500,350,40), ChatSend));
            chatlist = new MenuList(new Rectangle(Game1.x / 2 - 350, Game1.y / 2 - 250, 350, 500));
            chatlist.padding = 0;
            buttons.Add(chatlist);
        }

        public void Nothing(String input){}

        public void HostGame(Object o, EventArgs e)
        {
            String rep = "";
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ip in localIPs) {
                rep = ip.ToString();
                if (rep.Contains(".")) {
                    Console.WriteLine(rep);
                    break;
                }
            }

            ChatSend(rep + "-" + String.Format("{0:yyyy|MM|dd|HH|mm|ss}", DateTime.Now));
            //MenuManager.ClickHost("127.0.0.1", EventArgs.Empty);
            MenuManager.screen = new MapSelectScreen(true);
        }

        public void JoinGame(Object o, EventArgs e)
        {
            try
            {
                MenuManager.ClickClientConnect(ipbox.input, EventArgs.Empty);
            }
            catch (Exception ex) {
                ipbox.input = "";
            }
            
        }

        public void ChatSend(String input)
        {
            chatClient.SendMessage("me", input); 
            chatClient.UpdateLocalLogList();
        }

        public void Update()
        {
            MouseState mousestate = Mouse.GetState();
            //ipbox.Contains(mousestate.X, mousestate.Y)
            if (true) { ipbox.Update(mousestate, mousestateold); }
            //else
           // {
                foreach (MenuComponent mb in buttons)
                {
                    mb.Update(mousestate, mousestateold);
                }
           // }
            mousestateold = mousestate;

            chatlist.Clear();
            List<ChatLog> newChats = chatClient.GetLogs();

            foreach (ChatLog c in newChats)
            {
                //string finalMessage = "";
                
                //bool nameSaid = false;
                /*while (.Length > 31)
                {
                    
                    //finalMessage += init.Substring(0, 25) + "\n";
                    //init = init.Substring(26);
                    finalMessage = init.Substring(0, 31) + "\n";
                    if (nameSaid == false)
                    {
                        chatlist.AddNewTextLineDefault(20, 200, c.playerName + finalMessage);
                    }
                    else
                    {
                        chatlist.AddNewTextLineDefault(20, 200, "          " + finalMessage);
                    
                    }
                    nameSaid = true;
                    
                    init = init.Substring(31);
                }*/
                /*if (nameSaid == false)
                {
                    chatlist.AddNewTextLineDefault(20, 200, c.playerName + init);
                }
                else
                {
                    chatlist.AddNewTextLineDefault(20, 200, "          " + init);

                }*/
                //Console.WriteLine("\n\n\n\n"+c.message+"\n\n\n\n");
                String ipa = c.message.Split('-')[0];
                chatlist.AddNewTextLineDefault(20, 200, "Game Hosted at " + ipa);

            }
        }

        public void Draw()
        {
            foreach (MenuComponent mb in buttons)
            {
                mb.Draw();
            }
            ipbox.Draw();
        }
    }
}
