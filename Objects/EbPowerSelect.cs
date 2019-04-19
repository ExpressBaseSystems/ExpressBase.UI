﻿using ExpressBase.Common;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Data;
using ExpressBase.Objects.Objects.DVRelated;
using ExpressBase.Objects.ServiceStack_Artifacts;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    public enum DefaultSearchFor
    {
        BeginingWithKeyword,
        EndingWithKeyword,
        ExactMatch,
        Contains,
    }

    [ProtoBuf.ProtoContract]
    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
    public class EbPowerSelect : EbControlUI
    {

        public EbPowerSelect() { }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
        }

        public override string SetValueJSfn
        {
            get
            {
                return @"
                     this.initializer.setValues(p1);
                ";
            }
            set { }
        }

        public override string SetDisplayMemberJSfn
        {
            get
            {
                return @"
console.log(1000);
        let VMs = this.initializer.Vobj.valueMembers;
        let DMs = this.initializer.Vobj.displayMembers;
        let columnvals = this.initializer.columnvals;

        if (VMs.length > 0)// clear if already values there
            this.initializer.clearValues();

        let valMsArr = p1[0].split(',');
        let DMtable = p1[1];


        $.each(valMsArr, function (i, vm) {
            VMs.push(vm);
            $.each(this.DisplayMembers.$values, function (j, dm) {
                $.each(DMtable, function (j, r) {
                    if (getObjByval(r.Columns, 'Name', this.ValueMember.name).Value === vm) {
                        let _dm = getObjByval(r.Columns, 'Name', dm.name).Value;
                        DMs[dm.name].push(_dm);
                    }
                }.bind(this));
            }.bind(this));
        }.bind(this));

        $.each(DMtable, function (j, r) {
            $.each(r.Columns, function (j, item) {
                if (!columnvals[item.Name]) {
                    console.warn('Mismatch found in Colums in datasource and Colums in object');
                    return true;
                }
                columnvals[item.Name].push(item.Value);
            }.bind(this));
        }.bind(this));

                ";
            }
            set { }
        }

        public override string ClearJSfn
        {
            get
            {
                return @"
                     this.initializer.clearValues();
                ";
            }
            set { }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public override EbDbTypes EbDbType
        {
            get
            {
                return (this.MultiSelect) ? EbDbTypes.String : EbDbTypes.Decimal;
            }
            set { }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [OSE_ObjectTypes(EbObjectTypes.iDataReader)]
        public string DataSourceId { get; set; }

        [EnableInBuilder(BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.WebForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.CollectionProp, "Columns", "bVisible")]
        public DVColumnCollection Columns { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns")]
        public DVColumnCollection DisplayMembers { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns", 1)]
        [OnChangeExec(@"if (this.Columns.$values.length === 0 ){pg.MakeReadOnly('ValueMember');} else {pg.MakeReadWrite('ValueMember');}")]
        public DVBaseColumn ValueMember { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [DefaultPropValue("100")]
        [Alias("DropdownWidth(%)")]
        public int DropdownWidth { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [DefaultPropValue("100")]
        public int DropdownHeight { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public int Value { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [HelpText("Specify minimum number of charecters to initiate search")]
        public int MinSeachLength { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public string Text { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [OnChangeExec(@"
            if (this.MultiSelect === true ){
                pg.MakeReadWrite('MaxLimit');   
                if (this.Required === true ){
                    if(this.MinLimit < 1){
                        pg.setSimpleProperty('MinLimit', 1);
                    }
                    pg.MakeReadWrite('MinLimit');
                }
                else{
                    pg.setSimpleProperty('MinLimit', 0);
                    pg.MakeReadOnly('MinLimit');                 
                }
            } 
            else {
                pg.setSimpleProperty('MaxLimit', 1);
                pg.MakeReadOnly(['MaxLimit','MinLimit']);
                if (this.Required === true ){
                    pg.setSimpleProperty('MinLimit', 1);
                }
                else{
                    pg.setSimpleProperty('MinLimit', 0);
                }
            }")]
        public override bool Required { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [Category("Behavior")]
        [OnChangeExec(@"
            if (this.MultiSelect === true ){
                pg.MakeReadWrite('MaxLimit');   
                if (this.Required === true ){
                    if(this.MinLimit < 1){
                        pg.setSimpleProperty('MinLimit', 1);
                    }
                    pg.MakeReadWrite('MinLimit');
                }
                else{
                    pg.setSimpleProperty('MinLimit', 0);
                    pg.MakeReadOnly('MinLimit');                 
                }
            } 
            else {
                pg.setSimpleProperty('MaxLimit', 1);
                pg.MakeReadOnly(['MaxLimit','MinLimit']);
                if (this.Required === true ){
                    pg.setSimpleProperty('MinLimit', 1);
                }
                else{
                    pg.setSimpleProperty('MinLimit', 0);
                }
            }")]

        public bool MultiSelect
        {
            get
            {
                return this.MaxLimit != 1;
            }
            set { }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [DefaultPropValue("1")]
        public int MaxLimit { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public int MinLimit { get; set; }


        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public DefaultSearchFor DefaultSearchFor { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public int NumberOfFields { get; set; }

        //[EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        public int[] values { get; set; }

        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.BotForm)]
        public override bool IsReadOnly { get => this.ReadOnly; }

        private string VueSelectcode
        {
            get
            {
                int noOfFileds = this.DisplayMembers.Count;
                int i = 0;
                string rs = "<div id='@ebsid@Wraper' class='search-wraper' data-toggle='tooltip' title='@tooltipText@'>";
                foreach (DVBaseColumn obj in this.DisplayMembers)
                {
                    rs += @"
<div class='search-block' @perWidth@>
    <div class='input-group'>
        <v-select maped-column='$$' column-type='@type@' id='@ebsid@$$' style='width:{3}px;' 
            multiple
            v-model='displayMembers[`$$`]'
            :on-change='updateCk'
            placeholder = '@sTitle@'>
        </v-select>
        <span class='input-group-addon' @border-r$$> <i id='@ebsid@TglBtn' class='fa  fa-search' aria-hidden='true'></i> </span>
    </div>
</div>"
.Replace("$$", obj.Name ?? "")
.Replace("@ebsid@", this.EbSid_CtxId)
.Replace("@type@", ((int)obj.Type).ToString())
.Replace("@sTitle@", obj.sTitle.ToString())
.Replace("@perWidth@", "style='width:" + ((int)(100 / noOfFileds)).ToString() + "%'")
.Replace("@border-r" + i, (i != noOfFileds - 1) ? "style='border-radius: 0px;'" : "");
                    i++;
                }
                return rs + "</div>";
            }
        }

        public override string GetToolHtml()
        {
            return @"<div eb-type='@toolName' class='tool'> &#9869; PowerSelect</div>".Replace("@toolName", this.GetType().Name.Substring(2));
        }

        public override string GetDesignHtml()
        {
            return @"
<div id='cont_@name@  ' class='Eb-ctrlContainer' Ctype='TextBox' eb-hidden='@isHidden@'>
   <div role='form' data-toggle='validator' style=' width: inherit;'>
    <div class='eb-ctrl-label' style='background-color:@LabelBackColor@; color:@LabelForeColor@ '> @Label@  </div>
      <div class='combo-wrap' data-toggle='tooltip' title='' data-original-title=''>
         <div style='display: inline-block; width: 100%; margin-right: -4px;'>
            <div class='input-group'>
               <div class='dropdown v-select searchable' id='acmasterid0'>
                  <div type='button' class='dropdown-toggle clearfix' style='border-top-left-radius: 5px; border-bottom-left-radius: 5px;'>
                     <input debounce='0' type='search'  readonly  placeholder='label0' class='form-control' id='acmaster1_xid' style='width: 100%; background-color: #fff;'> <i role='presentation' class='open-indicator' style='display: none;'></i> 
                     <div class='spinner' style='display: none;'>Loading...</div>
                  </div>
               </div>
               <span class='input-group-addon' style='border-radius: 0px;'><i id='acmasteridTglBtn' aria-hidden='true' class='fa  fa-search'></i></span>
            </div>
         </div>
      </div>
   </div>
</div>"
    .Replace("@name@", this.Name)
    .RemoveCR().DoubleQuoted();//GetHtmlHelper(RenderMode.Developer).RemoveCR().DoubleQuoted();
        }

        public override string GetHtml()
        {
            return GetHtmlHelper(RenderMode.User);
        }

        public override string DesignHtml4Bot { get => @"
<div id='cont_@name@  ' class='Eb-ctrlContainer' Ctype='TextBox' eb-hidden='@isHidden@'>
   <div role='form' data-toggle='validator' style=' width: inherit;'>
    <span style='background-color:@LabelBackColor@; color:@LabelForeColor@ '> @Label@  </span>
      <div class='combo-wrap' data-toggle='tooltip' title='' data-original-title=''>
         <div style='display: inline-block; width: 100%; margin-right: -4px;'>
            <div class='input-group'>
               <div class='dropdown v-select searchable' id='acmasterid0'>
                  <div type='button' class='dropdown-toggle clearfix' style='border-top-left-radius: 5px; border-bottom-left-radius: 5px;'>
                     <input debounce='0' type='search'  readonly  placeholder='label0' class='form-control' id='acmaster1_xid' style='width: 100%; background-color: #fff;'> <i role='presentation' class='open-indicator' style='display: none;'></i> 
                     <div class='spinner' style='display: none;'>Loading...</div>
                  </div>
               </div>
               <span class='input-group-addon' style='border-radius: 0px;'><i id='acmasteridTglBtn' aria-hidden='true' class='fa  fa-search'></i></span>
            </div>
         </div>
      </div>
   </div>
</div>"; set => base.DesignHtml4Bot = value; }

        public override string GetBareHtml()
        {
            if (this.DisplayMembers != null)
            {
                return @"
<div id='@ebsid@Container'  role='form' data-toggle='validator' style='width:100%;'>
    <input type='hidden' ui-inp name='@ebsid@Hidden4val' data-ebtype='8' id='@ebsid@'/>
    @VueSelectCode
    <center class='pow-center'>
        <div id='@ebsid@DDdiv' v-show='DDstate' class='DDdiv expand-transition'  style='width:@DDwidth%;'> 
            <table id='@ebsid@tbl' tabindex='1000' style='width:100%' class='table table-bordered'></table>
        </div>
    </center>
</div>"
    .Replace("@VueSelectCode", this.VueSelectcode)
    .Replace("@name@", this.Name)
    //.Replace("@ebsid@", this.EbSid_CtxId)
    .Replace("@width", 900.ToString())//this.Width.ToString())
    .Replace("@perWidth", (this.DisplayMembers.Count != 0) ? (900 / this.DisplayMembers.Count).ToString() : 900.ToString())
    .Replace("@DDwidth", (this.DropdownWidth == 0) ? "100" : this.DropdownWidth.ToString())
    .Replace("@tooltipText@", this.ToolTipText ?? string.Empty);
            }
            else
                return string.Empty;
        }

        private string GetHtmlHelper(RenderMode mode)
        {
            string EbCtrlHTML = Helpers.HtmlConstants.CONTROL_WRAPER_HTML4WEB
.Replace("@Label@ ", ((this.Label != null) ? this.Label : "@Label@ "))
.Replace("@tooltipText@", this.ToolTipText ?? string.Empty);

            return ReplacePropsInHTML(EbCtrlHTML);
        }


        //INCOMPLETE
        public string GetSelectQuery(Service service, string Col, string Tbl = null, string _id = null)
        {
            EbDataReader dr = service.Redis.Get<EbDataReader>(this.DataSourceId);
            if (dr == null)
            {
                var result = service.Gateway.Send<EbObjectParticularVersionResponse>(new EbObjectParticularVersionRequest { RefId = this.DataSourceId });
                dr = EbSerializers.Json_Deserialize(result.Data[0].Json);
                service.Redis.Set<EbDataReader>(this.DataSourceId, dr);
            }

            string Sql = dr.Sql.Trim();
            if (Sql.LastIndexOf(";") == Sql.Length - 1)
                Sql = Sql.Substring(0, Sql.Length - 1);

            if (Tbl == null || _id == null)
                return string.Format(@"SELECT __A.* FROM ({0}) __A 
                                    WHERE __A.{1} = ANY(STRING_TO_ARRAY({2}::TEXT, ',')::INT[]);",
                                    Sql, this.ValueMember.Name, Col);
            else
                return string.Format(@"SELECT __A.* FROM ({0}) __A, {1} __B
                                    WHERE __A.{2} = ANY(STRING_TO_ARRAY(__B.{3}::TEXT, ',')::INT[]) AND __B.{4} = :id;",
                                        Sql, Tbl, this.ValueMember.Name, Col, _id);
        }
    }
}