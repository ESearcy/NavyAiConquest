using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace SEMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Driver : MySessionComponentBase
    {


        private static String _logPath = "Driver";
        private int _ticks = 0;
        private bool _initalized = false;
        private DateTime _lastSaveTime;
        Dictionary<long, List<Ship>> assets = new Dictionary<long, List<Ship>>();
        NavyController navy = new NavyController();

        public override void UpdateBeforeSimulation()
        {
            Util.debuggingOn = true;
            try
            {
                if (_ticks % 10 == 0)
                {
                    Util.SaveLogs();
                    _lastSaveTime = DateTime.Now;
                }

                if (_ticks%1 == 0)
                {
                    if (_initalized)
                    {
                        ClearHud();
                        FindAllDrones();
                        navy.Update();
                    }
                    else
                    {
                        Setup();
                    }
                }



                //TimeSpan span = DateTime.Now.Subtract(_lastSaveTime);

                

                _ticks++;
            }
            catch (Exception e)
            {
                Util.Log(_logPath, e.ToString());
            }
        }

        public void ClearHud()
        {
            long id = MyAPIGateway.Session.Player.IdentityId;
            foreach (var gps in MyAPIGateway.Session.GPS.GetGpsList(id))
            {
                MyAPIGateway.Session.GPS.RemoveGps(id, gps);
            }
        }

        public void FindAllDrones()
        {
            HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
            List<IMyPlayer> players = new List<IMyPlayer>();

            try
            {
                MyAPIGateway.Entities.GetEntities(entities);
                MyAPIGateway.Players.GetPlayers(players);
            }
            catch (Exception e)
            {
                Util.LogError(_logPath, e.ToString());
                return;
            }
            

            //filter out any grids that are already accounted for
            foreach (IMyEntity entity in entities.Where(x => x is IMyCubeGrid))
            {
                if (assets.All(x => !x.Value.Any(y=>y._cubeGrid==entity)))
                {
                    if (!entity.Transparent)
                    {
                        Util.Log(_logPath,"sending grid to be analyzed");
                        SetUpDrone(entity);
                    }
                }
            }
        }

        private void AddDiscoveredShip(Ship ship)
        {

            if (assets.Keys.Contains(ship.GetOwnerId()))
            {
                assets[ship.GetOwnerId()].Add(ship);
                Util.Log(_logPath, "[AddSpacePirate] squad existed: drone added!");
            }
            else
            {
                assets.Add(ship.GetOwnerId(), new List<Ship>());
                Util.Log(_logPath, "[AddSpacePirate] squad created: drone added!");
            }
        }


        private void SetUpDrone(IMyEntity entity)
        {
            Sandbox.ModAPI.IMyGridTerminalSystem gridTerminal = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid((IMyCubeGrid)entity);
            List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> T = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
            gridTerminal.GetBlocksOfType<IMyTerminalBlock>(T);

            var droneType = IsDrone(T);
                try
                {
                Util.Log(_logPath, "Found Drone of type: " + droneType);

                switch (droneType)
                    {
                        case ShipTypes.NavyFighter:
                        {
                            Ship ship = new Ship((IMyCubeGrid)entity, NavyController.NavyPlayerId);
                            Util.Log(_logPath, "[SetUpDrone] Found New Pirate Ship. id=" + ship.GetOwnerId());
                            navy.AddFighterToFleet(ship, droneType);
                            AddDiscoveredShip(ship);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Util.LogError(_logPath, e.ToString());
                }

        }

        private string Drone = "#PirateDrone#";
        private ShipTypes IsDrone(List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> T)
        {
            Util.Log(_logPath, "[MiningDrones.SetUpDrone] is a drone?");
            if (T.Exists(x => ((x).CustomName.Contains(Drone) && x.IsWorking)))
            {
                Util.Log(_logPath, "[MiningDrones.SetUpDrone] is a drone!");
                return ShipTypes.NavyFighter;
            }

            return ShipTypes.NotADrone;

        }


        private void Setup()
        {
            _initalized = true;
            Util.Log(_logPath,"Test startup");
            //_lastSaveTime = DateTime.Now;
            TestExecutor.SpawnShip(new Vector3D(0, 0, 0), NavyController.NavyPlayerId);
            TestExecutor.SpawnShip(new Vector3D(20,20,20), NavyController.NavyPlayerId);
        }
    }
}
