using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Drawing;
using Rage;
using System;

namespace CarControlls.Models
{
    class Kendaraan
    {
        public VehicleWheelType wheelType;
        public int wheelModIndex;
        public string ModelName { get; set; }
        public uint ModelHash { get; set; }
        public SimpleVector3 Position { get; set; }
        public float Heading { get; set; }
        public string LicensePlate { get; set; }
        public LicensePlateStyle LicensePlateStyle { get; set; }
        public float DirtLevel { get; set; }
        public int Health { get; set; }
        public float EngineHealth { get; set; }
        public float FuelTankHealth { get; set; }
        public bool CanTiresBurst { get; set; }
        public float FuelLevel { get; set; }
        public VehicleClass Class { get; set; }
        public VehicleConvertibleRoofState ConvertibleRoofState { get; set; }
        public Color PrimaryColor { get; set; }
        public Color SecondaryColor { get; set; }
        public Color PearlescentColor { get; set; }
        public Color RimColor { get; set; }
        public Color TrimColor { get; set; }
        public int WindowTint { get; set; }
        public List<KendaraanMod> Mods { get; set; }

        public Kendaraan()
        {
            Mods = new List<KendaraanMod>();
        }

        public Kendaraan(Vehicle vehicle)
        {
            ModelName = GetVehicleName(vehicle);
            ModelHash = vehicle.Model.Hash;
            Position = new SimpleVector3(vehicle.Position);
            Heading = vehicle.Heading;
            vehicle.Mods.GetWheelMod(out wheelType, out wheelModIndex);
            LicensePlate = vehicle.LicensePlate;
            LicensePlateStyle = vehicle.LicensePlateStyle;
            DirtLevel = vehicle.DirtLevel;
            Health = vehicle.Health;
            EngineHealth = vehicle.EngineHealth;
            FuelTankHealth = vehicle.FuelTankHealth;
            FuelLevel = vehicle.FuelLevel;
            Class = vehicle.Class;
            ConvertibleRoofState = vehicle.ConvertibleRoofState;
            CanTiresBurst = vehicle.CanTiresBurst;
            PrimaryColor = vehicle.PrimaryColor;
            SecondaryColor = vehicle.SecondaryColor;
            PearlescentColor = vehicle.PearlescentColor;
            RimColor = vehicle.RimColor;
            WindowTint = Rage.Native.NativeFunction.CallByName<int>("GET_VEHICLE_WINDOW_TINT", vehicle);

            Mods = GetMods(vehicle);
        }

        public Vehicle Spawn()
        {
            Model model = new Model(ModelHash);
            Vehicle vehicle = new Vehicle(model, Position.ToVector3(), Heading);
            vehicle.LockStatus = VehicleLockStatus.Locked;
            vehicle.DirtLevel = DirtLevel;
            vehicle.Health = Health;
            vehicle.EngineHealth = EngineHealth;
            vehicle.FuelTankHealth = FuelTankHealth;
            vehicle.FuelLevel = FuelLevel;
            vehicle.ConvertibleRoofState = ConvertibleRoofState;
            vehicle.CanTiresBurst = CanTiresBurst;
            vehicle.LicensePlate = LicensePlate;
            vehicle.LicensePlateStyle = LicensePlateStyle;
            vehicle.PrimaryColor = PrimaryColor;
            vehicle.SecondaryColor = SecondaryColor;
            vehicle.PearlescentColor = PearlescentColor;
            vehicle.RimColor = RimColor;
            Rage.Native.NativeFunction.CallByName<int>("SET_VEHICLE_WINDOW_TINT", vehicle, WindowTint);
            vehicle.Mods.InstallModKit();

            vehicle.Mods.SetWheelMod(wheelType, wheelModIndex, false);
            foreach (var myMod in Mods)
            {
                Rage.Native.NativeFunction.Natives.SET_VEHICLE_MOD(vehicle, myMod.Type, myMod.Index, false);
            }
            //}
            //}
            return vehicle;
        }

        Tuple<string, int>[] VehicleModType =
        {
            new Tuple<string, int>("SPOILER", 0),
            new Tuple<string, int>("BUMPER_F", 1),
            new Tuple<string, int>("BUMPER_R", 2),
            new Tuple<string, int>("SKIRT", 3),
            new Tuple<string, int>("EXHAUST", 4),
            new Tuple<string, int>("CHASSIS", 5),
            new Tuple<string, int>("GRILL", 6),
            new Tuple<string, int>("BONNET", 7),
            new Tuple<string, int>("WING_L", 8),
            new Tuple<string, int>("WING_R", 9),
            new Tuple<string, int>("ROOF", 10),
            new Tuple<string, int>("ENGINE", 11),
            new Tuple<string, int>("BRAKES", 12),
            new Tuple<string, int>("GEARBOX", 13),
            new Tuple<string, int>("HORN", 14),
            new Tuple<string, int>("SUSPENSION", 15),
            new Tuple<string, int>("ARMOUR", 16),
            new Tuple<string, int>("NITROUS", 17),
            new Tuple<string, int>("TURBO", 18),
            new Tuple<string, int>("SUBWOOFER", 19),
            new Tuple<string, int>("TYRE_SMOKE", 20),
            new Tuple<string, int>("HYDRAULICS", 21),
            new Tuple<string, int>("XENON_LIGHTS", 22),
            new Tuple<string, int>("PLTHOLDER", 25),
            new Tuple<string, int>("PLTVANITY", 26),
            new Tuple<string, int>("INTERIOR1", 27),
            new Tuple<string, int>("INTERIOR2", 28),
            new Tuple<string, int>("INTERIOR3", 29),
            new Tuple<string, int>("INTERIOR4", 30),
            new Tuple<string, int>("INTERIOR5", 31),
            new Tuple<string, int>("SEATS", 32),
            new Tuple<string, int>("STEERING", 33),
            new Tuple<string, int>("KNOB", 34),
            new Tuple<string, int>("PLAQUE", 35),
            new Tuple<string, int>("ICE", 36),
            new Tuple<string, int>("TRUNK", 37),
            new Tuple<string, int>("HYDRO", 38),
            new Tuple<string, int>("ENGINEBAY1", 39),
            new Tuple<string, int>("ENGINEBAY2", 40),
            new Tuple<string, int>("ENGINEBAY3", 41),
            new Tuple<string, int>("CHASSIS2", 42),
            new Tuple<string, int>("CHASSIS3", 43),
            new Tuple<string, int>("CHASSIS4", 44),
            new Tuple<string, int>("CHASSIS5", 45),
            new Tuple<string, int>("DOOR_L", 46),
            new Tuple<string, int>("DOOR_R", 47),
            new Tuple<string, int>("LIVERY_MOD", 48),
            new Tuple<string, int>("LIGHTBAR", 49)
        };

        List<KendaraanMod> GetMods(Vehicle vehicle)
        {
            var mods = new List<KendaraanMod>();
            foreach (var type in VehicleModType)
            {
                int modIndex = Rage.Native.NativeFunction.CallByName<int>("GET_VEHICLE_MOD", vehicle, type.Item2);
                if (modIndex >= 0)
                {
                    mods.Add(new KendaraanMod(type.Item2, type.Item1, modIndex));
                }
            }
            return mods;
        }

        public static string GetVehicleName(Vehicle vehicle)
        {
            return Rage.Native.NativeFunction.Natives.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL<string>(vehicle.Model.Hash);
        }
    }
}
