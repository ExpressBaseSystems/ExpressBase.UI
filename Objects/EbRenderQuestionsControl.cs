﻿using ExpressBase.Common;
using ExpressBase.Common.Constants;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ExpressBase.Objects
{
    [EnableInBuilder(BuilderType.WebForm, BuilderType.UserControl, BuilderType.SurveyControl)]
    [SurveyBuilderRoles(SurveyRoles.AnswerControl)]
    public class EbRenderQuestionsControl : EbControlUI
    {
        public EbRenderQuestionsControl()
        {

        }


        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
            if (this.ValueExpr == null)
                this.ValueExpr = new EbScript();
        }

        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.UserControl)]
        public override bool DoNotPersist { get; set; }


        public override string GetDesignHtml()
        {
            return GetHtml().RemoveCR().GraveAccentQuoted();
        }

        public override string GetBareHtml()
        {
            return @" 
        <div class='qrende-wrap'>
        </div>"
    .Replace("@name@", this.Name)
    .Replace("@ebsid@", this.EbSid);
        }


        public override string GetHtml()
        {
            string EbCtrlHTML = HtmlConstants.CONTROL_WRAPER_HTML4WEB

    .Replace("@LabelForeColor ", "color:" + ((this.LabelForeColor != null) ? this.LabelForeColor : "@LabelForeColor ") + ";")
    .Replace("@LabelBackColor ", "background-color:" + ((this.LabelBackColor != null) ? this.LabelBackColor : "@LabelBackColor ") + ";");

            return ReplacePropsInHTML(EbCtrlHTML);
        }


        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolIconHtml { get { return "<i class='fa fa-list'></i>"; } set { } }



        [HideInPropertyGrid]
        [JsonIgnore]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.UserControl)]
        public override string ToolNameAlias { get { return "Render Questions"; } set { } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [PropertyGroup(PGConstants.DATA)]
        [OSE_ObjectTypes(EbObjectTypes.iWebForm)]
        [Alias("Source Form")]
        public string FormRefId { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup(PGConstants.DATA)]
        public string SourceControlName { get; set; }


        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.ScriptEditorJS, PropertyEditorType.ScriptEditorSQ)]
        [Alias("Data Id Expression")]
        [PropertyGroup(PGConstants.VALUE)]
        [HelpText("Define how Data Id of this field should be calculated.")]
        public override EbScript ValueExpr { get; set; }

    }
}
