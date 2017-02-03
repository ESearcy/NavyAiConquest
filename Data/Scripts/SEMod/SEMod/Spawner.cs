using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace SEMod
{
    class Spawner
    {
        private String _logPath = "Spawner";
        public Spawner()
        {
            map.Add(ShipTypes.NavyFighter, "-DC-Stinger");
            map.Add(ShipTypes.NavyFrigate, "-DC-Praetorian");
            //map.Add(2, "-DC-Swarmer");
            //map.Add(4, "-DC-Tusker");
            //map.Add(5, "-DC-Buzzer");
        }

        private Vector3D GetPositionWithinAnyPlayerViewDistance(Vector3D pos)
        {
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            var playerViewDistance = MyAPIGateway.Session.SessionSettings.ViewDistance;
            Vector3D position = Vector3D.Zero;

            if (players.Any())
            {
                position = (players.OrderBy(x => (x.GetPosition() - pos).Length()).First().GetPosition() + new Vector3D(0, 0, playerViewDistance * .85));
            }

            return position;
        }

        private IMyGps marker = null;
        private Dictionary<ShipTypes, String> map = new Dictionary<ShipTypes, String>();

        public void SpawnShip(ShipTypes type, Vector3D location, long ownerid)
        {
            try
            {

                var definitions = MyDefinitionManager.Static.GetPrefabDefinitions();
                foreach (var item in definitions)
                {
                    Util.Log(_logPath, "ShipName: " + item.Key);
                }

                var t = MyDefinitionManager.Static.GetPrefabDefinition(map[type]);

                if (t == null)
                {
                    Util.Log("Failed To Load Ship: " + map[type], "Spawner.txt");
                    return;
                }

                var s = t.CubeGrids;
                s = (MyObjectBuilder_CubeGrid[])s.Clone();

                if (s.Length == 0)
                {
                    return;
                }

                Vector3I min = Vector3I.MaxValue;
                Vector3I max = Vector3I.MinValue;

                s[0].CubeBlocks.ForEach(b => min = Vector3I.Min(b.Min, min));
                s[0].CubeBlocks.ForEach(b => max = Vector3I.Max(b.Min, max));
                float size = new Vector3(max - min).Length();

                var freeplace = MyAPIGateway.Entities.FindFreePlace(location, size * 5f);
                if (freeplace == null)
                    return;

                var newPosition = (Vector3D)freeplace;

                var grid = s[0];
                if (grid == null)
                {
                    Util.Log(_logPath, "A CubeGrid is null!");
                    return;
                }

                List<IMyCubeGrid> shipMade = new List<IMyCubeGrid>();

                var spawnpoint = GetPositionWithinAnyPlayerViewDistance(newPosition);
                var safespawnpoint = MyAPIGateway.Entities.FindFreePlace(spawnpoint, size * 5f);
                spawnpoint = safespawnpoint is Vector3D ? (Vector3D)safespawnpoint : new Vector3D();

                //to - from
                var direction = newPosition - spawnpoint;
                var finalSpawnPoint = location;//(direction / direction.Length()) * (MyAPIGateway.Session.SessionSettings.ViewDistance * .85);
                
                MyAPIGateway.PrefabManager.SpawnPrefab(shipMade, map[type], finalSpawnPoint, Vector3.Forward, Vector3.Up, Vector3.Zero, default(Vector3), null, SpawningOptions.SpawnRandomCargo, ownerid);
                
                //MyAPIGateway.PrefabManager.SpawnPrefab(shipMade, map[type], newPosition, Vector3.Forward, Vector3.Up);
                var bs = new BoundingSphereD(finalSpawnPoint, 500);
                var ents = MyAPIGateway.Entities.GetEntitiesInSphere(ref bs);
                var closeBy = ents;

            }
            catch (Exception e)
            {
                Util.LogError(_logPath, e.ToString());
            }
        }
    }
}
