using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SourceAFIS.Simple;
namespace TwoFactorAuth.App_Code
{
    public class globalClass
    {
        static AfisEngine Afis = new AfisEngine();
        public static AfisEngine afisGlobal
        {
            get
            {
                return Afis;
            }
        }
    }

    [Serializable]
    public class MyFingerprint : Fingerprint
    {
        public string Filename;
    }

    [Serializable]
    public class MyPerson : Person
    {
        public string Name;
        public int Nic;
        public int Minutae;


    }
}