using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CarControlls.Models;
using DieptidiUtility;
using Rage;

namespace CarControlls
{
    public class CarControllEntryPoint
    {
        public List<Blip> VehicleBlips;
        public List<Kendaraan> Kendaraans { get; set; }
        public List<Vehicle> Vehicles;
        public readonly string carLockDictionaryAnimation = "anim@mp_player_intmenu@key_fob@";
        public readonly string carLockActionAnimation = "fob_click_fp";

        public CarControllEntryPoint()
        {
            if (!Directory.Exists(StorageController.DirVehicleLocation))
                Directory.CreateDirectory(StorageController.DirVehicleLocation);
        }

        public void StartVehicleLockingSystem()
        {
            try
            {
                Entity frontEntity = Helper.GetEntityInFrontPlayer();
                if (Game.LocalPlayer.Character.IsOnFoot && frontEntity != null && frontEntity is Vehicle)
                {
                    Vehicle vehicleOnFront = (Vehicle)frontEntity;
                    if (vehicleOnFront.LockStatus == VehicleLockStatus.Locked)
                    {
                        Unlocking(vehicleOnFront);
                        return;
                    }

                    Locking(vehicleOnFront);
                    return;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GetFuel()
        {
            Entity frontEntity = Helper.GetEntityInFrontPlayer();
            if (frontEntity == null)
            {
                frontEntity = Game.LocalPlayer.Character.CurrentVehicle;
            }
            if (frontEntity != null && frontEntity is Vehicle)
            {
                Vehicle vehicleOnFront = (Vehicle)frontEntity;
                return "Fuel: " + Math.Floor((decimal)vehicleOnFront.FuelLevel).ToString();
            }

            return "Fuel: ~";
        }

        public void TurnEngine(Vehicle vehicle, bool isOn)
        {
            Rage.Native.NativeFunction.Natives.SET_VEHICLE_ENGINE_ON(vehicle, isOn, false, true);
        }

        public void CloseAllDoor(Vehicle vehicle)
        {
            Rage.Native.NativeFunction.Natives.SET_VEHICLE_DOORS_SHUT(vehicle, false);
        }

        void BlinkingLight(Vehicle vehicle)
        {
            Rage.Native.NativeFunction.Natives.SET_VEHICLE_LIGHTS(vehicle, 2);
            GameFiber.Yield();
            GameFiber.Wait(200);
            Rage.Native.NativeFunction.Natives.SET_VEHICLE_LIGHTS(vehicle, 0);
            GameFiber.Wait(200);
            Rage.Native.NativeFunction.Natives.SET_VEHICLE_LIGHTS(vehicle, 2);
            GameFiber.Wait(400);
            Rage.Native.NativeFunction.Natives.SET_VEHICLE_LIGHTS(vehicle, 0);
        }

        void Unlocking(Vehicle vehicle)
        {
            var _blip = VehicleBlips.Find(b => b.Name == $"{vehicle.Model.Name}-{vehicle.LicensePlate}");
            if (_blip != null && StorageController.IsPlayerVehicle(vehicle))
            {
                _blip.Delete();
                VehicleBlips.Remove(_blip);

                vehicle.LockStatus = VehicleLockStatus.Unlocked;
                VehicleLockingFinishing("Unlocked", vehicle, (k) => StorageController.DeleteVehicle(k));
            }
        }
        void Locking(Vehicle vehicle)
        {
            if (Helper.IsLastVehicleExist() && Helper.IsThisVehicleAreLastVehicle(vehicle))
            {
                AddVehicleBlip(vehicle);
                if (!Vehicles.Any(v => v.IsValid() && v.LicensePlate == vehicle.LicensePlate)) Vehicles.Add(vehicle);

                vehicle.LockStatus = VehicleLockStatus.Locked;
                VehicleLockingFinishing("Locked", vehicle, (k) => StorageController.SaveVehicle(k));
            }
        }
        public void ReSpawnSavedVehicle()
        {
            if (Vehicles.Any(v => v.IsValid() == false))
            {
                foreach (var veh in Vehicles)
                {
                    if (veh.IsValid() == true)
                    {
                        veh.Delete();
                    }
                }

                Vehicles.Clear();

                foreach (var ken in Kendaraans)
                {
                    Vehicles.Add(ken.Spawn());
                }
            }
        }

        void AddVehicleBlip(Vehicle vehicle)
        {
            string blipName = string.Format("{0}-{1}", vehicle.Model.Name, vehicle.LicensePlate);
            Blip _blip = Helper.CreateBlip(vehicle.Position, blipName, Color.PeachPuff, BlipSprite.GangVehicle);
            VehicleBlips.Add(_blip);
        }

        void VehicleLockingFinishing(string action, Vehicle vehicle, Action<Kendaraan> storage)
        {
            int ANIMATION_DURATION = 1000;
            int DRAW_TEXT_DURATION = 100;
            GameFiber.StartNew(() => BlinkingLight(vehicle));
            storage(new Kendaraan(vehicle));
            Helper.PlayingAnimation(carLockDictionaryAnimation, carLockActionAnimation, ANIMATION_DURATION);
            GameFiber.StartNew(() => Helper.Draw3dText(vehicle.Position, action, DRAW_TEXT_DURATION));
        }
    }
}
