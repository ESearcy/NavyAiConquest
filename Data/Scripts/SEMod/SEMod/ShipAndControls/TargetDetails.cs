using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using Sandbox.Game.Entities;
using VRageMath;

namespace SEMod
{
    class TargetDetails
    {
        private static string logPath = "targetDetails";
        public DateTime LastScannedTime = DateTime.Now;
        public IMyCubeGrid Ship;
        private List<IMyTerminalBlock> _keyPoints = new List<IMyTerminalBlock>();
        private static int ShipRescanRate = 30;
        public double ShipSize = 0;
        public bool IsDrone;
        private List<IMyTerminalBlock> weapons = new List<IMyTerminalBlock>();
        private List<IMyTerminalBlock> terminalBlocks = new List<IMyTerminalBlock>();

        public TargetDetails(IMyCubeGrid ship, bool isDrone)
        {
            Ship = ship;
            this.IsDrone = isDrone;
            LocateTargetHardpoints();
        }

        public IMyTerminalBlock GetBestHardPointTarget(Vector3D position)
        {
            //if ((DateTime.Now - LastScannedTime).TotalSeconds > ShipRescanRate && Ship != null)
            {
                LocateTargetHardpoints();
                LastScannedTime = DateTime.Now;
            }

            _keyPoints = _keyPoints.Where(x => x.IsFunctional).OrderBy(x=>(position-x.GetPosition()).Length()).ToList();
            var temp = _keyPoints.FirstOrDefault();
            return temp;
        }

        public void LocateTargetHardpoints()
        {
            IMyCubeGrid grid = Ship;
            var centerPosition = Ship.GetPosition();
            _keyPoints.Clear();
            //get position, get lenier velocity in each direction
            //add them like 10 times and add that to current coord
            if (grid != null)
            {
                Sandbox.ModAPI.IMyGridTerminalSystem gridTerminal =
                    Sandbox.ModAPI.MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);

                gridTerminal.GetBlocksOfType<Sandbox.ModAPI.IMyTerminalBlock>(terminalBlocks);
                terminalBlocks = terminalBlocks.Where(x => x.IsFunctional).ToList();

                List<IMyTerminalBlock> reactors = terminalBlocks.Where(x => x is IMyReactor).ToList();
                List<IMyTerminalBlock> batteries = terminalBlocks.Where(x => x is IMyBatteryBlock).ToList();
                List<IMyTerminalBlock> cockpits = terminalBlocks.Where(x => x is IMyCockpit).ToList();
                List<IMyTerminalBlock> thrusters = terminalBlocks.Where(x => x is IMyThrust).ToList();

                weapons = terminalBlocks.Where(x => x is Sandbox.ModAPI.IMyUserControllableGun).ToList();
                ShipSize = terminalBlocks.Max(x=>(x.GetPosition()-Ship.GetPosition()).Length())*2;

                //now that we have a list of reactors and guns lets primary one.
                //try to find a working gun, if none are found then find a reactor to attack
                _keyPoints.AddRange(reactors);
                _keyPoints.AddRange(batteries);
                _keyPoints.AddRange(cockpits);
                _keyPoints.AddRange(thrusters);
                _keyPoints.AddRange(weapons);
                _keyPoints.AddRange(terminalBlocks);
            }

        }

        public bool IsOperational()
        {
            
            List<IMyTerminalBlock> reactorBlocks = terminalBlocks.Where(x => (x is IMyReactor || x is IMyBatteryBlock) && x.IsFunctional).ToList();
            return reactorBlocks.Count > 0 && Ship.Physics.Mass > 2000;
        }
    }
}
