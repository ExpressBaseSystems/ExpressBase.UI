﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    [ProtoBuf.ProtoContract]
    public class EbDataSource : EbObject
    {
        [ProtoBuf.ProtoMember(1)]
        public string Sql { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class EbSqlFunction : EbObject
    {
        [ProtoBuf.ProtoMember(1)]
        public string Sql { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class EbJsFunction : EbObject
    {
        [ProtoBuf.ProtoMember(1)]
        public string JsCode { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class EbJsValidator : EbObject
    {
        [ProtoBuf.ProtoMember(1)]
        public string JsCode { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class EbDataVisualization : EbObject
    {
        [ProtoBuf.ProtoMember(1)]
        public string settingsJson { get; set; }
    }

}
