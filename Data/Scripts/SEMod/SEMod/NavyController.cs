using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace SEMod
{
    class NavyController
    {
        private String _logPath = "NavyController";
        private IMyCubeGrid station;
        List<Ship> fighters = new List<Ship>();
        internal static long NavyPlayerId = 10;
        private int maxNumberOfFighters = 10;
        private int fighterUpdate = 0;
        public void Update()
        {
            if ((DateTime.Now - lastUpdate).Seconds >= 20)
            {

                //UpdateAllTargetingComputers();
                lastUpdate = DateTime.Now;
            }
            fighterUpdate = fighterUpdate > fighters.Count ? 0 : fighterUpdate + 1;
            CalculateFleetMovements();
        }

        private void CalculateFleetMovements()
        {
            fighters = fighters.Where(x => x.IsOperational()).ToList();
            var fighter = fighters[fighterUpdate];

            Vector3D loc = MyAPIGatewayShortcuts.GetLocalPlayerPosition();
            try
            {
                //fighter.SetOwner(100);
                fighter.ReportDiagnostics();
                fighter.Update();
                
            }
            catch (Exception e)
            {
                    
                Util.NotifyHud(e.Message);
            }
                
            
        }

        private DateTime lastUpdate = DateTime.Now;
        //private void UpdateFighters()
        //{
        //    fighters = fighters.Where(x => x.IsOperational()).ToList();
        //    Util.Log(_logPath, "number of fighters " +fighters.Count);
            

        //    foreach (var fighter in fighters)
        //    {
        //        Vector3D loc = MyAPIGatewayShortcuts.GetLocalPlayerPosition();
        //        fighter.ReportDiagnostics();
        //        fighter.Guard(new Vector3D(0, 0, 0));
        //    }
        //}

        //private void UpdateAllTargetingComputers()
        //{
        //    if (fighters.Count < 3)
        //        TestExecutor.SpawnShip(new Vector3D(0, 0, 0));

        //    foreach (var fighter in fighters)
        //    {
        //        fighter.ScanLocalArea();
        //    }
        //}

        internal void AddFighterToFleet(Ship ship, ShipTypes droneType)
        {
            if (fighters.Exists(x => x._cubeGrid == ship._cubeGrid))
                return;
            ship.SetOwner(NavyPlayerId);
            fighters.Add(ship);
            //MyAPIGateway.Session.Factions.AddNewNPCToFaction
        }

        internal int GetNumberOfFighters()
        {
            return fighters.Count();
        }
    }
}
