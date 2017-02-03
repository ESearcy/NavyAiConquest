using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace SEMod
{
    class Ship
    {
          
        private static string _logPath = "Ship";
        internal WeaponControls weaponControls;
        public IMyCubeGrid _cubeGrid;
        internal VRage.Game.ModAPI.Interfaces.IMyControllableEntity ShipControls;
        internal long _ownerId;
        internal NavigationControls navigation;
        public static double DETECTIONRANGE = 2000;
        public Sandbox.ModAPI.IMyGridTerminalSystem GridTerminalSystem;
        Random _r = new Random();
        
        private double HealthBlockBase = 0;
        private int functionalBlockCount = 0;
        private int healthPercent = 0;
        private TimeSpan rescanDelayTimespan;
        private DroneWeaponActions _currentWeaponAction = DroneWeaponActions.Standby;
        private DroneNavigationActions _currentNavigationAction = DroneNavigationActions.Stationary;

        //internal string _beaconName = "CombatDrone";

        //private List<IMyTerminalBlock> beacons = new List<IMyTerminalBlock>();
        //private List<IMyTerminalBlock> antennas = new List<IMyTerminalBlock>();

        ////Weapon Controls
        //internal bool _isFiringManually;
        //internal List<IMyTerminalBlock> _allWeapons = new List<IMyTerminalBlock>();
        //internal List<IMyTerminalBlock> _allReactors = new List<IMyTerminalBlock>();
        //internal List<IMyTerminalBlock> _manualGuns = new List<IMyTerminalBlock>();
        //internal List<IMyTerminalBlock> _manualRockets = new List<IMyTerminalBlock>();


        //internal double _maxAttackRange = 1000;
        //private long _bulletSpeed = 200; //m/s
        //private long _defaultOrbitRange = 700; //m/s
        //private long _maxFiringRange = 700;

        //internal IMyEntity GetEntity()
        //{
        //    return _cubeGrid as IMyEntity;
        //}

        //private int _radiusOrbitmultiplier = 12;
        //private int _saftyOrbitmultiplier = 8;

        //internal DateTime _createdAt = DateTime.Now;
        //internal int _minTargetSize = 10;


        //int missileStaggeredFireIndex = 0;
        //DateTime _lastRocketFired = DateTime.Now;
        //#endregion

        //private static int numDrones = 0;
        //internal int myNumber;
        //public Type Type = typeof(Ship);

        public long GetOwnerId()
        {
            return _ownerId;
        }

        public Ship(IMyCubeGrid ent, long id)
        {
            _cubeGrid = ent;
            Util.NotifyHud("Creating Drone");
            GridTerminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(_cubeGrid);

            SetOwner(id);
            List<Sandbox.ModAPI.IMyTerminalBlock> remoteControls = new List<Sandbox.ModAPI.IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(remoteControls);

            ShipControls = remoteControls.FirstOrDefault() != null ? remoteControls.First() as Sandbox.Game.Entities.IMyControllableEntity : null;
            _ownerId = id;

            if (ShipControls == null)
                return;

            DetectReactors();
            LocateShipComponets();
            ResetHealthMax();
            SetupRescanDelay();
            
            //ConfigureAntennas();
            //ReloadWeaponsAndReactors();
            //Util.NotifyHud("Checkpoint:" + T.Count);

            //_cubeGrid.OnBlockAdded += RecalcMaxHp;
            //myNumber = numDrones;
            //numDrones++;
        }

        private int one = 1;
        private int rescanDelay = 10;
        private void SetupRescanDelay()
        {
            DateTime start = new DateTime(one, one, one, one, one, one);
            DateTime end = new DateTime(one, one, one, one, one, one * rescanDelay);
            rescanDelayTimespan = (end - start);
        }

        List<IMyTerminalBlock> reactors = new List<IMyTerminalBlock>();

        private void ResetHealthMax()
        {
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(allTerminalBlocks);
            _cubeGrid.OnBlockAdded += _cubeGrid_OnBlockAdded;
            HealthBlockBase = allTerminalBlocks.Count;
        }

        private void _cubeGrid_OnBlockAdded(IMySlimBlock obj)
        {
            ResetHealthMax();
        }

        List<IMyTerminalBlock> allTerminalBlocks = new List<IMyTerminalBlock>();
        private void UpdateHealthPercent()
        {
            allTerminalBlocks = allTerminalBlocks.Where(x => x != null).ToList();
            functionalBlockCount = allTerminalBlocks.Count(x=> x.IsFunctional);
            healthPercent = (int)((functionalBlockCount/HealthBlockBase)*100);
        }

        private void DetectReactors()
        {
            reactors.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyReactor>(reactors);
        }

        private void LocateShipComponets()
        {
            
            navigation = new NavigationControls(_cubeGrid, ShipControls);
            weaponControls = new WeaponControls(_cubeGrid, _ownerId, GridTerminalSystem);
        }

        private void ConfigureAntennas()
        {
            //var lstSlimBlock = new List<IMySlimBlock>();
            //_cubeGrid.GetBlocks(lstSlimBlock, (x) => x.FatBlock is Sandbox.ModAPI.IMyRadioAntenna);
            //foreach (var block in lstSlimBlock)
            //{
            //    Sandbox.ModAPI.IMyRadioAntenna antenna =
            //        (Sandbox.ModAPI.IMyRadioAntenna)block.FatBlock;
            //    if (antenna != null)
            //    {
            //        //antenna.GetActionWithName("SetCustomName").Apply(antenna, new ListReader<TerminalActionParameter>(new List<TerminalActionParameter>() { TerminalActionParameter.Get("Combat Drone " + _manualGats.Count) }));
            //        antenna.SetValueFloat("Radius", 10000);//antenna.GetMaximum<float>("Radius"));
            //        _blockOn.Apply(antenna);
            //    }
            //}

            //lstSlimBlock = new List<IMySlimBlock>();
            //_cubeGrid.GetBlocks(lstSlimBlock, (x) => x.FatBlock is Sandbox.ModAPI.IMyBeacon);
            //foreach (var block in lstSlimBlock)
            //{
            //    Sandbox.ModAPI.IMyBeacon beacon = (Sandbox.ModAPI.IMyBeacon)block.FatBlock;
            //    if (beacon != null)
            //    {
            //        beacon.SetValueFloat("Radius", 10000);//beacon.GetMaximum<float>("Radius"));
            //        _blockOn.Apply(beacon);
            //    }
            //}
        }

        //this percent is based on IMyTerminalBlocks so it does not take into account the status of armor blocks
        //any blocks not functional decrease the overall %
        //having less blocks than when the drone was built will also result in less hp (parts destoried)
        private void CalculateDamagePercent()
        {
            //Util.Log(_logPath, "Entering Method Calculate DamagePercent");
            //try
            //{
            //    List<IMyTerminalBlock> allTerminalBlocks =
            //        new List<IMyTerminalBlock>();

            //    GridTerminalSystem.GetBlocksOfType<IMyCubeBlock>(allTerminalBlocks);


            //    double runningPercent = 0;
            //    foreach (var block in allTerminalBlocks)
            //    {
            //        runningPercent += block.IsWorking || block.IsFunctional ? 100d : 0d;
            //    }
            //    runningPercent = runningPercent / allTerminalBlocks.Count;

            //    _healthPercent = ((int)((allTerminalBlocks.Count / HealthBlockBase) * (runningPercent)) + "%");//*(runningPercent);
            //}
            //catch (Exception e)
            //{
            //    Util.NotifyHud(e.ToString());
            //    Util.LogError(_logPath, e.ToString());
            //    //this is to catch the exception where the block blows up mid read bexcause its under attack or whatever
            //}
        }

        private void RecalcMaxHp(IMySlimBlock obj)
        {
            //List<IMyTerminalBlock> allTerminalBlocks =
            //        new List<IMyTerminalBlock>();

            //GridTerminalSystem.GetBlocksOfType<IMyCubeBlock>(allTerminalBlocks);

            //double count = 0;
            //foreach (var block in allTerminalBlocks)
            //{
            //    count += block.IsWorking || block.IsFunctional ? 100d : 0d;
            //}

            //HealthBlockBase = allTerminalBlocks.Count;
        }


        //Turn weapons on and off SetWeaponPower(true) turns weapons online: vice versa
        public void SetWeaponPower(bool isOn)
        {
            //foreach (var w in _allWeapons)
            //{
            //    if (isOn)
            //        _blockOn.Apply(w);
            //    else
            //        _blockOff.Apply(w);
            //}
        }

        private void FindWeapons()
        {
            //if (_cubeGrid == null)
            //    return;

            //_allWeapons.Clear();
            //_allReactors.Clear();
            //_manualGuns.Clear();
            //_manualRockets.Clear();


            //Sandbox.ModAPI.IMyGridTerminalSystem gridTerminal = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(_cubeGrid);
            //List<Sandbox.ModAPI.IMyTerminalBlock> T = new List<Sandbox.ModAPI.IMyTerminalBlock>();

            //gridTerminal.GetBlocksOfType<IMySmallMissileLauncher>(T);
            //T = new List<Sandbox.ModAPI.IMyTerminalBlock>();
            //_manualRockets = T;
            //gridTerminal.GetBlocksOfType<IMySmallGatlingGun>(T);
            //T = new List<Sandbox.ModAPI.IMyTerminalBlock>();
            //_manualGuns = T;
            //gridTerminal.GetBlocksOfType<IMyReactor>(T);
            //T = new List<Sandbox.ModAPI.IMyTerminalBlock>();
            //_allReactors = T;
            //gridTerminal.GetBlocksOfType<IMyUserControllableGun>(T);
            //_allWeapons = T;

        }
        private void SetupActions()
        {
            
            //if (_fireGun == null && _allWeapons.Count > 0 && _allWeapons[0] != null && !actionsConfigured)
            //{
            //    var actions = new List<ITerminalAction>();
                
            //    ((Sandbox.ModAPI.IMyUserControllableGun)_allWeapons[0]).GetActions(actions);
            //    Util.Log(_logPath, "Number of actions " + actions.Count);
            //    if (_fireRocket == null && actions.Count>0)
            //    {
            //        foreach (var act in actions)
            //        {
            //            Util.Log(_logPath, "[Drone.IsOperational] Action Name " + act.Name.Replace(" ", "_"));
            //            switch (act.Name.ToString())
            //            {
            //                case "Shoot_once":
            //                    _fireRocket = act;
            //                    break;
            //                case "Shoot_On":
            //                    _fireGun = act;
            //                    break;
            //                case "Toggle_block_Off":
            //                    _blockOff = act;
            //                    break;
            //                case "Toggle_block_On":
            //                    _blockOn = act;
            //                    break;
            //            }
            //        }
            //        actionsConfigured = true;
            //        Util.Log(_logPath, "[Drone.IsOperational] Has Missile attack -> " + (_fireRocket != null) + " Has Gun Attack " + (_fireRocket != null) + " off " + (_blockOff != null) + " on " + (_blockOn != null));
            //    }
            //}
        }
        //All three must be true
        //_cubeGrid is not trash
        //_cubeGrid Controlls are functional
        //Weapons Exist on ship
        //There have been a few added restrictions that must be true for a ship[ to be alive
        public bool IsOperational()
        {
            bool shipAndControlAlive = _cubeGrid != null && //ship
                ShipControls != null && (ShipControls as IMyTerminalBlock).IsFunctional; //shipcontrols
            bool isAlive = navigation.IsOperational() && weaponControls.IsOperational() && shipAndControlAlive;
            Util.Log(_logPath,"Is Alive: "+isAlive);
            return isAlive;
        }

        //Disables all beacons and antennas and deletes the ship.
        public void DeleteShip()
        {
            var lstSlimBlock = new List<IMySlimBlock>();
            _cubeGrid.GetBlocks(lstSlimBlock, (x) => x.FatBlock is Sandbox.ModAPI.IMyRadioAntenna);
            foreach (var block in lstSlimBlock)
            {
                Sandbox.ModAPI.IMyRadioAntenna antenna = (Sandbox.ModAPI.IMyRadioAntenna)block.FatBlock;
                ITerminalAction act = antenna.GetActionWithName("OnOff_Off");
                act.Apply(antenna);
            }

            lstSlimBlock = new List<IMySlimBlock>();
            _cubeGrid.GetBlocks(lstSlimBlock, (x) => x.FatBlock is Sandbox.ModAPI.IMyBeacon);
            foreach (var block in lstSlimBlock)
            {
                Sandbox.ModAPI.IMyBeacon beacon = (Sandbox.ModAPI.IMyBeacon)block.FatBlock;
                ITerminalAction act = beacon.GetActionWithName("OnOff_Off");
                act.Apply(beacon);
            }

            MyAPIGateway.Entities.RemoveEntity(_cubeGrid as IMyEntity);
            _cubeGrid = null;
        }

        private void TurnOnShip()
        {
            List<IMyTerminalBlock> thrusters = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);

            foreach (var thruster in thrusters)
            {
                thruster.GetActionWithName("OnOff_On").Apply(thruster);
            }

            List<IMyTerminalBlock> gyro = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyGyro>(gyro);

            foreach (var g in gyro)
            {
                g.GetActionWithName("OnOff_On").Apply(g);
            }
        }

        //ship location
        public Vector3D GetPosition()
        {
            return _cubeGrid.GetPosition();
        }

        //Changes grid ownership of the drone
        public void SetOwner(long id)
        {
            _ownerId = id;
            _cubeGrid.ChangeGridOwnership(id,MyOwnershipShareModeEnum.Faction);
            _cubeGrid.UpdateOwnership(id, true);
        }

        //usses ammo manager to Reload the inventories of the reactors and guns (does not use cargo blcks)
        public void ReloadWeaponsAndReactors()
        {
            //FindWeapons();
            //Util.Log(_logPath, "Number of weapons reloading");
            //ItemManager im = new ItemManager();
            //im.Reload(_allWeapons);
            //im.ReloadReactors(_allReactors);
        }

        //turn on all weapons
        public void ManualFire(bool doFire)
        {
            //FindWeapons();
            //SetWeaponPower(doFire);
            //if (doFire)
            //{
            //    Util.Log(_logPath, _fireGun + "[Drone.ManualFire] Number of guns -> " + _manualGuns.Count);
            //    Util.Log(_logPath, _fireGun + "[Drone.ManualFire] number of all weapons -> " + _allWeapons.Count);
            //    foreach (var gun in _manualGuns)
            //    {
            //        _fireGun.Apply(gun);
            //    }

            //    if (Math.Abs((DateTime.Now - _lastRocketFired).TotalMilliseconds) > 500 && _fireRocket != null &&
            //        _manualRockets.Count > 0)
            //    {
            //        var launcher = _manualRockets[missileStaggeredFireIndex];
            //        _fireGun.Apply(launcher);
            //        if (missileStaggeredFireIndex + 1 < _manualRockets.Count())
            //        {
            //            missileStaggeredFireIndex++;
            //        }
            //        else
            //            missileStaggeredFireIndex = 0;
            //        _lastRocketFired = DateTime.Now;
            //    }
            //}

            //_isFiringManually = doFire;

        }

        public void DisableThrusterGyroOverrides()
        {
            navigation.DisableThrusterGyroOverrides();
        }

        public bool ControlledByPlayer()
        {
            return navigation.PlayerHasControl();
        }

        DateTime lastScan = DateTime.Now;
        private int _rescanRate = 3;//seconds
        private int numRescans = 0;
        public void ScanLocalArea()
        {
            var ts = (DateTime.Now - lastScan);
            if (ts.TotalSeconds < _rescanRate)
                return;

            if (numRescans > 10)
            {
                weaponControls.ClearTargets();
                numRescans = 0;
            }
            numRescans++;

            lastScan = DateTime.Now;
            var bs = new BoundingSphereD(_cubeGrid.GetPosition(), DETECTIONRANGE);
            var nearbyEntities = MyAPIGateway.Entities.GetEntitiesInSphere(ref bs);

            //var closeAsteroids = asteroids.Where(z => (z.GetPosition() - drone.GetPosition()).Length() < MaxEngagementRange).ToList();

            weaponControls.ClearNearbyObjects();
            foreach (var closeItem in nearbyEntities)
            {
                if (closeItem is IMyCubeGrid && !closeItem.Transparent && closeItem.Physics.Mass > 5000)
                    weaponControls.AddNearbyFloatingItem(closeItem as IMyCubeGrid);
                //if (closeItem is IMyVoxelBase)
                //    _aMananager.Scan((IMyVoxelBase)closeItem);
            }
        }

        //Working - and damn good I might add
        //returns status means -1 = not activated, 0 = notEngaged, 1 = InCombat
        public int Guard(Vector3D position)
        {
            //if (_bulletSpeed < 400)
            //    _bulletSpeed += 100;
            //else
            //    _bulletSpeed = 100;

            //var targetVector = Vector3D.Zero;
            //var target = Vector3D.Zero;


            //ManualFire(false);
            //float enemyShipRadius = _defaultOrbitRange;
            //IMyCubeGrid enemyTarget = tc.FindEnemyTarget();
            //var avoidanceVector = navigation.GetWeightedCollisionAvoidanceVectorForNearbyStructures();

            //if (enemyTarget != null)
            //{
            //    target = enemyTarget.GetPosition();

            //    var keyPoint = tc.GetTargetKeyAttackPoint(enemyTarget);
            //    enemyShipRadius = tc.GetTargetSize(enemyTarget);
            //    if (keyPoint != null)
            //    {
            //        target = keyPoint.GetPosition();
            //        targetVector = enemyTarget.Physics.LinearVelocity;
            //    }
            //}
            //else if (tc.GetEnemyPlayer() != null)
            //{
            //    target = tc.GetEnemyPlayer().GetPosition();
            //}

            //if (target != Vector3D.Zero)
            //{

            //    var distance = (position - _cubeGrid.GetPosition()).Length();
            //    var distanceFromTarget = (target - _cubeGrid.GetPosition()).Length();

            //    double distanceVect = (target - _cubeGrid.GetPosition()).Length() / _bulletSpeed;
            //    Vector3D compAmount = targetVector - _cubeGrid.Physics.LinearVelocity;
            //    Vector3D compVector = new Vector3D(compAmount.X * distanceVect, compAmount.Y * distanceVect, compAmount.Z * distanceVect);

            //    if (distance > _maxAttackRange)
            //    {
            //        _currentNavigationAction = DroneNavigationActions.Approaching;
            //        Approach(position);
            //    }
            //    else
            //    {
            //        _currentNavigationAction = DroneNavigationActions.Orbiting;


            //        if (avoidanceVector != Vector3D.Zero)
            //        {
            //            if (_cubeGrid.Physics.LinearVelocity.Normalize() > _maxAvoidanceSpeed)
            //            {
            //                navigation.SlowDown();
            //            }
            //            else
            //            {
            //                navigation.WeightedThrustTwordsDirection(avoidanceVector);
            //            }
            //            _currentNavigationAction = DroneNavigationActions.Avoiding;
            //        }
            //        else
            //            CombatOrbit(target, enemyShipRadius * _radiusOrbitmultiplier);
            //        //KeepAtCombatRange(target, targetVector);
            //        double alignment = AlignTo(target + compVector);


            //        if (alignment < 1)
            //        {
            //            _currentWeaponAction = DroneWeaponActions.Attacking;
            //            ManualFire(true);
            //        }
            //        else
            //        {
            //            _currentWeaponAction = DroneWeaponActions.LockedOn;
            //            ManualFire(false);
            //        }
            //    }
            //}
            //else if (avoidanceVector != Vector3D.Zero)
            //{
            //    if (_cubeGrid.Physics.LinearVelocity.Normalize() > _maxAvoidanceSpeed)
            //    {
            //        navigation.SlowDown();
            //    }
            //    else
            //    {
            //        AlignTo(avoidanceVector);
            //        navigation.WeightedThrustTwordsDirection(avoidanceVector);
            //    }
            //    _currentNavigationAction = DroneNavigationActions.Avoiding;
            //    _currentWeaponAction = DroneWeaponActions.Standby;
            //}
            //else
            //{
            //    _currentWeaponAction = DroneWeaponActions.Standby;
            //    _currentNavigationAction = DroneNavigationActions.Approaching;
            //    Orbit(position);
            //    //ManualFire(false);
            //}

            return 0;
        }

        //this sets the status of the ship in its beacon name or antenna name - this is user settable within in drone name
        //if drone name includes :antenna then the drone will display information on the antenna rather than the beacon
        public void NameBeacon()
        {
            //try
            //{

            //    if (broadcastingType == 1)
            //    {
            //        if (Util.debuggingOn)
            //        {
            //            FindBeacons();
            //            if (beacons != null && beacons.Count > 0)
            //            {
            //                CalculateDamagePercent();
            //                var beacon = beacons[0] as Sandbox.ModAPI.IMyBeacon;
            //                beacon.SetCustomName(_beaconName +
            //                                     " HP: " + _healthPercent +
            //                                     " MS: " + (int)_cubeGrid.Physics.LinearVelocity.Normalize());
            //            }
            //        }
            //        else
            //        {
            //            FindBeacons();
            //            if (beacons != null && beacons.Count > 0)
            //            {
            //                CalculateDamagePercent();
            //                var beacon = beacons[0] as Sandbox.ModAPI.IMyBeacon;
            //                beacon.SetCustomName("HP: " + _healthPercent);
            //            }
            //        }
            //    }
            //    else
            //        Broadcast();
            //}
            //catch
            //{
            //}
        }

        internal void ReportDiagnostics()
        {
            FindAntennasAndBeacons();
            Broadcast();
            //    SetBroadcasting(true);
        }

        private String _beaconName = "";
        public void Broadcast()
        {
            
            Util.Log(_logPath, _beaconName +
                                              "\nHP: " + healthPercent +
                                              "\nMS: " + (int)_cubeGrid.Physics.LinearVelocity.Normalize() +
                                              "\nNA: " + _currentNavigationAction +
                                              "\nWA: " + _currentWeaponAction +
                                              "\nWC: " + weaponControls.GetWeaponsCount() +
                                              "\nGC: " + navigation.GetWorkingGyroCount() +
                                              "\nTC: " + navigation.GetWorkingThrusterCount());
            try
            {
                if (Util.debuggingOn)
                {
                    if (antennas != null && antennas.Count > 0)
                    {
                        CalculateDamagePercent();
                        var antenna = antennas[0] as Sandbox.ModAPI.IMyRadioAntenna;
                        antenna.SetCustomName(_beaconName +
                                              "\nHP: " + healthPercent +
                                              "\nMS: " + (int)_cubeGrid.Physics.LinearVelocity.Normalize() +
                                              "\nNA: " + _currentNavigationAction +
                                              "\nWA: " + _currentWeaponAction);

                    }
                }
                else
                {
                    if (antennas != null && antennas.Count > 0)
                    {
                        CalculateDamagePercent();
                        var antenna = antennas[0] as Sandbox.ModAPI.IMyRadioAntenna;
                        antenna.SetCustomName(_beaconName +
                                              "\nHP: " + healthPercent +
                                              "\nMS: " + (int)_cubeGrid.Physics.LinearVelocity.Normalize());
                    }
                }
            }
            catch (Exception e)
            {
                Util.LogError(_logPath, e.ToString());
            }
        }

        internal void SetBroadcasting(bool broadcastingEnabled)
        {
            //FindBeacons();
            //ITerminalAction power = broadcastingEnabled ? _blockOn : _blockOff;
            //Util.Log(_logPath, "ShipAligned 1" + (power==null));
            //foreach (var v in beacons)
            //{
            //    power.Apply(v);
            //}
            //FindAntennas();
            //foreach (var v in antennas)
            //{
            //    power.Apply(v);
            //}
        }

        public void FindBeacons()
        {
            Sandbox.ModAPI.IMyGridTerminalSystem gridTerminal = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(_cubeGrid);
            List<Sandbox.ModAPI.IMyTerminalBlock> T = new List<Sandbox.ModAPI.IMyTerminalBlock>();
            gridTerminal.GetBlocksOfType<IMyBeacon>(T);
            beacons = T;
        }

        List<IMyTerminalBlock> antennas = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> beacons = new List<IMyTerminalBlock>();

        public void FindAntennasAndBeacons()
        {
            beacons = allTerminalBlocks.Where(x => x is IMyBeacon).ToList();
            antennas = allTerminalBlocks.Where(x => x is IMyRadioAntenna).ToList();
        }

        // for thoes pesky drones that just dont care about the safty of others
        public void Detonate()
        {
            ShipControls.MoveAndRotateStopped();
            List<IMyTerminalBlock> warHeads = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<Sandbox.ModAPI.IMyWarhead>(warHeads);

            foreach (var warhead in warHeads)
                warhead.GetActionWithName("StartCountdown").Apply(warhead);
        }

        public double AlignTo(Vector3D target)
        {
            return navigation.AlignTo(target);
        }

        public void FullThrust(Vector3D target)
        {
            var distance = (_cubeGrid.GetPosition() - target).Length();

            var maxSpeed = distance > 1000 ? 150 : distance / 10 > 20 ? distance / 10 : 20;

            navigation.ThrustTwordsDirection(_cubeGrid.GetPosition() - target);
        }

        public void Approach(Vector3D target)
        {

            var distance = (_cubeGrid.GetPosition() - target).Length();

            var maxSpeed = distance > 1000 ? 150 : distance / 10 > 20 ? distance / 10 : 20;
            AlignTo(target);

            Vector3D avoidanceVector = navigation.AvoidNearbyGrids();
            if (avoidanceVector != Vector3D.Zero)
            {
                navigation.ThrustTwordsDirection(avoidanceVector, false, true);
            }
            else if (_cubeGrid.Physics.LinearVelocity.Normalize() > maxSpeed)
            {
                navigation.SlowDown();
            }
            else if (distance > 50)
            {
                navigation.ThrustTwordsDirection(_cubeGrid.GetPosition() - target);
            }
            //else if (distance < 50)
            //{
            //    navigation.ThrustTwordsDirection(target - _cubeGrid.GetPosition());
            //}

        }

        private void KeepAtCombatRange(Vector3D target, Vector3D velocity)
        {
            var distance = (_cubeGrid.GetPosition() - target).Length();

            if (_cubeGrid.Physics.LinearVelocity.Normalize() > velocity.Normalize() * 1.2)
            {
                navigation.SlowDown();
            }
            else if (distance > 700)
            {
                navigation.ThrustTwordsDirection(_cubeGrid.GetPosition() - target);
            }
            else if (distance < 500)
            {
                navigation.ThrustTwordsDirection(target - _cubeGrid.GetPosition());
            }
            else
            {
                navigation.EvasiveManeuvering(velocity);
            }
        }

        public void Orbit(Vector3D lastTargetPosition)
        {
            navigation.Orbit(lastTargetPosition);
        }

        public void AimFreeOrbit(Vector3D lastTargetPosition)
        {
            navigation.CombatOrbit(lastTargetPosition);
        }


        private int breakawayDistance = 75;
        Vector3D bounceVector = Vector3D.Zero;
        private bool attacking = false;
        public bool Update()
        {
            UpdateHealthPercent();
            weaponControls.DisableWeapons();

            bool isOperational = IsOperational();
            if (isOperational)
            {
                ScanLocalArea();
                // navigation.Orbit(MyAPIGateway.Session.Player.GetPosition());
                CalculateAvoidanceVectors();
                TargetDetails target = weaponControls.GetEnemyTarget();


                bool targetLocked = target != null;
                if (targetLocked)
                {
                    var targetblock = weaponControls.GetTargetKeyAttackPoint(target.Ship);
                    var targetPoition = targetblock != null ? targetblock.GetPosition() : target.Ship.GetPosition();
                    

                    var awayDir = (_cubeGrid.GetPosition() - target.Ship.GetPosition());
                    var dirTotarget = (target.Ship.GetPosition() - _cubeGrid.GetPosition());
                    var distance = dirTotarget.Length() - target.ShipSize;
                    var avoidTargetVector = ((dirTotarget*avoidanceVector) + dirTotarget)*100;
                    if (distance > breakawayDistance)
                    {
                        navigation.CombatApproach(targetPoition); //CombatOrbit(MyAPIGateway.Session.Player.GetPosition());
                        var falloff = target.Ship.Physics.LinearVelocity - _cubeGrid.Physics.LinearVelocity;
                        double alignment = AlignTo(targetPoition+ falloff);
                        if (alignment < 1)
                        {
                            weaponControls.EnableWeapons();
                            if (alignment <= .1)
                                navigation.StopSpin();
                        }


                        breakawayDistance = 75;
                        bounceVector = Vector3D.Zero;
                        attacking = true;
                    }
                    else
                    {
                        //dirTotarget.Normalize();
                        //avoidanceVector.Normalize();
                        
                        //bounceVector = (avoidanceVector*2+ _cubeGrid.Physics.LinearVelocity) *100;
                        //else if (bounceVector == Vector3D.Zero)
                        //    bounceVector = _cubeGrid.Physics.LinearVelocity;
                        
                        
                        avoidanceVector.Normalize();
                        //ShowVectorOnHud(_cubeGrid.GetPosition(), avoidanceVector * 100);
                        //ShowVectorOnHud(_cubeGrid.GetPosition(), avoidTargetVector);
                        navigation.ThrustTwordsDirection(avoidTargetVector);
                        AlignTo(_cubeGrid.GetPosition() - avoidTargetVector);
                        //ShowLocationOnHud(_cubeGrid.GetPosition() + bounceVector);
                        breakawayDistance = 150;
                        attacking = false;
                    }
                }
                else
                {
                    var avoidTargetVector = (avoidanceVector);
                    navigation.ThrustTwordsDirection(avoidTargetVector);
                    AlignTo(_cubeGrid.GetPosition() - avoidTargetVector);
                    //Orbit(new Vector3D(0,0,0));
                }

                //Util.NotifyHud("num targets: "+weaponControls.GetNumberTargets());
                
                ////if (avoidanceVector != Vector3D.Zero)
                ////{
                ////    Util.NotifyHud(avoidanceVector.Normalize()+"");
                ////    navigation.ThrustTwordsDirection(avoidanceVector);
                ////}
                ////else if (targetLocked)
                ////    navigation.(target.Ship.GetPosition(), 100);
                ////else
                



                //weaponControls.DebugMarkTargetAndKeyPoint();
                //IMyTerminalBlock closiestKeyBlock = weaponControls.GetTargetKeyAttackPoint(targetGrid);

                //Vector3D keyPointPos = closiestKeyBlock.GetPosition();

                //weaponControls.MarkAllTrackedObjects();
                //Util.NotifyHud("alive: " + healthPercent);
            }
            else
            {
                //Util.NotifyHud("dead: " + healthPercent);
            }
            
            //check if alive - display
            //Update nearby objects

            //figure out current order
            //Calculate avoidance if needed
            //Calculate if in combat range of target

            Broadcast();

            return isOperational;
        }

        private void ShowLocationOnHud(Vector3D position)
        {

            long id = MyAPIGateway.Session.Player.IdentityId;

            IMyGps mygps = MyAPIGateway.Session.GPS.Create("=", "", position, true, true);
            MyAPIGateway.Session.GPS.AddGps(id, mygps);
        }

        private void ShowVectorOnHud(Vector3D position, Vector3D direction)
        {
            var color = Color.Red.ToVector4();
            MySimpleObjectDraw.DrawLine(position, position + direction, "null or transparent material", ref color, .1f);
        }

        private int collisionDistance = 200;
        private int avoidanceRange = 300;
        private Vector3D avoidanceVector = Vector3D.Zero;
        private void CalculateAvoidanceVectors()
        {
            List<TargetDetails> closiest = weaponControls.GetObjectsInRange(avoidanceRange);
            List<IMyVoxelBase> asteroids = weaponControls.GetAsteroids(avoidanceRange);

            int count = 0;
            avoidanceVector = Vector3D.Zero;
            foreach (var target in closiest)
            {
                count ++;
                Vector3D vector = (_cubeGrid.GetPosition() - target.Ship.GetPosition());
                //ShowVectorOnHud(target.Ship.GetPosition(), vector);
                avoidanceVector = avoidanceVector + vector;
            }
            avoidanceVector = avoidanceVector/count;
            //ShowVectorOnHud(_cubeGrid.GetPosition(), avoidanceVector);
            double maxvalue = avoidanceVector.X > avoidanceVector.Y ? avoidanceVector.X > avoidanceVector.Z ? avoidanceVector.X : avoidanceVector.Z : avoidanceVector.Y > avoidanceVector.Z ? avoidanceVector.Y : avoidanceVector.Z;
            var stepDirection = avoidanceVector / Math.Abs(maxvalue);
            avoidanceVector = stepDirection;
        }
    }
}
