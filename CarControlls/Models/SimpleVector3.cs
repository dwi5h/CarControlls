using Rage;

namespace CarControlls.Models
{
    public class SimpleVector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public SimpleVector3()
        {
        }

        public SimpleVector3(Vector3 vector3)
        {
            X = vector3.X;
            Y = vector3.Y;
            Z = vector3.Z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
    }
}
