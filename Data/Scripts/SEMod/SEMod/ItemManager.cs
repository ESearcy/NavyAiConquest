using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

namespace SEMod
{
    class ItemManager
    {

        private static string logPath = "ItemManager.txt";
        private static SerializableDefinitionId _gatlingAmmo;
        private static SerializableDefinitionId _launcherAmmo;
        private static SerializableDefinitionId _uraniumFuel;
        public ItemManager()
        {

            Util.Log(logPath, "inside constructor");
            _gatlingAmmo = new SerializableDefinitionId(new MyObjectBuilderType(new MyObjectBuilder_AmmoMagazine().GetType()), "NATO_25x184mm");

            Util.Log(logPath, _gatlingAmmo + " ammo1");
            _launcherAmmo = new SerializableDefinitionId(new MyObjectBuilderType(new MyObjectBuilder_AmmoMagazine().GetType()), "Missile200mm");

            Util.Log(logPath, _launcherAmmo + " ammo1");
            _uraniumFuel = new SerializableDefinitionId(new MyObjectBuilderType(new MyObjectBuilder_Ingot().GetType()), "Uranium");

            Util.Log(logPath, _uraniumFuel + " ammo1");
        }


        public void Reload(List<IMyTerminalBlock> guns)
        {
            for (int i = 0; i < guns.Count; i++)
            {
                if (IsAGun((MyEntity)guns[i]))
                    Reload((MyEntity)guns[i], _gatlingAmmo);
                else
                    Reload((MyEntity)guns[i], _launcherAmmo);
            }
        }

        private void Reload(MyEntity gun, SerializableDefinitionId ammo, bool reactor = false)
        {
            var cGun = gun;
            MyInventory inv = cGun.GetInventory(0);
            VRage.MyFixedPoint point = inv.GetItemAmount(ammo, MyItemFlags.None | MyItemFlags.Damaged);

            if (point.RawValue > 1000000)
                return;
            //inv.Clear();
            VRage.MyFixedPoint amount = new VRage.MyFixedPoint();
            amount.RawValue = 2000000;
            Util.Log(logPath, ammo.SubtypeName + " [ReloadGuns] Amount " + amount);
            MyObjectBuilder_InventoryItem ii;
            if (reactor)
            {

                Util.Log(logPath, ammo.SubtypeName + " [ReloadGuns] loading reactor " + point.RawValue);
                ii = new MyObjectBuilder_InventoryItem()
                {
                    Amount = 10,
                    Content = new MyObjectBuilder_Ingot() { SubtypeName = ammo.SubtypeName }
                };
                Util.Log(logPath, ammo.SubtypeName + " [ReloadGuns] loading reactor 2 " + point.RawValue);
            }
            else
            {

                Util.Log(logPath, ammo.SubtypeName + " [ReloadGuns] loading guns " + point.RawValue);
                ii = new MyObjectBuilder_InventoryItem()
                {
                    Amount = 4,
                    Content = new MyObjectBuilder_AmmoMagazine() { SubtypeName = ammo.SubtypeName }
                };
                Util.Log(logPath, ammo.SubtypeName + " [ReloadGuns] loading guns 2 " + point.RawValue);
            }
            //inv.
            Util.Log(logPath, amount + " Amount : content " + ii.Content);
            inv.AddItems(amount, ii.Content);


            point = inv.GetItemAmount(ammo, MyItemFlags.None | MyItemFlags.Damaged);
        }

        private bool IsAGun(MyEntity gun)
        {
            return gun is IMyLargeTurretBase;
        }

        public void ReloadReactors(List<IMyTerminalBlock> reactors)
        {
            for (int i = 0; i < reactors.Count; i++)
            {

                Reload((MyEntity)reactors[i], _uraniumFuel, true);
            }
        }

    }
}
