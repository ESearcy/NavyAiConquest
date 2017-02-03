using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens.Helpers;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Library.Collections;
using VRage.ModAPI;
using VRageMath;

namespace SEMod
{
    internal class WeaponControls
    {

        internal IMyPlayer _targetPlayer = null;
        internal IMyCubeGrid _target = null;
        private IMyEntity Ship;
        private static string _logPath = "WeaponControls";
        private long _ownerId;
        internal int _minTargetSize = 10;
        List<IMyTerminalBlock> allWeapons = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> directionalWeapons = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> turrets = new List<IMyTerminalBlock>();
        private IMyGridTerminalSystem _gridTerminalSystem;

        Dictionary<IMyCubeGrid, TargetDetails> targets = new Dictionary<IMyCubeGrid, TargetDetails>();

        List<IMyCubeGrid> _nearbyFloatingObjects = new List<IMyCubeGrid>();
        List<IMyVoxelBase> _nearbyAsteroids = new List<IMyVoxelBase>();

        //same time no recalc
        private TimeSpan oneSecond;

        public void EnableWeapons()
        {
            directionalWeapons = directionalWeapons.Where(x => x != null && x.IsFunctional).ToList();
            foreach (var weapon in directionalWeapons)
            {
                weapon.GetActionWithName("OnOff_On").Apply(weapon);
                weapon.GetActionWithName("Shoot_On").Apply(weapon);
            }
        }

        public void DisableWeapons()
        {
            directionalWeapons = directionalWeapons.Where(x => x != null && x.IsFunctional).ToList();
            foreach (var weapon in directionalWeapons)
            {
                weapon.GetActionWithName("Shoot_Off").Apply(weapon);
                weapon.GetActionWithName("OnOff_Off").Apply(weapon);
            }
        }

        public IMyTerminalBlock GetTargetKeyAttackPoint(IMyCubeGrid grid)
        {
            TargetDetails details;
            IMyTerminalBlock block = null;
            if (targets.TryGetValue(grid, out details))
            {
                block = details.GetBestHardPointTarget(Ship.GetPosition());
            }
            return block;
        }

        public WeaponControls(IMyEntity Ship, long ownerID, IMyGridTerminalSystem grid)
        {
            this.Ship = Ship;
            this._ownerId = ownerID;
            this._gridTerminalSystem = grid;
            DateTime start = new DateTime(1,1,1,1,1,1);
            DateTime end = new DateTime(1, 1, 1, 1, 1, 2);
            oneSecond = start - end;
            DetectWeapons();
        }

        public bool IsOperational()
        {
            Util.Log(_logPath,"number of weapons: "+ GetWeaponsCount()+"turrets/other => "+turrets.Count+":"+directionalWeapons.Count);
            return GetWeaponsCount() > 0;
        }

        public int GetWeaponsCount()
        {
            allWeapons = allWeapons.Where(x => x != null).ToList();
            turrets = turrets.Where(x => x != null).ToList();

            allWeapons = allWeapons.Where(x => x.IsFunctional).ToList();
            turrets = turrets.Where(x => x.IsFunctional).ToList();
            return allWeapons.Count+turrets.Count;
        }

        private void DetectWeapons()
        {
            _gridTerminalSystem.GetBlocksOfType<IMyUserControllableGun>(allWeapons);
            _gridTerminalSystem.GetBlocksOfType<Sandbox.ModAPI.IMyLargeTurretBase>(turrets);
            directionalWeapons = allWeapons.Where(x=> !turrets.Contains(x)).ToList();
        }

        public void AddNearbyFloatingItem(IMyCubeGrid entity)
        {
            if (!_nearbyFloatingObjects.Contains(entity) && Ship != entity)
            {
                _nearbyFloatingObjects.Add(entity);
                ScanTarget(entity);
            }
        }

        public void AddNearbyAsteroid(IMyVoxelBase entity)
        {
            if (!_nearbyAsteroids.Contains(entity) && Ship != entity)
            {
                _nearbyAsteroids.Add(entity);
            }
        }

        private void ScanTarget(IMyCubeGrid grid)
        {
            //save logic time, no need to rescan targets
            if (targets.ContainsKey(grid))
                return;
            
            List<Sandbox.ModAPI.IMyTerminalBlock> terminalBlocks = new List<IMyTerminalBlock>();
            _gridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(terminalBlocks);

            //powerproducers
            List<IMyTerminalBlock> reactorBlocks = terminalBlocks.Where(x => x is IMyReactor).ToList();
            List<IMyTerminalBlock> batteryBlocks = terminalBlocks.Where(x => x is IMyBatteryBlock).ToList();

            bool isOnline = (reactorBlocks.Exists(x => (x.IsWorking)) || batteryBlocks.Exists(x => (x.IsWorking)));

            bool isownerId = grid.SmallOwners.Contains(_ownerId);
            
            if (!isOnline) return;
            
            var isEnemy = !GridFriendly(terminalBlocks) && !isownerId;
            //Util.NotifyHud("shared ownership " + isownerId+" isEnemy:"+isEnemy);

            List<IMyTerminalBlock> remoteControls = terminalBlocks.Where(x => x is IMyRemoteControl).ToList();
            List<IMyTerminalBlock> weapons = terminalBlocks.Where(x => x is IMyUserControllableGun).ToList();

            bool isDrone = remoteControls.Exists(x => ((IMyRemoteControl)x).CustomName.Contains("Drone#"));

            Util.Log(_logPath, "is Enemy: "+ isEnemy);
            if (isEnemy)
                targets.Add(grid, new TargetDetails(grid, isDrone));
        }

       

        private bool GridFriendly(List<IMyTerminalBlock> gridblocks)
        {
            bool isFriendly = false;
            //Dictionary<MyRelationsBetweenPlayerAndBlock, int> uniqueIds = new Dictionary<MyRelationsBetweenPlayerAndBlock, int>();
            //var myfaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(_ownerId);
            //int numUnknownFactions = 0;
            //bool hasFriendlyBlock = false;
            //bool isownerId = gridblocks.SmallOwners.Contains(_ownerId);
            int fs = 0;
            int n = 0; int no = 0; int o = 0; int e = 0;
            foreach (var block in gridblocks)
            {
                switch (block.GetUserRelationToOwner(_ownerId))
                {
                    case MyRelationsBetweenPlayerAndBlock.FactionShare:
                        fs++;
                        //isFriendly = true;
                        break;
                    case MyRelationsBetweenPlayerAndBlock.Neutral:
                        n++;
                        break;
                    case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                        no++;
                        break;
                    case MyRelationsBetweenPlayerAndBlock.Enemies:
                        e++;
                        break;
                }
                if (isFriendly)
                    break;
            }
            //Util.NotifyHud("fs:"+fs+ " n:" + n + " no:" + no + " o:" + o + " e:" + e);


            return isFriendly;

            ////this shit doesnt work even though it should, maybe i just did it wrong.

        }

        public void ClearNearbyObjects()
        {
            _nearbyFloatingObjects.Clear();
        }
        TargetDetails targetDetails = null;

        DateTime _targetAquiredTime = DateTime.Now;
        //always gets the most current nearest target (causes switching alot in fleet fights)
        public TargetDetails GetEnemyTarget()
        {
            //Util.NotifyHud("targets count: "+targets.Count);
            targets = targets.Where(x=>x.Value.IsOperational()).OrderBy(x => (x.Key.GetPosition() - Ship.GetPosition()).Length()).ToDictionary(x => x.Key, x => x.Value);

            if ((DateTime.Now - _targetAquiredTime).Seconds > 10)
            {
                _targetAquiredTime = DateTime.Now;
                targetDetails = null;
            }
            //first case is caught first to avoid null exception
            if (targetDetails ==null || !targetDetails.IsOperational())
                targetDetails = null;

            if (targetDetails == null && targets.Count > 0)
            {
                var orderedTargets = targets.OrderBy(x => (x.Key.GetPosition() - Ship.GetPosition()).Length());
                targetDetails = orderedTargets.First().Value;
            }
            if (targetDetails != null && !targetDetails.IsOperational())
                targetDetails = null;

            return targetDetails;
        }

        public List<TargetDetails> GetObjectsInRange(int range)
        {
            List<TargetDetails> keyValuePairs = targets.Where(x => (x.Key.GetPosition() - Ship.GetPosition()).Length()<=range).Select(x=>x.Value).ToList();
            //Util.NotifyHud(keyValuePairs.Count+" count");
            return keyValuePairs;
        }

        public IMyPlayer GetEnemyPlayer()
        {
            return _targetPlayer;
        }

        public float GetTargetSize(IMyCubeGrid enemyTarget)
            {
                float radiusOfTarget = 1000;//default
                if (targets.ContainsKey(enemyTarget))
                {
                    radiusOfTarget = (float)targets[enemyTarget].ShipSize;
                }

                return radiusOfTarget;
            }

        int count = 0;
        public void DebugMarkAllTrackedObjects()
        {
            long id = MyAPIGateway.Session.Player.IdentityId;
            count = 0;
            Util.NotifyHud(targets.Count+"");
            foreach (var obj in _nearbyFloatingObjects)
            {
                count++;
                IMyGps mygps = MyAPIGateway.Session.GPS.Create("X", "", obj.GetPosition(), true, true);
                MyAPIGateway.Session.GPS.AddGps(id, mygps);
            }
        }
        public void DebugMarkAllTrackedTargets()
        {
            long id = MyAPIGateway.Session.Player.IdentityId;
            count = 0;
            Util.NotifyHud(targets.Count + "");
            foreach (var obj in targets)
            {
                count++;
                IMyGps mygps = MyAPIGateway.Session.GPS.Create("X", "", obj.Key.GetPosition(), true, true);
                MyAPIGateway.Session.GPS.AddGps(id, mygps);
            }
        }

        public void DebugMarkTargetAndKeyPoint()
        {
            TargetDetails target = GetEnemyTarget();
            if (target != null)
            {
                var targetBlock = target.GetBestHardPointTarget(Ship.GetPosition());

                long id = MyAPIGateway.Session.Player.IdentityId;
                IMyGps mygps = MyAPIGateway.Session.GPS.Create("X", "", target.Ship.GetPosition(), true, true);
                MyAPIGateway.Session.GPS.AddGps(id, mygps);

                if (targetBlock != null)
                {
                    IMyGps mygps2 = MyAPIGateway.Session.GPS.Create("X", "", targetBlock.GetPosition(), true, true);
                    MyAPIGateway.Session.GPS.AddGps(id, mygps2);
                }
            }
        }

        internal void ClearTargets()
        {
            targets.Clear();
        }

        internal List<IMyVoxelBase> GetAsteroids(int range)
        {
            return _nearbyAsteroids.Where(x => (x.GetPosition() - Ship.GetPosition()).Length() <= range).ToList();
        }

        internal int GetNumberTargets()
        {
            return targets.Count;
        }
    }
}