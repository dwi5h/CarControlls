using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CarControlls.Models;
using DieptidiUtility;
using Rage;

//[assembly: Rage.Attributes.Plugin("CarControlls", Description = "Car Controlling System", Author = "Dieptidi",
//    EntryPoint = "CarControlls.EntryPoint.Main", ExitPoint = "CarControlls.EntryPoint.OnUnload")]
namespace CarControlls
{
    public class CarControllEntryPoint
    {
        static List<Blip> vehicleBlips;
        static List<Kendaraan> kendaraans;
        static List<Vehicle> vehicles;
        static string carLockDictionaryAnimation = "anim@mp_player_intmenu@key_fob@";
        static string carLockActionAnimation = "fob_click_fp";
        public static void Main()
        {
            kendaraans = StorageController.LoadAllKendaraan(out vehicles, out vehicleBlips);
            GameFiber.StartNew(() => Helper.RequestAnimDict(carLockDictionaryAnimation));
        }

        public static void OnUnload(bool isTerminating)
        {
            if (isTerminating)
            {
                foreach (var _blip in vehicleBlips)
                {
                    _blip.Delete();
                }

                foreach (var _vehicle in vehicles)
                {
                    _vehicle.Delete();
                }
            }
        }

        public static void StartVehicleLockingSystem()
        {
            Entity fontEntity = Helper.GetEntityInFrontPlayer();
            if (Game.LocalPlayer.Character.IsOnFoot && fontEntity != null && fontEntity is Vehicle)
            {
                Vehicle vehicleOnFront = (Vehicle)fontEntity;
                if (vehicleOnFront.LockStatus == VehicleLockStatus.Locked)
                {
                    Unlocking(vehicleOnFront);
                    return;
                }

                Locking(vehicleOnFront);
                return;
            }
        }

        static void Unlocking(Vehicle vehicle)
        {
            var _blip = vehicleBlips.Find(b => b.Name == $"{vehicle.Model.Name}-{vehicle.LicensePlate}");
            if (_blip != null && StorageController.IsPlayerVehicle(vehicle))
            {
                _blip.Delete();
                vehicleBlips.Remove(_blip);

                vehicle.LockStatus = VehicleLockStatus.Unlocked;
                VehicleLockingFinishing("Unlocked", vehicle, (k) => StorageController.DeleteVehicle(k));
            }
        }
        static void Locking(Vehicle vehicle)
        {
            if (Helper.IsLastVehicleExist() && Helper.IsThisVehicleAreLastVehicle(vehicle))
            {
                AddVehicleBlip(vehicle);
                if (!vehicles.Any(v => v.LicensePlate == vehicle.LicensePlate)) vehicles.Add(vehicle);

                vehicle.LockStatus = VehicleLockStatus.Locked;
                VehicleLockingFinishing("Locked", vehicle, (k) => StorageController.SaveVehicle(k));
            }
        }

        static void AddVehicleBlip(Vehicle vehicle)
        {
            string blipName = string.Format("{0}-{1}", vehicle.Model.Name, vehicle.LicensePlate);
            Blip _blip = Helper.CreateBlip(vehicle.Position, blipName, Color.PeachPuff, BlipSprite.GangVehicle);
            vehicleBlips.Add(_blip);
        }

        static void VehicleLockingFinishing(string action, Vehicle vehicle, Action<Kendaraan> storage)
        {
            int ANIMATION_DURATION = 1000;
            int DRAW_TEXT_DURATION = 100;
            storage(new Kendaraan(vehicle));
            Helper.PlayingAnimation(carLockDictionaryAnimation, carLockActionAnimation, ANIMATION_DURATION);
            GameFiber.StartNew(() => Helper.Draw3dText(vehicle.Position, action, DRAW_TEXT_DURATION));
        }
    }
}
