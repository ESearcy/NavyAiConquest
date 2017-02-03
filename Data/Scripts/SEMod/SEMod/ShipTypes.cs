using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEMod
{
    enum ShipTypes
    {
        NavyFighter = 0,
        NavyFrigate = 1,
        NotADrone = 2
    }

    enum DroneWeaponActions
    {
        Standby,
        Attacking,
        LockedOn
    }

    enum DroneNavigationActions
    {
        Stationary,
        Approaching,
        Orbiting,
        Avoiding
    }

    enum OrbitTypes
    {
        X, XY, Y, YZ, Z, XZ,
        Default
    }

}
