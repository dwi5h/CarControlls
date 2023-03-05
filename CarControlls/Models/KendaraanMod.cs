using Rage;

namespace CarControlls.Models
{
    class KendaraanMod
    {
        public int Type { get; set; }

        /// <summary>
        /// Friendly name of the vehicle mod. Helps to make vehicle save files more readable.
        /// </summary>
        public string FriendlyName { get; set; }
        public int Index { get; set; }

        public KendaraanMod(int type, string name, int index)
        {
            Type = type;
            FriendlyName = name;
            Index = index;
        }
    }
}
