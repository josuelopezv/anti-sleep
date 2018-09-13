using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace API
{
    [DataContract]
    public class Settings
    {
        [NonSerialized]
        static string FileName = "Settings.json";

        static DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Settings));

        [DataMember]
        public bool asEnabled { get; set; }

        [DataMember]
        public IEnumerable<HotKey> HotKeys { get; set; }

        public static Settings Load()
        {
            if (!File.Exists(FileName)) return new Settings();
            Stream stream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var obj = (Settings)ser.ReadObject(stream);
            stream.Close();
            return obj;
        }

        public void Save()
        {
            Stream stream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None);
            ser.WriteObject(stream, this);
            stream.Close();
        }
    }
}
