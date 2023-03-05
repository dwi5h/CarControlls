using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CarControlls.Models;
using Rage;

[assembly: Rage.Attributes.Plugin("CarControlls", Description = "Car Controlling System", Author = "Dieptidi",
    EntryPoint = "CarControlls.Temp.Main", ExitPoint = "CarControlls.Temp.OnUnload")]
namespace CarControlls
{
    public class Temp
    {
        static bool active = false;
        static Entity ent;
        static Vector3 pos;
        static List<Blip> vehicleBlips;
        static List<Kendaraan> kendaraans;
        static List<Vehicle> vehicles;
        public static void Main()
        {
            kendaraans = StorageController.LoadAllKendaraan(out vehicles, out vehicleBlips);
            GameFiber.StartNew(loadAnimDict);
            while (true)
            {
                Tick();
                GameFiber.Yield();
            }
        }

        static void InitialKendaraans()
        {
            kendaraans = StorageController.LoadAllKendaraan(out vehicles, out vehicleBlips);
            GameFiber.Wait(100);
            GameFiber.Yield();
        }

        private static void OnUnload(bool isTerminating)
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

        static void Tick()
        {
            if (active && ent != null && ent is Vehicle)
            {
                //Vehicle veh = (Vehicle)ent;
                //Draw3dText(pos, veh.LockStatus.ToString());
            }
            KeyPressed();
            //SpawnNearPlayerVehicle();
        }

        static void SpawnNearPlayerVehicle()
        {
            foreach (var _kendaraan in kendaraans)
            {
                if (GetDistanceBetweenCoord(_kendaraan.Position.ToVector3(), Game.LocalPlayer.Character.Position) < 400f)
                {
                    Vehicle _vehicle = vehicles.Find(v => v.LicensePlate == _kendaraan.LicensePlate);
                    if (_vehicle == null)
                    {
                        vehicles.Add(_kendaraan.Spawn());
                    }
                }
            }
        }

        static float GetDistanceBetweenCoord(Vector3 pos1, Vector3 pos2)
        {
            return Rage.Native.NativeFunction.CallByName<float>("GET_DISTANCE_BETWEEN_COORDS",
                pos1.X, pos1.Y, pos1.Z, pos2.X, pos2.Y, pos2.Z, true);
        }

        static void LockUnlockVehicle()
        {
            if (Game.LocalPlayer.Character.IsOnFoot && ent != null && ent is Vehicle)
            {
                Vehicle veh = (Vehicle)ent;
                //if (veh.LockStatus == VehicleLockStatus.Locked)
                if (veh.LockStatus == VehicleLockStatus.Locked && StorageController.IsPlayerVehicle(veh))
                {
                    var _blip = vehicleBlips.Find(b => b.Name == $"{veh.Model.Name}-{veh.LicensePlate}");
                    if (_blip != null)
                    {
                        _blip.Delete();
                        vehicleBlips.Remove(_blip);
                        //Vehicle _veh = vehicles.Find(v => v.LicensePlate == veh.LicensePlate);
                        //if (_veh != null) vehicles.Remove(_veh);
                        veh.LockStatus = VehicleLockStatus.Unlocked;
                        StorageController.DeleteVehicle(new Kendaraan(veh));
                        PlayingAnimation();
                        Game.DisplayNotification("Car Unlocked");
                    }
                }
                else if (veh.LockStatus != VehicleLockStatus.Locked && Game.LocalPlayer.Character.LastVehicle != null &&
                    veh.LicensePlate == Game.LocalPlayer.Character.LastVehicle.LicensePlate)
                {
                    Blip _blip = CreateBlip(veh.Position.X, veh.Position.Y, veh.Position.Z,
                        $"{veh.Model.Name}-{veh.LicensePlate}", Color.PeachPuff, BlipSprite.GangVehicle);
                    vehicleBlips.Add(_blip);
                    veh.LockStatus = VehicleLockStatus.Locked;
                    StorageController.SaveVehicle(new Kendaraan(veh));
                    PlayingAnimation();
                    Game.DisplayNotification("Vehicle saved");
                    Game.DisplayNotification("Car Locked");
                    Game.DisplayNotification("vehicleBlips: " + vehicleBlips.Count());
                }
                //veh.LockStatus = veh.LockStatus == VehicleLockStatus.Locked ? VehicleLockStatus.Unlocked : VehicleLockStatus.Locked;
            }
        }

        static void loadAnimDict()
        {
            bool isAnimPlayed = false;
            while (!isAnimPlayed)
            {
                string dict = "anim@mp_player_intmenu@key_fob@";
                Rage.Native.NativeFunction.Natives.REQUEST_ANIM_DICT(dict);
                isAnimPlayed = Rage.Native.NativeFunction.CallByName<bool>(
                    "HAS_ANIM_DICT_LOADED", dict);
                GameFiber.Yield();
            }
            Game.DisplayNotification("Anim Dict Loaded");
        }

        static void PlayingAnimation()
        {
            string dict = "anim@mp_player_intmenu@key_fob@";
            Rage.Native.NativeFunction.Natives.TASK_PLAY_ANIM(
                Game.LocalPlayer.Character, dict, "fob_click_fp", 2.0f, 8.0f, 1000, 50, 0f, false, false, false);
        }

        private static void GetEntityName()
        {
            //try
            //{
            //if (active)
            //{
            var position = Game.LocalPlayer.Character.GetOffsetPosition(new Vector3(0, 0.5f, 0));
            var ents = World.GetAllEntities();
            ents = ents.Where(x => x.Model.Name != "PLAYER_ONE").ToArray();
            ent = World.GetClosestEntity(ents, position);
            pos = ent.Position;
            LockUnlockVehicle();
            //}
            //}
            //catch (Exception ex)
            //{
            //    Game.DisplayNotification("~r~Error: ~w~" + ex.Message);
            //}
        }

        private static void KeyPressed()
        {
            if (Game.IsKeyDown(Keys.E))
            {
                //active = !active;
                GetEntityName();
                Game.DisplayNotification("E Pressesd");
            }

            if (Game.IsKeyDown(Keys.X))
            {
                Rage.Native.NativeFunction.Natives.CLEAR_PED_TASKS(Game.LocalPlayer.Character);
                //active = !active;
                //var kendaraan = StorageController.LoadAllKendaraan()[0];
                //kendaraan.Spawn();
            }

            if (Game.IsKeyDown(Keys.Z))
            {
                if (ent != null && ent is Vehicle)
                {
                    Kendaraan kendaraan = new Kendaraan((Vehicle)ent);
                    StorageController.SaveVehicle(kendaraan);
                    Game.DisplayNotification("Vehicle saved");
                }
            }
        }

        private static void Draw3dText(Vector3 position, string text)
        {
            try
            {
                Rage.Native.NativeFunction.Natives.SET_DRAW_ORIGIN(position.X, position.Y, position.Z, 0);
                Rage.Native.NativeFunction.Natives.SET_TEXT_FONT(0);
                Rage.Native.NativeFunction.Natives.SET_TEXT_SCALE(10.0f, 0.555f);
                Rage.Native.NativeFunction.Natives.SET_TEXT_COLOUR(255, 255, 255, 255);
                Rage.Native.NativeFunction.Natives.SET_TEXT_CENTRE(true);
                Rage.Native.NativeFunction.Natives.SET_TEXT_DROPSHADOW(0, 0, 0, 0, 0);
                Rage.Native.NativeFunction.Natives.SET_TEXT_EDGE(0, 0, 0, 0, 0);
                Rage.Native.NativeFunction.Natives.BEGIN_TEXT_COMMAND_DISPLAY_TEXT("STRING");
                Rage.Native.NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(text);
                Rage.Native.NativeFunction.Natives.END_TEXT_COMMAND_DISPLAY_TEXT(0, 0);
                Rage.Native.NativeFunction.Natives.CLEAR_DRAW_ORIGIN();
            }
            catch (Exception ex)
            {
                Game.DisplayNotification("~r~Error: ~w~" + ex.Message);
            }
        }

        private static void DrawTextOnScreen(string text)
        {
            try
            {
                Rage.Native.NativeFunction.Natives.SET_TEXT_FONT(0);
                Rage.Native.NativeFunction.Natives.SET_TEXT_SCALE(10.0f, 0.555f);
                Rage.Native.NativeFunction.Natives.SET_TEXT_COLOUR(255, 255, 255, 255);
                Rage.Native.NativeFunction.Natives.SET_TEXT_CENTRE(false);
                Rage.Native.NativeFunction.Natives.SET_TEXT_DROPSHADOW(0, 0, 0, 0, 0);
                Rage.Native.NativeFunction.Natives.BEGIN_TEXT_COMMAND_DISPLAY_TEXT("STRING");
                Rage.Native.NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(text);
                Rage.Native.NativeFunction.Natives.END_TEXT_COMMAND_DISPLAY_TEXT(0.23f, 0.93f);
            }
            catch (Exception ex)
            {
                Game.DisplayNotification("~r~Error: ~w~" + ex.Message);
            }
        }

        public static Blip CreateBlip(float x, float y, float z, string name, Color color, BlipSprite sprite)
        {
            Blip _blip = new Blip(new Vector3(x, y, z));
            _blip.Sprite = sprite;
            _blip.Color = color;
            _blip.Name = name;
            return _blip;
        }
    }
}
