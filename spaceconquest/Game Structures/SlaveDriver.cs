using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spaceconquest
{
    class SlaveDriver
    {
        Map map;
        Galaxy galaxy;
        HashSet<Command> commands = new HashSet<Command>(); //hashset so that we ignore multiple commands to one unit
        List<QueuedCommand> qcs = new List<QueuedCommand>();
        GameScreen gamescreen;
        //Player player;

        public SlaveDriver(GameScreen gs) { gamescreen = gs; }

        public SlaveDriver(Map m)
        {
            map = m;
            galaxy = map.galaxy;
        }

        public void SetMap(Map m)
        {
            map = m;
            galaxy = map.galaxy;
        }

        public Map GetMap()
        {
            return map;
        }

        public void Receive(List<Command> cl)
        {
            Console.WriteLine("Received " + cl.Count + " Commands");
            cl.Reverse(); //reverse the list because we want the most recent command to each unit to be the one recorded
            foreach (Command c in cl)
            {
                commands.Add(c);
            }
        }

        public void Execute()
        {
            foreach (Command C in commands) {
                if (C.action != Command.Action.Move)
                {
                    qcs.Add(new QueuedCommand(C, galaxy, 0));
                }
                else if (C.action == Command.Action.Move)
                {
                    qcs.AddRange(QueuedCommand.QueuedCommandList(C, galaxy));
                }
                /*else {
                    Ship agent = (Ship)((galaxy.GetHex(C.start)).GetGameObject());
                    int speed = agent.getSpeed();
                    int i = 0;
                    for (; i < speed; i++) { 
                        qcs.Add(new QueuedCommand(agent, galaxy.GetHex(C.target), i));
                    }
                    qcs.Add(new QueuedCommand(C, galaxy, i));
                }*/
                
            }

            qcs.Sort(Sorter);

            Console.WriteLine("Executing " + qcs.Count + " Commands");
            foreach (QueuedCommand qc in qcs)
            {
                ExecuteCommand(qc);
            }
            commands.Clear();
            qcs.Clear();
            Console.WriteLine("Done executing");
            Boolean iLost = true;
            Boolean iWon = true;
            Player p1 = map.GetInstancePlayer();
            foreach (Player p in map.players)
            {
                Console.WriteLine("Player " + p.id);
                List<Unit> newlist = new List<Unit>();
                newlist.AddRange(p.army);
                
                foreach (Unit u in newlist)
                {
                    Console.WriteLine("Unit " + u.hex);
                    if (!(u is Asteroid)) {
                        if (iLost && p == p1)
                        {
                            iLost = false;
                        }
                        else if (iWon && p != p1) {
                            iWon = false;
                        }
                    }
                    u.upkeep();
                }

            }
            if (iLost)
            {
                Console.WriteLine("I LOST THE GAME " + map.players.Count);
                //Lose screen
                MenuManager.screen = new WinScreen(false);
                gamescreen.middleman.Close();
                gamescreen.middleman.AttendClose();
            }
            if (iWon)
            {
                Console.WriteLine("I WON THE GAME " + map.players.Count);
                //Win screen
                MenuManager.screen = new WinScreen(true);
                gamescreen.middleman.Close();
                gamescreen.middleman.AttendClose();
            }
        }

        private int Sorter(QueuedCommand qc1, QueuedCommand qc2) { return qc1.priority-qc2.priority; }


        private List<Hex3D> Pathfinder(Hex3D s, Hex3D d) {
            Console.WriteLine(s.ToString() + ", " + d.ToString());
            //Assume d is occupable. 
            if (s == d) {
                List<Hex3D> ret = new List<Hex3D>();
                ret.Add(d);
                return ret;
            }
            List<Hex3D> path = new List<Hex3D>();
            HashSet<Hex3D> HexFrontier = new HashSet<Hex3D>();
            HashSet<Hex3D> ExploredHexes = new HashSet<Hex3D>();
            HashSet<Hex3D> OldHexFrontier = new HashSet<Hex3D>();
            ExploredHexes.Add(d);
            OldHexFrontier.Add(d);
            d.distance = 0;
            while (true) { 
                foreach (Hex3D h1 in OldHexFrontier) {
                    //Console.WriteLine("h1 is " + h1.ToString());
                    //Console.WriteLine("h1 dist is " + h1.distance);
                    foreach(Hex3D h2 in h1.getNeighbors()) {
                        //Console.WriteLine("h2 is " + h2.ToString());
                        //Console.WriteLine("h2 dist is " + h2.distance);
                        if (h2 == s)
                        {
                            Console.WriteLine("Victory!\n"+h2.distance + " vs. " + h1.distance);
                        }
                        if ((h2.distance < 0 || h2.distance > h1.distance + 1) && (h2.GetGameObject() == null || s == h2)) {
                            h2.distance = h1.distance+1;
                            h2.prev = h1;
                            HexFrontier.Add(h2);
                            ExploredHexes.Add(h2);
                        }
    
                    }
                }
                if (HexFrontier.Contains(s))
                    break;
                if (HexFrontier.Count == 0) {
                    foreach (Hex3D h in ExploredHexes)
                    {
                        h.distance = -1;
                        h.prev = null;
                    }
                    return null;
                }
                OldHexFrontier = HexFrontier;
                HexFrontier = new HashSet<Hex3D>();
            }
            path.Add(s);
            Hex3D cursor = s;
            while (cursor != d) {
                cursor = cursor.prev;
                path.Add(cursor);
            }
            if (d.GetGameObject() != null) { 
                path.Remove(d);
            }
            foreach (Hex3D h in ExploredHexes) {
                h.distance = -1;
                h.prev = null;
            }
            return path;
        }     

        private bool ExecuteCommand(QueuedCommand c)
        {
            if (c.agent.getAffiliation() == null) { return false; }
            Console.WriteLine(c.ToString());
            Console.WriteLine(c.priority);

            if (c.order == Command.Action.Move)
            {
                Console.WriteLine("MOVE");
                Console.WriteLine("Recognized Move");
                if (c.agent != null && c.agent is Ship)
                {
                    Console.WriteLine("Recognized Ship");
                    /*if (c.targetHex.GetGameObject() != null) { return false; }
                    ((Ship)c.agent).move(c.targetHex);
                    return true;*/
                    //Need to calculate path to target, move one space towards it. 
                    int diff = Math.Abs(c.targetHex.x - c.agent.hex.x) + Math.Abs(c.targetHex.y - c.agent.hex.y);
                    List<Hex3D> path = Pathfinder(c.agent.hex, c.targetHex);
                    if (path == null) {
                        Console.WriteLine("null path");
                        //No path. Wait?
                    }
                    else if (path.Count <= 1)
                    {
                        Console.WriteLine("already there");
                        //Do nothing. Already there.
                    }
                    else {
                        Console.WriteLine("moving");
                        ((Ship)(c.agent)).move(path[1]);
                    }
                    
                }
            }
            if (c.order == Command.Action.Fire)
            {
                if (c.agent != null && c.agent is Warship)
                {
                    if (c.targetHex.GetGameObject() == null) { return false; }
                    ((Warship)c.agent).Attack((Unit)c.targetHex.GetGameObject()); //i should probly check that the target is a unit
                    return true;
                }
            }
            if (c.order == Command.Action.Jump)
            {
                
                if (c.agent != null && c.agent is Ship)
                {
                    Hex3D newTarget = null;
                    if (c.targetHex.GetGameObject() != null)
                    {
                        //Best effort.
                        HashSet<Hex3D> attemptedHexes = new HashSet<Hex3D>();
                        HashSet<Hex3D> NewFrontier = new HashSet<Hex3D>();
                        HashSet<Hex3D> OldFrontier = new HashSet<Hex3D>();
                        attemptedHexes.Add(c.targetHex);
                        NewFrontier.Add(c.targetHex);

                        while (newTarget == null)
                        {
                            OldFrontier = NewFrontier;
                            NewFrontier = new HashSet<Hex3D>();
                            foreach (Hex3D h1 in OldFrontier)
                            {
                                foreach (Hex3D h2 in h1.getNeighbors().Except(attemptedHexes))
                                {
                                    attemptedHexes.Add(h2);
                                    if (h2.hexgrid.GetWarpable().Contains(h2))
                                    {
                                        NewFrontier.Add(h2);
                                        if (h2.GetGameObject() == null)
                                        {
                                            newTarget = h2;
                                            break;
                                        }
                                    }
                                }
                                if (newTarget != null) { break; }
                            }
                        }
                    }
                    else {
                        newTarget = c.targetHex;
                    }
                    if (!newTarget.hexgrid.neighbors.Contains(c.agent.hex.hexgrid)) {
                        return false;
                    }

                    foreach (Hex3D h3 in newTarget.hexgrid.getHexes()) {
                        h3.visible = true;
                    }
                    ((Ship)c.agent).move(newTarget);
                    return true;
                }
            }
            if (c.order == Command.Action.Build)
            {
                if (c.agent != null && c.agent is Planet)
                {
                    //if (!c.agent.getAffiliation().payMetal(c.shiptype.)) { return false; }
                    ((Planet)c.agent).build(c.shiptype.CreateShip());
                    return true;
                }
            }

            if (c.order == Command.Action.Upgrade)
            {
                if (c.agent != null && c.agent is Planet)
                {
                    if (((Planet)c.agent).getMaxHealth() >= 4) { return false; }
                    ((Planet)c.agent).build(Shield.creator.CreateShip());
                    return true;
                }
            }

            if (c.order == Command.Action.Colonize)
            {
                if (c.agent != null && c.agent is Ship)
                {
                    if (c.targetHex.GetGameObject() is Planet && ((Ship)c.agent).shiptype is ColonyShip)
                    {
                        if (((Planet)c.targetHex.GetGameObject()).getAffiliation() != null) { return false; }
                        if (!c.targetHex.getNeighbors().Contains(c.agent.hex)) { return false; }
                        ((Planet)c.targetHex.GetGameObject()).setAffiliation(((Ship)c.agent).getAffiliation());
                        c.agent.kill();
                        return true;
                    }
                    if (c.targetHex.GetGameObject() is Asteroid && ((Ship)c.agent).shiptype is MiningShip)
                    {
                        if (((Asteroid)c.targetHex.GetGameObject()).getAffiliation() != null) { return false; }
                        if (!c.targetHex.getNeighbors().Contains(c.agent.hex)) { return false; }

                        Ship s = (Ship)c.agent;
                        int robots = s.GetMiningRobots();
                        if (robots > 0)
                        {
                            ((Asteroid)c.targetHex.GetGameObject()).setAffiliation(((Ship)c.agent).getAffiliation());
                            s.SetMiningRobots(robots - 1);
                            //don't kill, just decrease mining bot
                            //c.agent.kill();
                            return true;
                        }
                    }
                }
            }

            if (c.order == Command.Action.Enter)
            {
                Console.WriteLine("ENTER");
                if (c.agent != null && c.agent is Carrier)
                {
                    ((Carrier)c.agent).UnloadAll();
                    return true;
                }
                else if (c.agent != null && c.agent is Ship)
                {
                    GameObject target = c.targetHex.GetGameObject();
                    if (target is Carrier) {
                        Console.WriteLine("Attempt Enter");
                        return ((Carrier)target).LoadShip((Ship)(c.agent));
                    }
                    if (target == null) {
                        Console.WriteLine("null Target");
                    }
                    return false;
                }
            }

            return false;
        }

    
    }
}
