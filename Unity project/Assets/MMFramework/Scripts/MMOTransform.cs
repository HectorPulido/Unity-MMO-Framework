using UnityEngine;

namespace MMO.Models
{
    public struct MMOTransform
    {
        public float px,py,pz;
        public float rx,ry,rz,rw;

        public static MMOTransform FromTransform(Transform t)
        {
            var r = new MMOTransform();
            r.px = t.position.x;
            r.py = t.position.y;
            r.pz = t.position.z;
            r.rx = t.rotation.x;
            r.ry = t.rotation.y;
            r.rz = t.rotation.z;
            r.rw = t.rotation.w;
            return r;
        }

        public Vector3 GetVector3()
        {
            return new Vector3(px,py,pz);
        }
        public Quaternion GetQuaternion()
        {
            return new Quaternion(rx, ry, rz, rw);
        }
    }
}