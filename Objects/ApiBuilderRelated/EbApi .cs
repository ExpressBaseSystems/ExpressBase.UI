﻿using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Text;
using ExpressBase.Common.Extensions;
using Newtonsoft.Json;
using ExpressBase.Common;
using ExpressBase.Common.Data;
using System.Linq;

namespace ExpressBase.Objects
{
    public class ListOrdered : List<EbApiWrapper>
    {
        public ListOrdered()
        {
            this.Sort((x, y) => x.RouteIndex.CompareTo(y.RouteIndex));
        }
    }

    public abstract class EbApiWrapper : EbObject
    {
        [EnableInBuilder(BuilderType.ApiBuilder)]
        [HideInPropertyGrid]
        public virtual int RouteIndex { set; get; }

        [EnableInBuilder(BuilderType.ApiBuilder)]
        [UIproperty]
        [MetaOnly]
        public string Label { set; get; }

        public virtual string Refid { get; set; }

        public object Result { set; get; }

        public virtual object GetOutParams(List<Param> _param) { return null; }
    }

    [EnableInBuilder(BuilderType.ApiBuilder)]
    public class EbApi : EbApiWrapper
    {
        public override int RouteIndex { set; get; }

        [EnableInBuilder(BuilderType.ApiBuilder)]
        [HideInPropertyGrid]
        public ListOrdered Resources { set; get; }
    }

    [EnableInBuilder(BuilderType.ApiBuilder)]
    public class EbSqlReader : EbApiWrapper
    {
        [EnableInBuilder(BuilderType.ApiBuilder)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [PropertyGroup("Data Settings")]
        [OSE_ObjectTypes(EbObjectTypes.iDataReader)]
        public override string Refid { get; set; }

        public override string GetDesignHtml()
        {
            return "<div class='apiPrcItem dropped' eb-type='SqlReader' id='@id'><div tabindex='1' class='drpbox' onclick='$(this).focus();'> @Label </div></div>".RemoveCR().DoubleQuoted(); ;
        }

        public override object GetOutParams(List<Param> _param)
        {
            List<Param> p = new List<Param>();
            foreach (EbDataTable table in (this.Result as EbDataSet).Tables)
            {
                string[] c = _param.Select(item => item.Name).ToArray();
                foreach (EbDataColumn cl in table.Columns)
                {
                    if (c.Contains(cl.ColumnName))
                        p.Add(new Param { Name = cl.ColumnName, Type = cl.Type.ToString(), Value = table.Rows[0][cl.ColumnIndex].ToString() });
                }
            }
            return p;
        }
    }

    [EnableInBuilder(BuilderType.ApiBuilder)]
    public class EbSqlFunc : EbApiWrapper
    {
        [EnableInBuilder(BuilderType.ApiBuilder)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [PropertyGroup("Data Settings")]
        [OSE_ObjectTypes(EbObjectTypes.iSqlFunction)]
        public override string Refid { get; set; }

        public override string GetDesignHtml()
        {
            return "<div class='apiPrcItem dropped' eb-type='SqlFunc' id='@id'><div tabindex='1' class='drpbox' onclick='$(this).focus();'> @Label </div></div>".RemoveCR().DoubleQuoted();
        }

        public override object GetOutParams(List<Param> _param)
        {
            return null;
        }
    }

    [EnableInBuilder(BuilderType.ApiBuilder)]
    public class EbSqlWriter : EbApiWrapper
    {
        [EnableInBuilder(BuilderType.ApiBuilder)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [PropertyGroup("Data Settings")]
        [OSE_ObjectTypes(EbObjectTypes.iDataWriter)]
        public override string Refid { get; set; }

        public override string GetDesignHtml()
        {
            return "<div class='apiPrcItem dropped' eb-type='SqlWriter' id='@id'><div tabindex='1' class='drpbox' onclick='$(this).focus();'> @Label </div></div>".RemoveCR().DoubleQuoted();
        }

        public override object GetOutParams(List<Param> _param)
        {
            return null;
        }
    }

    [EnableInBuilder(BuilderType.ApiBuilder)]
    public class EbEmailNode : EbApiWrapper
    {
        [EnableInBuilder(BuilderType.ApiBuilder)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [PropertyGroup("Data Settings")]
        [OSE_ObjectTypes(EbObjectTypes.iEmailBuilder)]
        public override string Refid { get; set; }

        public override string GetDesignHtml()
        {
            return "<div class='apiPrcItem dropped' eb-type='EmailNode' id='@id'><div tabindex='1' class='drpbox' onclick='$(this).focus();'> @Label </div></div>".RemoveCR().DoubleQuoted();
        }

        public override object GetOutParams(List<Param> _param)
        {
            
            return null;
        }
    }
}
