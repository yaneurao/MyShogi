using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace MyShogi.Model.Shogi.Converter
{

    public static class JkfUtil
    {
        public static Jkf FromString(string str)
        {
            var serializer = new DataContractJsonSerializer(typeof(Jkf));
            MemoryStream st = new MemoryStream(Encoding.UTF8.GetBytes(str));
            return (Jkf)serializer.ReadObject(st);
        }
        public static Jkf FromUri(string uri)
        {
            var serializer = new DataContractJsonSerializer(typeof(Jkf));
            WebRequest req = WebRequest.Create(uri);
            WebResponse res = req.GetResponse();
            Stream st = res.GetResponseStream();
            return (Jkf)serializer.ReadObject(st);
        }
        public static string ToJson(this Jkf data)
        {
            var serializer = new DataContractJsonSerializer(typeof(Jkf));
            MemoryStream st = new MemoryStream();
            serializer.WriteObject(st, data);
            return Encoding.UTF8.GetString(st.ToArray());
        }
        public static string ToXml(this Jkf data)
        {
            var serializer = new DataContractSerializer(typeof(Jkf));
            MemoryStream st = new MemoryStream();
            serializer.WriteObject(st, data);
            return Encoding.UTF8.GetString(st.ToArray());
        }
    }

    /// <summary>
    /// JSON Kifu Format
    /// https://github.com/na2hiro/json-kifu-format で提案されている棋譜フォーマット
    /// </summary>
    [DataContract] public class Jkf
    {
        [DataMember] public Dictionary<string, string> header;
        [DataMember] public Initial initial;
        [DataMember] public List<MoveFormat> moves;

        [DataContract] public class Initial
        {
            [DataMember] public string preset;
            [DataMember] public Data data;
        }
        [DataContract] public class Data
        {
            [DataMember] public int color;
            [DataMember] public Board[,] board;
            [DataMember] public List<Dictionary<string, int>> hands;
        }
        [DataContract] public class Board
        {
            [DataMember] public int? color;
            [DataMember] public string kind;
        }
        [DataContract] public class MoveFormat
        {
            [DataMember] public List<string> comments;
            [DataMember] public Move move;
            [DataMember] public Time time;
            [DataMember] public string special;
            [DataMember] public List<List<MoveFormat>> forks;
        }
        [DataContract] public class Move
        {
            [DataMember] public int color;
            [DataMember] public PlaceFormat from;
            [DataMember] public PlaceFormat to;
            [DataMember] public string piece;
            [DataMember] public bool? same;
            [DataMember] public bool? promote;
            [DataMember] public string capture;
            [DataMember] public string relative;
        }
        [DataContract] public class Time
        {
            [DataMember] public TimeFormat now;
            [DataMember] public TimeFormat total;
        }
        [DataContract] public class TimeFormat
        {
            [DataMember] public int? h;
            [DataMember] public int m;
            [DataMember] public int s;
        }
        [DataContract] public class PlaceFormat
        {
            [DataMember] public int x;
            [DataMember] public int y;
        }
    }
}
