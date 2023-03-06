using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CarControlls.Models;
using DieptidiUtility;
using Rage;

namespace CarControlls
{
    class StorageController
    {
        static readonly string DirLocation = Directory.GetCurrentDirectory() + @"\plugins\DieptidiMenu\CarControlls";

        public static void SaveVehicle(Kendaraan kendaraan)
        {
            var serializedVehicle = JsonConvert.SerializeObject(kendaraan, Formatting.Indented);

            try
            {
                File.WriteAllText(Path.Combine(DirLocation + @"\Kendaraan", GetVehicleFileName(kendaraan)), serializedVehicle, Encoding.UTF8);
                var vehicleFilePath = Path.Combine(DirLocation + @"\Kendaraan", GetVehicleFileNameDelete(kendaraan));
                File.Delete(vehicleFilePath);
            }
            catch (Exception e)
            {
                throw new Exception($"Couldn't save vehicle file '{GetVehicleFileName(kendaraan)}' to folder '{DirLocation}'. Error message: '{e.Message}'", e);
            }
        }

        public static List<Kendaraan> LoadAllKendaraan(out List<Vehicle> vehicles, out List<Blip> blips)
        {
            string[] fileList = Directory.GetFiles(DirLocation + @"\Kendaraan", "*.json");

            List<Kendaraan> kendaraans = new List<Kendaraan>();
            vehicles = new List<Vehicle>();
            blips = new List<Blip>();

            for (int i = 0; i < fileList.Length; i++)
            {
                try
                {
                    if (!fileList[i].Contains("[isDeleted]"))
                    {
                        Kendaraan kendaraan = JsonConvert.DeserializeObject<Kendaraan>(File.ReadAllText(fileList[i]));
                        kendaraans.Add(kendaraan);
                        vehicles.Add(kendaraan.Spawn());
                        blips.Add(Helper.CreateBlip(kendaraan.Position.ToVector3(),
                            $"{kendaraan.ModelName}-{kendaraan.LicensePlate}", Color.PeachPuff, BlipSprite.GangVehicle));
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Couldn't load vehicle from file '{fileList[i]}'. Fix or remove this file, and try again. Error message: '{e.Message}'", e);
                }
            };

            return kendaraans;
        }

        public static void DeleteVehicle(Kendaraan vehicle)
        {
            var vehicleFilePath = Path.Combine(DirLocation + @"\Kendaraan", GetVehicleFileName(vehicle));

            if (!File.Exists(vehicleFilePath))
                return;

            try
            {
                File.Delete(vehicleFilePath);
                var serializedVehicle = JsonConvert.SerializeObject(vehicle, Formatting.Indented);
                File.WriteAllText(Path.Combine(DirLocation + @"\Kendaraan", GetVehicleFileNameDelete(vehicle)), serializedVehicle, Encoding.UTF8);
            }
            catch (Exception e)
            {
                throw new Exception($"Couldn't delete vehicle file '{GetVehicleFileName(vehicle)}' from folder '{DirLocation}. Error message: '{e.Message}'", e);
            }
        }

        public static bool IsPlayerVehicle(Vehicle vehicle)
        {
            var vehicleFilePath = Path.Combine(DirLocation + @"\Kendaraan", GetVehicleFileName(vehicle));

            if (!File.Exists(vehicleFilePath))
                return false;

            return true;
        }

        private static string GetVehicleFileName(Vehicle vehicle)
        {
            return $"{Kendaraan.GetVehicleName(vehicle)}-{vehicle.LicensePlate}.json";
        }

        private static string GetVehicleFileName(Kendaraan vehicle)
        {
            return $"{vehicle.ModelName}-{vehicle.LicensePlate}.json";
        }

        private static string GetVehicleFileNameDelete(Kendaraan vehicle)
        {
            return $"{vehicle.ModelName}-{vehicle.LicensePlate}-[isDeleted].json";
        }
    }
}
