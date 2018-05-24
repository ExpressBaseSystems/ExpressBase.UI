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

    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.BotForm)]
    public class EbSimpleSelect : EbControlUI
    {

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        [OSE_ObjectTypes(EbObjectTypes.iDataSource)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        public string DataSourceId { get; set; }

        [EnableInBuilder(BuilderType.FilterDialog, BuilderType.BotForm)]
        [HideInPropertyGrid]
        public ColumnColletion Columns { get; set; }

        [EnableInBuilder(BuilderType.FilterDialog, BuilderType.BotForm)]
        [PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns", 1)]
		[OnChangeExec(@"console.log(100); if (this.Columns.$values.length === 0 ){pg.MakeReadOnly('ValueMember');} else {pg.MakeReadWrite('ValueMember');}")]
		public EbDataColumn ValueMember { get; set; }

        [EnableInBuilder(BuilderType.FilterDialog, BuilderType.BotForm)]
        [PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns", 1)]
		[OnChangeExec(@"console.log(100); if (this.Columns.$values.length === 0 ){pg.MakeReadOnly('DisplayMember');} else {pg.MakeReadWrite('DisplayMember');}")]
		public EbDataColumn DisplayMember { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        public int Value { get; set; }

		[HideInPropertyGrid]
		[EnableInBuilder(BuilderType.BotForm)]
		public override bool IsReadOnly { get => this.ReadOnly; }

		public EbSimpleSelect() { }

        public string OptionHtml { get; set; }

        public override string GetToolHtml()
        {
            return @"<div eb-type='@toolName' class='tool'><i class='fa fa-align-justify'></i>  @toolName</div>".Replace("@toolName", this.GetType().Name.Substring(2));
        }



        public void InitFromDataBase(JsonServiceClient ServiceClient)
		{

			//this.DataSourceId = "eb_roby_dev-eb_roby_dev-2-1015-1739";
			var result = ServiceClient.Get<DataSourceDataResponse>(new DataSourceDataRequest { RefId = this.DataSourceId });
			string _html = string.Empty;

			foreach (EbDataRow option in result.Data)
			{
				_html += string.Format("<option value='{0}'>{1}</option>", option[this.ValueMember.ColumnIndex], option[this.DisplayMember.ColumnIndex]);
				//_html += string.Format("<option value='{0}'>{1}</option>", option[0].ToString().Trim(), option[0]);
            }

			this.OptionHtml = _html;
		}


		[OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
        }

        public override string GetHead()
        {
            return this.RequiredString + @"
<script>

</script>
";
        }

        public override string GetDesignHtml()
        {
            return @"

    <div id='cont_@name@' Ctype='SimpleSelect' class='Eb-ctrlContainer' style='@hiddenString'>
        <div id='@name@' class='btn-group bootstrap-select show-tick' style='width: 100%;'><button type='button' class='btn dropdown-toggle btn-default'><span class='filter-option pull-left'>Simple Select</span>&nbsp;<span class='bs-caret'><span class='caret'></span></span></button></div>
    </div>".RemoveCR().DoubleQuoted();//GetHtmlHelper(RenderMode.Developer).RemoveCR().DoubleQuoted();
        }

        public override string GetHtml()
        {
            return GetHtmlHelper(RenderMode.User);
        }

        public override string GetBareHtml()
        {
            return @"
        <select id='@name@' name='@name@' data-ebtype='@data-ebtype@'>
            @options@
        </select>"
.Replace("@name@", this.Name)
.Replace("@options@", this.OptionHtml)
.Replace("@data-ebtype@", "16");
        }

        private string GetHtmlHelper(RenderMode mode)
        {
            return @"
<div id='cont_@name@  ' class='Eb-ctrlContainer' Ctype='TextBox' style='@HiddenString '>
    <div class='eb-ctrl-label' id='@name@Lbl' style='@LabelBackColor@ @LabelForeColor@ '> @Label@  </div>
       @barehtml@
    <span class='helpText'> @HelpText@ </span>
</div>"
.Replace("@barehtml@", this.GetBareHtml())
.Replace("@HelpText@", this.HelpText)
.Replace("@Label@", this.Label)
.Replace("@name@", this.Name)
.Replace("@HiddenString ", this.HiddenString)
.Replace("@ToolTipText ", this.ToolTipText);

        }
    }
}
