using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace DZDraven
{
    class Reticle
    {
        private GameObject obj;
        private double CreationTime;
        private double EndTime;
        private int NetworkId;
        private Vector3 posi;
        public Reticle(GameObject retObject,double CreatT,Vector3 position,double EndT,int NId)
        {
            this.obj = retObject;
            this.CreationTime = CreatT;
            this.EndTime = EndT;
            this.NetworkId = NId;
            this.posi = position;
        }
        public GameObject getObj()
        {
            return this.obj;
        }
        public Vector3 getPosition()
        {
            return this.posi;
        }
        public double getCreationTime()
        {
            return this.CreationTime;
        }
        public double getEndTime()
        {
            return this.EndTime;
        }
        public int getNetworkId()
        {
            return this.NetworkId;
        }

    }
}
