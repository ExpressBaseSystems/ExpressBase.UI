﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    [ProtoBuf.ProtoContract]
    public enum EbObjectType
    {
        WebForm = 0,
        DisplayBlock = 1,
        DataSource = 2,
        Report = 3,
        Table = 4,
        SqlFunction = 5,
        SqlValidator = 6,
        JavascriptFunction = 7,
        JavascriptValidator = 8,
        Application = 9,
        ApplicationModule = 10,
        DataVisualization = 11,
        FilterDialog = 12
    }

    [ProtoBuf.ProtoContract]
    public enum EbObjectTypesUI
    {
        Form = 0,
        Report = 3,
        Table = 4,
        Application = 9,
        ApplicationModule = 10,
        DataVisualization = 11,
        FilterDialog = 12
    }

    [ProtoBuf.ProtoContract]
    public enum EbObjectTypesNonVer
    {
        Application = 9,
        ApplicationModule = 10,
        DataVisualization = 11
    }

    [ProtoBuf.ProtoContract]
    public enum EbDataGridViewColumnType
    {
        Boolean,
        DateTime,
        Image,
        Numeric,
        Text,
        Null,
        Chart
    }
    [ProtoBuf.ProtoContract]
    public enum ObjectLifeCycleStatus
    {
        Development,
        Test,
        UAT,
        Live,
        Offline,
        Obsolete
    }
}
