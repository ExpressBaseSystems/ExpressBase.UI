﻿using ExpressBase.Common;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ExpressBase.Objects.Objects
{
	[EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
	public class EbTagInput: EbControlUI
	{
		public EbTagInput()
		{

		}

		[OnDeserialized]
		public void OnDeserializedMethod(StreamingContext context)
		{
			this.BareControlHtml = this.GetBareHtml();
			this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
		}

		public override string ToolIconHtml { get { return "<i class='fa fa-tags'></i>"; } set { } }

		public override string ToolNameAlias { get { return "Tag Input"; } set { } }

		public override string ToolHelpText { get { return "Tag Input"; } set { } }

		public override string UIchangeFns
		{
			get
			{
				return @"EbTagInput = {
                
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

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }



		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		public bool Textareas { get; set; }

		[EnableInBuilder(BuilderType.WebForm)]
		[HideInPropertyGrid]
		[PropertyEditor(PropertyEditorType.Color)]
		public string TagColor { get; set; }






		public override string GetBareHtml()
		{


			return @" 
 <div id='@ebsid@_TagDiv'  >  
	<input type='text' name='@ebsid@_tags' value='' data-role='tagsinput'  />
</div>"
.Replace("@ebsid@", String.IsNullOrEmpty(this.EbSid_CtxId) ? "@ebsid@" : this.EbSid_CtxId)
.Replace("@name@", this.Name)
.Replace("@toolTipText@", this.ToolTipText)
.Replace("@value@", "");

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
		public override string GetValueFromDOMJSfn
		{
			get
			{
				return @" return $('input[name = ' + this.EbSid_CtxId +'_tags]').val();";
			}
			set { }
		}

		public override string OnChangeBindJSFn
		{
			get
			{
				return @"$('input[name = ' + this.EbSid_CtxId + '_tags]').on('change', p1);";
			}
			set { }
		}

		public override string SetValueJSfn
		{
			get
			{
				return @"$('input[name = ' + this.EbSid_CtxId + '_tags]').tagsinput('refresh');
							$('input[name = ' + this.EbSid_CtxId + '_tags]').tagsinput('add', p1);
						$(`[ebsid=${this.EbSid}]`).find('#' + this.EbSid + '_TagDiv').find('.bootstrap-tagsinput').css({ 'border': 'none' });";
			}
			set { }
		}

		public override string ClearJSfn
		{
			get
			{
				return @"$('input[name = ' + this.EbSid_CtxId + '_tags]').val('');";
			}
			set { }
		}

		
	}
}
