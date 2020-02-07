﻿using ExpressBase.Common;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using Newtonsoft.Json;
using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ExpressBase.Objects
{
	

	[EnableInBuilder(BuilderType.WebForm, BuilderType.UserControl)]
	public class EbBluePrint : EbControlUI, IEbSpecialContainer
	{
		public EbBluePrint()
		{
			
		}




		[OnDeserialized]
		public void OnDeserializedMethod(StreamingContext context)
		{
			this.BareControlHtml = this.GetBareHtml();
			this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
		}

		public override string ToolIconHtml { get { return "<i class='fa fa-building'></i>"; } set { } }

		public override string ToolNameAlias { get { return "Blueprint"; } set { } }

		public override string ToolHelpText { get { return "Blueprint"; } set { } }

		public override string UIchangeFns
		{
			get
			{
				return @"EbBluePrint = {
                
            }";
			}
		}



		//--------Hide in property grid------------
		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override string HelpText { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override string ToolTipText { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override bool Unique { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override List<EbValidator> Validators { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override EbScript DefaultValueExpression { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override EbScript VisibleExpr { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override EbScript ValueExpr { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override bool IsDisable { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override bool Required { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override bool DoNotPersist { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]


		[HideInPropertyGrid]
		public override string BackColor { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override string ForeColor { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override string LabelBackColor { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override string LabelForeColor { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override EbScript OnChangeFn { get; set; }

		public override string GetBareHtml()
		{


			return @" 
 <div id='@ebsid@' > 
        <div class='ebimg-cont' style='width:100%; text-align:center;'>
            <img id='@name@' class='dpctrl-img' src='@src@'  style='opacity:0.5; max-width:100%; max-height:@maxheight@px;' alt='@alt@'>
        </div>
        <div class='edit_bp_btn-cont' style='width: 100%;position: absolute;bottom: 0;background: transparent; ;'>
                <div id='edit_buleprintbtn' class='edit_bp@ebsid@' style='height: 20px;width: 70px;display: inline-block;text-align: center;border: 1px solid #ccc;border-radius: 20px;font-size: 12px;padding: 1px;background-color: #eee;cursor: pointer;'>
                <i class='fa fa-pencil' aria-hidden='true'></i> Change
            </div>
        </div>
</div>"
.Replace("@ebsid@", String.IsNullOrEmpty(this.EbSid_CtxId) ? "@ebsid@" : this.EbSid_CtxId)
.Replace("@name@", this.Name)
.Replace("@src@", "/images/image.png")
.Replace("@toolTipText@", this.ToolTipText)
.Replace("@value@", "")//"value='" + this.Value + "'")
.Replace("@alt@ ", "Display Picture");
//.Replace("@maxheight@", this.MaxHeight > 0 ? this.MaxHeight.ToString() : "200");

					
		}

		public override string GetDesignHtml()
		{
			return GetHtml().RemoveCR().DoubleQuoted();
		}

		public override string GetHtml()
		{
			string EbCtrlHTML = HtmlConstants.CONTROL_WRAPER_HTML4WEB
			   .Replace("@LabelForeColor ", "color:" + (LabelForeColor ?? "@LabelForeColor ") + ";")
			   .Replace("@LabelBackColor ", "background-color:" + (LabelBackColor ?? "@LabelBackColor ") + ";");

			return ReplacePropsInHTML(EbCtrlHTML);
		}

	}
}
