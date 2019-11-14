﻿using ExpressBase.Common;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Data;
using ExpressBase.Objects.Objects.DVRelated;
using ExpressBase.Objects.ServiceStack_Artifacts;
using Newtonsoft.Json;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
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

    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
    public class EbPowerSelect : EbControlUI
    {

        public EbPowerSelect()
        {
            EbSimpleSelect = new EbSimpleSelect();
            AddButton = new EbButton();
        }

        //public override string SetValueJSfn
        //{
        //    get
        //    {
        //        return @"
        //             this.initializer.setValues(p1, p2);
        //        ";
        //    }
        //    set { }
        //}

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        [PropertyPriority(98)]
        public bool IsInsertable { get; set; }

        public override string IsRequiredOKJSfn
        {
            get
            {
                return @"
                    if(this.RenderAsSimpleSelect){"
                        + this.EbSimpleSelect.IsRequiredOKJSfn +
                    @"}
                    else{"
                        + new EbControl().IsRequiredOKJSfn +
                    @"}
                ";
            }
            set { }
        }

        public override string GetDisplayMemberJSfn
        {
            get
            {
                return @"
                    if(this.RenderAsSimpleSelect){"
                        + this.EbSimpleSelect.GetDisplayMemberJSfn +
                    @"}
                    else{"
                        + new EbControl().IsRequiredOKJSfn +
                    @"}
                ";
            }
            set { }
        }
        public override string IsEmptyJSfn { get { return @" return this.initializer.Vobj.valueMembers.length === 0;"; } set { } }


        public override string DisableJSfn
        {
            get
            {
                return @"
                    if(this.RenderAsSimpleSelect){"
                        + this.EbSimpleSelect.DisableJSfn +
                    @"}
                    else{"
                        + new EbControl().DisableJSfn +
                    @"}
                ";
            }
            set { }
        }

        public override string EnableJSfn
        {
            get
            {
                return @"
                    if(this.RenderAsSimpleSelect){"
                        + this.EbSimpleSelect.EnableJSfn +
                    @"}
                    else{"
                        + new EbControl().EnableJSfn +
                    @"}
                ";
            }
            set { }
        }

        public override string SetValueJSfn
        {
            get
            {
                return @"
                    if(this.RenderAsSimpleSelect){"
                        + this.EbSimpleSelect.SetValueJSfn +
                    @"}
                    else{
                        this.initializer.setValues(p1, p2);
                    }
                ";
            }
            set { }
        }

        public override string GetValueJSfn
        {
            get
            {
                return @"
                    if(this.RenderAsSimpleSelect){"
                        + this.EbSimpleSelect.GetValueJSfn +
                    @"}
                    else{"
                        + new EbControl().GetValueJSfn +
                    @"}
                ";
            }
            set { }
        }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.BareControlHtml4Bot = this.BareControlHtml;
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
            if (this.RenderAsSimpleSelect)
            {
                EbSimpleSelect = new EbSimpleSelect()
                {
                    EbSid = EbSid,
                    EbSid_CtxId = EbSid_CtxId,
                    Name = Name,
                    HelpText = HelpText,
                    IsDynamic = IsDynamic,
                    ValueMember = ValueMember,
                    DisplayMember = DisplayMember,
                    DataSourceId = DataSourceId,
                    IsMultiSelect = MultiSelect
                };
            }
            else if (this.IsInsertable)
            {
                AddButton = new EbButton()
                {
                    EbSid = EbSid + "_addbtn",
                    EbSid_CtxId = EbSid_CtxId + "_addbtn",
                    Name = Name + "_addbtn",
                    FormRefId = FormRefId,
                    Label = "<i class='fa fa-plus' aria-hidden='true'></i>"
                };
            }
        }

        public EbSimpleSelect EbSimpleSelect { set; get; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public EbButton AddButton { set; get; }

        public override string SetDisplayMemberJSfn
        {
            get
            {
                return @"
        let VMs = this.initializer.Vobj.valueMembers;
        let DMs = this.initializer.Vobj.displayMembers;
        let columnVals = this.initializer.columnVals;

        if (VMs.length > 0)// clear if already values there
            this.initializer.clearValues();

        let valMsArr = p1[0].split(',');
        let DMtable = p1[1];

        for (let i = 0; i < valMsArr.length; i++) {
            let vm = valMsArr[i];
            VMs.push(vm);
            for (let j = 0; j < this.DisplayMembers.$values.length; j++) {
                let dm = this.DisplayMembers.$values[j];
                for (var k = 0; k < DMtable.length; k++) {
                    let row = DMtable[k];
                    if (getObjByval(row.Columns, 'Name', this.ValueMember.name).Value === vm) {// to select row which includes ValueMember we are seeking for 
                        let _dm = getObjByval(row.Columns, 'Name', dm.name).Value;
                        DMs[dm.name].push(_dm);
                    }
                }
            }
        }

        if (this.initializer.datatable === null) {//for aftersave actions
            $.each(valMsArr, function (i, vm) {
                $.each(DMtable, function (j, row) {
                    if (getObjByval(row.Columns, 'Name', this.ValueMember.name).Value === vm) {// to select row which includes ValueMember we are seeking for 
                        $.each(row.Columns, function (k, column) {
                            if (!columnVals[column.Name]) {
                                console.warn('Found mismatch in Columns from datasource and Colums in object');
                                return true;
                            }
                            let val = EbConvertValue(column.Value, column.Type);
                            columnVals[column.Name].push(val);
                        }.bind(this));
                    }

                    //$.each(r.Columns, function (j, column) {
                    //    if (!columnVals[column.Name]) {
                    //        console.warn('Mismatch found in Colums in datasource and Colums in object');
                    //        return true;
                    //    }
                    //    let val = EbConvertValue(column.Value, column.Type);
                    //    columnVals[column.Name].push(val);
                    //}.bind(this));

                }.bind(this));
            }.bind(this));
        }";
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
        [HideInPropertyGrid]
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

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [OSE_ObjectTypes(EbObjectTypes.iWebForm)]
        public string FormRefId { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [OSE_ObjectTypes(EbObjectTypes.iWebForm)]
        public string DataImportId { get; set; }

        [EnableInBuilder(BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.WebForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.CollectionProp, "Columns", "bVisible")]
        [PropertyGroup("Behavior")]
        //[HideInPropertyGrid]
        public DVColumnCollection Columns { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        [PropertyEditor(PropertyEditorType.CollectionABCFrmSrc, "Columns")]
        [OnChangeExec(@"
if (this.Columns && this.Columns.$values.length === 0 )
{
pg.MakeReadOnly('DisplayMembers');} else {pg.MakeReadWrite('DisplayMembers');}")]
        public DVColumnCollection DisplayMembers { get; set; }

        [EnableInBuilder(BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.WebForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns", 1)]
        [PropertyGroup("Behavior")]
        [PropertyPriority(98)]
        [OnChangeExec(@"if (this.Columns && this.Columns.$values.length === 0 ){pg.MakeReadOnly('DisplayMember');} else {pg.MakeReadWrite('DisplayMember');}")]
        public DVBaseColumn DisplayMember { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns", 1)]
        [OnChangeExec(@"if (this.Columns.$values.length === 0 ){pg.MakeReadOnly('ValueMember');} else {pg.MakeReadWrite('ValueMember');}")]
        [PropertyGroup("Behavior")]
        [PropertyPriority(99)]
        public DVBaseColumn ValueMember { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [DefaultPropValue("100")]
        [Alias("DropdownWidth(%)")]
        [Category("Appearance")]
        public int DropdownWidth { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [DefaultPropValue("100")]
        [PropertyGroup("Appearance")]
        public int DropdownHeight { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public int Value { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [HelpText("Specify minimum number of charecters to initiate search")]
        [Category("Search Settings")]
        [PropertyGroup("Behavior")]
        public int MinSeachLength { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public string Text { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
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
        [PropertyGroup("Behavior")]
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
                if(this.MaxLimit === 1)
                    pg.setSimpleProperty('MaxLimit', 0);
                    
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
                if(this.MaxLimit !== 1)
                    pg.setSimpleProperty('MaxLimit', 1);
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
        [PropertyGroup("Behavior")]
        public int MaxLimit { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        public int MinLimit { get; set; }


        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup("Search Settings")]
        [Alias("Search Method")]
        [HelpText("Select Search Method - StartsWith, EndsWith, Contains or Exact Match")]
        public DefaultSearchFor DefaultSearchFor { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        public int NumberOfFields { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.Boolean)]
        public bool IsDynamic { get; set; }

        //[EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        public int[] values { get; set; }

        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.BotForm)]
        public override bool IsReadOnly { get => this.ReadOnly; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm, BuilderType.FilterDialog)]
        [PropertyGroup("Behavior")]
        [PropertyPriority(50)]
		[OnChangeExec(@"
if(this.RenderAsSimpleSelect == true)
{ 
	pg.ShowProperty('DisplayMember');
}
else
{
	pg.HideProperty('DisplayMember');
}
")]
        public bool RenderAsSimpleSelect { get; set; }

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
        <span class='input-group-addon ps-srch' @border-r$$> <i id='@ebsid@TglBtn' class='fa  fa-search' aria-hidden='true'></i> </span>
    </div>
</div>"
.Replace("$$", obj.Name ?? "")
.Replace("@ebsid@", this.EbSid_CtxId)
.Replace("@type@", ((int)obj.Type).ToString())
.Replace("@sTitle@", obj.sTitle.ToString())
.Replace("@perWidth@", "style='width:" + ( (obj.Width == 0) ? (((int)(100 / noOfFileds)).ToString()) : obj.Width.ToString() ) + "%'")
.Replace("@border-r" + i, (i != noOfFileds - 1) ? "style='border-radius: 0px;'" : "");
                    i++;
                }
                return rs + "</div>";
            }
        }

        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolIconHtml { get { return "<i class='fa fa-search-plus'></i>"; } set { } }

        //public override string GetToolHtml()
        //{
        //    return @"<div eb-type='@toolName' class='tool'><i class='fa fa-search-plus'></i> @toolName</div>".Replace("@toolName", this.GetType().Name.Substring(2));
        //}

        public override string GetDesignHtml()
        {

            string EbCtrlHTML = HtmlConstants.CONTROL_WRAPER_HTML4WEB
                .Replace("@barehtml@", @"
         <div style='display: inline-block; width: 100%; margin-right: -4px;'>
            <div class='input-group'>
               <div class='dropdown v-select searchable' id='acmasterid0'>
                  <div type='button' class='dropdown-toggle clearfix' style='border-top-left-radius: 5px; border-bottom-left-radius: 5px;'>
                     <input debounce='0' ui-inp type='search'  readonly  placeholder='label0' class='form-control' id='acmaster1_xid' style='width: 100%; background-color: #fff;'> <i role='presentation' class='open-indicator' style='display: none;'></i> 
                     <div class='spinner' style='display: none;'>Loading...</div>
                  </div>
               </div>
               <span class='input-group-addon' style='border-radius: 0px;'><i id='acmasteridTglBtn' aria-hidden='true' class='fa  fa-search'></i></span>
            </div>").RemoveCR().DoubleQuoted();
            //return GetHtml().RemoveCR().GraveAccentQuoted();
            return ReplacePropsInHTML(EbCtrlHTML);
        }

        public override string GetHtml()
        {
            return GetHtmlHelper(RenderMode.User);
        }

        public override string DesignHtml4Bot { get => @"
	<div class='combo-wrap' data-toggle='tooltip' title='' data-original-title=''>
		<div style='display: inline-block; width: 100%; margin-right: -4px;'>
			<div class='input-group'>
				<div class='dropdown v-select searchable' id='acmasterid0'>
					<div type='button' class='dropdown-toggle clearfix' style='border-top-left-radius: 5px; border-bottom-left-radius: 5px;'>
						<input debounce='0' type='search'  readonly  placeholder='label0' class='form-control' id='acmaster1_xid' style='width: 100%; background-color: #fff;'> <i role='presentation' class='open-indicator' style='display: none;'></i> 
						<div class='spinner' style='display: none;'>Loading...</div>
					</div>
				</div>
				<span class='input-group-addon'><i id='acmasteridTglBtn' aria-hidden='true' class='fa  fa-search'></i></span>
			</div>
		</div>
	</div>"; set => base.DesignHtml4Bot = value; }

		public override string GetHtml4Bot()
		{
			return ReplacePropsInHTML((HtmlConstants.CONTROL_WRAPER_HTML4BOT).Replace("@barehtml@", DesignHtml4Bot));
		}

		public override string GetBareHtml()
        {
            if (this.RenderAsSimpleSelect)
            {
                EbSimpleSelect.ContextId = this.ContextId;
                return EbSimpleSelect.GetBareHtml();
            }

            if (this.DisplayMembers != null)
            {
                return @"
<div id='@ebsid@Container'  role='form' data-toggle='validator' style='width:100%;'>
    <input type='hidden' ui-inp name='@ebsid@Hidden4val' data-ebtype='8' id='@ebsid@'/>
    @VueSelectCode
    <center class='pow-center'>
        <div id='@ebsid@DDdiv' v-show='DDstate' class='DDdiv expand-transition'  style='width:@DDwidth%;'> 
            @addbtn@
            <table id='@ebsid@tbl' tabindex='1000' style='width:100%' class='table table-bordered'></table>
        </div>
    </center>
</div>"
    .Replace("@VueSelectCode", this.VueSelectcode)
    .Replace("@name@", this.Name)
    .Replace("@ebsid@", this.EbSid_CtxId)
    .Replace("@width", 900.ToString())//this.Width.ToString())
    .Replace("@perWidth", (this.DisplayMembers.Count != 0) ? (900 / this.DisplayMembers.Count).ToString() : 900.ToString())
    .Replace("@DDwidth", (this.DropdownWidth == 0) ? "100" : this.DropdownWidth.ToString())
    .Replace("@addbtn@", this.IsInsertable ? string.Concat("<div class='ps-addbtn-cont'>", this.AddButton.GetBareHtml(), "</div>") : string.Empty)
    .Replace("@tooltipText@", this.ToolTipText ?? string.Empty);
            }
            else
                return string.Empty;
        }

        public string GetBareHtml(string ebsid)// temp
        {
            if (this.RenderAsSimpleSelect)
            {
                EbSimpleSelect.ContextId = this.ContextId;
                return EbSimpleSelect.GetBareHtml();
            }

            if (this.DisplayMembers != null)
            {
                return @"
<div id='@ebsid@Container'  role='form' data-toggle='validator' style='width:100%;'>
    <input type='hidden' ui-inp name='@ebsid@Hidden4val' data-ebtype='8' id='@ebsid@'/>
    @VueSelectCode
    <center class='pow-center'>
        <div id='@ebsid@DDdiv' v-show='DDstate' class='DDdiv expand-transition'  style='width:@DDwidth%;'> 
            @addbtn@
            <table id='@ebsid@tbl' tabindex='1000' style='width:100%' class='table table-bordered'></table>
        </div>
    </center>
</div>"
    .Replace("@VueSelectCode", this.VueSelectcode)
    .Replace("@name@", this.Name)
    .Replace("@ebsid@", ebsid)
    .Replace("@width", 900.ToString())//this.Width.ToString())
    .Replace("@perWidth", (this.DisplayMembers.Count != 0) ? (900 / this.DisplayMembers.Count).ToString() : 900.ToString())
    .Replace("@DDwidth", (this.DropdownWidth == 0) ? "100" : this.DropdownWidth.ToString())
    .Replace("@addbtn@", this.IsInsertable ? string.Concat("<div class='ps-addbtn-cont'>", this.AddButton.GetBareHtml(), "</div>") : string.Empty)
    .Replace("@tooltipText@", this.ToolTipText ?? string.Empty);
            }
            else
                return string.Empty;
        }

        private string GetHtmlHelper(RenderMode mode)
        {
            string EbCtrlHTML = HtmlConstants.CONTROL_WRAPER_HTML4WEB
.Replace("@Label@ ", ((this.Label != null) ? this.Label : "@Label@ "))
.Replace("@tooltipText@", this.ToolTipText ?? string.Empty);

            return ReplacePropsInHTML(EbCtrlHTML);
        }

        private string GetSql(Service service)
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

            return Sql;
        }

        //INCOMPLETE// to get the entire columns(vm+dm+others) in ps query
        public string GetSelectQuery(IDatabase DataDB, Service service, string Col, string Tbl = null, string _id = null)
        {
            string Sql = this.GetSql(service);

            if (Tbl == null || _id == null)// prefill mode
            {
                string s = "";
                if (DataDB.Vendor == DatabaseVendors.MYSQL)
                {
                    s = string.Format(@"SELECT __A.* FROM ({0}) __A 
                                    WHERE FIND_IN_SET(__A.{1}, '{2}');",
                                                        Sql, this.ValueMember.Name, Col);
                }
                else
                {
                    s = string.Format(@"SELECT __A.* FROM ({0}) __A 
                                    WHERE __A.{1} = ANY(STRING_TO_ARRAY('{2}'::TEXT, ',')::INT[]);",
                                                        Sql, this.ValueMember.Name, Col);
                }
                return s;
            }
            else
            {
                // normal mode
                string s = "";
                if (DataDB.Vendor == DatabaseVendors.MYSQL)
                {
                    s = string.Format(@"SELECT __A.* FROM ({0}) __A, {1} __B
                                    WHERE FIND_IN_SET(__A.{2}, __B.{3}) AND __B.{4} = :id;",
                                         Sql, Tbl, this.ValueMember.Name, Col, _id);
                }
                else
                {
                    s = string.Format(@"SELECT __A.* FROM ({0}) __A, {1} __B
                                    WHERE __A.{2} = ANY(STRING_TO_ARRAY(__B.{3}::TEXT, ',')::INT[]) AND __B.{4} = :id;",
                                        Sql, Tbl, this.ValueMember.Name, Col, _id);
                }
                return s;
            }
        }

        //to get vm+dm only for audit trail
        public string GetDisplayMembersQuery(IDatabase DataDB, Service service, string vms)
        {
            string Sql = this.GetSql(service);
            string vm = this.ValueMember.Name;
            string dm = string.Join(',', this.DisplayMembers.Select(e => e.Name));

            string s = "";
            if (DataDB.Vendor == DatabaseVendors.MYSQL)
            {
                s = string.Format(@"SELECT {0}, {1} FROM ({2}) __A
                                        WHERE FIND_IN_SET(__A.{0}, '{3}');",
                            vm, dm, Sql, vms);
            }
            else
            {
                s = string.Format(@"SELECT {0}, {1} FROM ({2}) __A
                                        WHERE __A.{0} = ANY(STRING_TO_ARRAY('{3}'::TEXT, ',')::INT[]);",
                                            vm, dm, Sql, vms);
            }

            return s;
        }
    }
}