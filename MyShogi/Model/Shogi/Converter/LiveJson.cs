using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace MyShogi.Model.Shogi.Converter
{
    public static class LiveJsonUtil
    {
        public static LiveJson FromString(string str)
        {
            var serializer = new DataContractJsonSerializer(typeof(LiveJson));
            MemoryStream st = new MemoryStream(Encoding.UTF8.GetBytes(str));
            return (LiveJson)serializer.ReadObject(st);
        }
        public static LiveJson FromUri(string uri, int timeout = 3000)
        {
            var serializer = new DataContractJsonSerializer(typeof(LiveJson));
            WebRequest req = WebRequest.Create(uri);
            req.Timeout = timeout;
            WebResponse res = req.GetResponse();
            Stream st = res.GetResponseStream();
            return (LiveJson)serializer.ReadObject(st);
        }
        public static string ToJson(this LiveJson data)
        {
            var serializer = new DataContractJsonSerializer(typeof(LiveJson));
            MemoryStream st = new MemoryStream();
            serializer.WriteObject(st, data);
            return Encoding.UTF8.GetString(st.ToArray());
        }
        public static string ToXml(this LiveJson data)
        {
            var serializer = new DataContractSerializer(typeof(LiveJson));
            MemoryStream st = new MemoryStream();
            serializer.WriteObject(st, data);
            return Encoding.UTF8.GetString(st.ToArray());
        }
    }

    /// <summary>
    /// JSON中継棋譜形式
    /// </summary>
    [DataContract] public class LiveJson
    {
        [DataMember] public List<Data> data { get; set; }
        [DataMember] public long cacheUpdateTime { get; set; }

        [DataContract] public class Data
        {
            [DataMember(Name = "_id")] public string idHex { get; set; }
            [DataMember] public string gametype { get; set; }
            [DataMember] public string key { get; set; }
            [DataMember] public string fname { get; set; }
            [DataMember(Name = "event")] public string eventName { get; set; }
            [DataMember] public string player1 { get; set; }
            [DataMember] public string player2 { get; set; }
            [DataMember] public string side { get; set; }
            [DataMember] public string place { get; set; }
            [DataMember] public string handicap { get; set; }
            [DataMember] public string judgeside { get; set; }
            [DataMember] public string note { get; set; }
            [DataMember] public string recordman { get; set; }
            /// <summary>
            /// realstarttime: Unixtime (unit: milliseconds)
            /// </summary>
            [DataMember] public long? realstarttime { get; set; }
            /// <summary>
            /// starttime: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string starttime { get; set; }
            /// <summary>
            /// endtime: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string endtime { get; set; }
            /// <summary>
            /// delatetime_p1: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string delatetime_p1 { get; set; }
            /// <summary>
            /// delatetime_p2: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string delatetime_p2 { get; set; }
            /// <summary>
            /// stoptime_start: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string stoptime_start { get; set; }
            /// <summary>
            /// stoptime_end: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string stoptime_end { get; set; }
            /// <summary>
            /// lunchtime_end: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string lunchtime_end { get; set; }
            /// <summary>
            /// lunchtime_start: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string lunchtime_start { get; set; }
            /// <summary>
            /// dinnertime_start: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string dinnertime_start { get; set; }
            /// <summary>
            /// dinnertime_end: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string dinnertime_end { get; set; }
            /// <summary>
            /// lunchtime_end_2: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string lunchtime_end_2 { get; set; }
            /// <summary>
            /// lunchtime_start_2: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string lunchtime_start_2 { get; set; }
            /// <summary>
            /// dinnertime_start_2: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string dinnertime_start_2 { get; set; }
            /// <summary>
            /// dinnertime_end_2: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string dinnertime_end_2 { get; set; }
            /// <summary>
            /// enddate: ISO8601(no timezone) or emptystring
            /// </summary>
            [DataMember] public string enddate { get; set; }
            [DataMember] public string timelimit { get; set; }
            [DataMember] public string countdown { get; set; }
            [DataMember] public string spendtime_p1 { get; set; }
            [DataMember] public string spendtime_p2 { get; set; }
            [DataMember] public string delaytimes_p1 { get; set; }
            [DataMember] public string delaytimes_p2 { get; set; }
            [DataMember] public string end_mark { get; set; }
            [DataMember] public string end_reason { get; set; }
            [DataMember] public string end_side { get; set; }
            [DataMember] public long? end_tesu { get; set; }
            [DataMember] public List<BreakTime> breaktime { get; set; }
            /// <summary>
            /// kif length
            /// </summary>
            [DataMember(Name = "__v")] public long v { get; set; }
            [DataMember] public List<Kif> kif { get; set; }
        }
        [DataContract] public class BreakTime
        {
            [DataMember(Name = "_id")] public string idHex { get; set; }
            [DataMember] public string reason { get; set; }
            /// <summary>
            /// start: Unixtime (unit: milliseconds)
            /// </summary>
            [DataMember] public long? start { get; set; }
            /// <summary>
            /// end: Unixtime (unit: milliseconds)
            /// </summary>
            [DataMember] public long? end { get; set; }
        }
        [DataContract] public class Kif
        {
            [DataMember(Name = "_id")] public string idHex { get; set; }
            [DataMember] public long? num { get; set; }
            [DataMember] public string move { get; set; }
            /// <summary>
            /// time: Unixtime (unit: milliseconds)
            /// </summary>
            [DataMember] public long? time { get; set; }
            [DataMember] public int? frX { get; set; }
            [DataMember] public int? frY { get; set; }
            [DataMember] public int? toX { get; set; }
            [DataMember] public int? toY { get; set; }
            [DataMember] public int? prmt { get; set; }
            /// <summary>
            /// type: 駒の種類
            /// FU:    "歩",
            /// KYO:   "香",
            /// KEI:   "桂",
            /// GIN:   "銀",
            /// KIN:   "金",
            /// KAKU:  "角",
            /// HI:    "飛",
            /// OU:    "王",
            /// NFU:   "と",
            /// NKYO:  "成香",
            /// NKEI:  "成桂",
            /// NGIN:  "成銀",
            /// NKAKU: "馬",
            /// NHI:   "竜",
            /// </summary>
            [DataMember] public string type { get; set; }
            [DataMember] public long? spend { get; set; }
        }
    }
}
