﻿using ExpressBase.Common;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Objects.ServiceStack_Artifacts;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace ExpressBase.Objects
{
	[EnableInBuilder(BuilderType.BotForm)]
	public class EbCards : EbControl
	{
		[EnableInBuilder(BuilderType.BotForm)]
		[PropertyEditor(PropertyEditorType.Collection)]
		public List<EbCard> CardCollection { get; set; }

		[EnableInBuilder(BuilderType.BotForm)]
		[OSE_ObjectTypes(EbObjectTypes.iDataSource)]
		[PropertyEditor(PropertyEditorType.ObjectSelector)]
		public string DataSourceId { get; set; }

		[EnableInBuilder(BuilderType.BotForm)]
		[HideInPropertyGrid]
		public ColumnColletion Columns { get; set; }

		[EnableInBuilder(BuilderType.BotForm)]
		[PropertyEditor(PropertyEditorType.Boolean)]
		public bool MultiSelect { get; set; }

		[EnableInBuilder(BuilderType.BotForm)]
		[PropertyEditor(PropertyEditorType.Collection)]
		public List<EbButton> Buttons { get; set; }

		[EnableInBuilder(BuilderType.BotForm)]
		[PropertyEditor(PropertyEditorType.Collection)]
		public List<EbCardField> Fields { get; set; }

		public List<EbCardField> SummarizeFields { get; set; }

		public EbCards()
		{
			this.CardCollection = new List<EbCard>();
			this.Fields = new List<EbCardField>();
			this.Buttons = new List<EbButton>();
			this.Buttons.Add(new EbButton { Text = "Select" });
			this.SummarizeFields = new List<EbCardField>();
		}

		[OnDeserialized]
		public void OnDeserializedMethod(StreamingContext context)
		{
			this.BareControlHtml = this.GetBareHtml();
			this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
		}
		public override string GetToolHtml()
		{
			return @"<div eb-type='@toolName' class='tool'><i class='fa fa-window-restore'></i>@toolName</div>".Replace("@toolName", this.GetType().Name.Substring(2));
		}

		public void InitFromDataBase(JsonServiceClient ServiceClient)
		{
			RowColletion ds = (ServiceClient.Get<DataSourceDataResponse>(new DataSourceDataRequest { RefId = this.DataSourceId })).Data;

			foreach (EbDataRow cardRow in ds)
			{
				EbCard Card = new EbCard();
				foreach (EbCardField Field in this.Fields)
				{
					Type classType = Field.GetType();
					Object Obj = Activator.CreateInstance(classType);
					PropertyInfo propInfo = classType.GetProperties()[0];
					propInfo.SetValue(Obj, cardRow[Field.DbFieldMap.ColumnIndex].ToString().Trim());
					Card.Name = "CardIn" + this.Name;//------------------------"CardIn"
					Card.Fields.Add(Obj as EbCardField);
					Card.Button = new EbButton { Text = "Order" };//------------------------Text = "Order"			
				}
				this.CardCollection.Add(Card);
			}
			foreach (EbCardField Field in this.Fields)
			{
				if (!Field.Summarize)//----------------------------------------if (Field.Summarize)
					this.SummarizeFields.Add(Field);
			}
		}

		public override string GetJsInitFunc()
		{
			return @"this.Init = function(id)
					{
						//this.CardCollection.$values.push(new EbObjects.EbCard(id + '_EbCard0'));
					};";
		}

		public string ButtonsString
		{
			get
			{
				string html = @"<div class='cards-btn-cont'>";
				foreach (EbButton ec in this.Buttons)
					html += ec.GetHtml();
				return html + "</div>";
			}
			set { }
		}

		public override string GetDesignHtml()
		{
			//this.CardCollection.Add(new EbCard(Fields));
			return GetHtml().RemoveCR().DoubleQuoted();
		}

		public override string GetHtml()
		{
			return @"<div id='cont_@name@' Ctype='Cards' class='Eb-ctrlContainer' style='@hiddenString'>
						@GetBareHtml@
					</div>"
						.Replace("@name@", this.Name ?? "@name@")
						.Replace("@GetBareHtml@", this.GetBareHtml());
		}

		public override string GetBareHtml()
		{
			string html = @"<div id='@name@'><div class='cards-cont'>".Replace("@name@", this.Name ?? "@name@");
			foreach(EbCard Card in CardCollection)
			{
				html += Card.GetBareHtml();
			}
			html += "</div>@SummarizeHtml@@ButtonsString@</div>"
				.Replace("@ButtonsString@", this.ButtonsString)
				.Replace("@SummarizeHtml@", this.getCartHtml()??"");
			return html;
		}


		public string getCartHtml()
		{
			if(this.SummarizeFields.Count == 0)
			{
				return null;
			}
			else
			{
				string html = @"<div><div style='font-size: 15px;'><b> Summary : </b></div>
								<table class='table table-striped'>
									<thead><tr>";
				foreach(EbCardField F in this.SummarizeFields)
				{
					html += "<th>" + F.Name + "</th>";
				}
				html += @"</tr></thead><tbody></tbody></table></div>";
				return html;
			}						
		}
	}


	/// ////////////////////////////////

	[EnableInBuilder(BuilderType.BotForm)]
	[HideInToolBox]
	//[GenerateDynamicMetaJsFunc("genCardmeta")]
	public class EbCard : EbControl
	{
		public List<EbCardField> Fields { get; set; }

		public EbButton Button { get; set; }

		public EbCard()
		{
			this.Fields = new List<EbCardField>();
		}

		public override string GetBareHtml()
		{
			string html = @"<div id='@name@' class='card-cont' style='width:100%;'>".Replace("@name@", this.Name.Trim());
			foreach (EbCardField CardField in this.Fields)
			{
				html += CardField.GetBareHtml();
			}
			html += this.Button.GetBareHtml() + "</div>";
			return html;
		}				
	}

	[EnableInBuilder(BuilderType.BotForm)]
	public abstract class EbCardField : EbControl
	{
		[EnableInBuilder(BuilderType.BotForm)]
		[HideInPropertyGrid]
		public ColumnColletion Columns { get; set; }

		[EnableInBuilder(BuilderType.BotForm)]
		[PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns", 1)]
		[OnChangeExec(@"console.log(100); if (this.Columns.$values.length === 0 ){pg.MakeReadOnly('DbFieldMap');} else {pg.MakeReadWrite('DbFieldMap');}")]
		public EbDataColumn DbFieldMap { get; set; }

		[EnableInBuilder(BuilderType.BotForm)]
		public bool Summarize { get; set; }

		[EnableInBuilder(BuilderType.BotForm)]
		public bool HideInCard { get; set; }
	}


	[EnableInBuilder(BuilderType.BotForm)]
	//[PropertyEditor(PropertyEditorType.xxx)]
	[HideInToolBox]
	public class EbCardImageField : EbCardField
	{
		[EnableInBuilder(BuilderType.BotForm)]
		[PropertyEditor(PropertyEditorType.ImageSeletor)]
		public string ImageID { get; set; }

		public EbCardImageField() { }

		public override string GetBareHtml()
		{
			return @"<img class='card-img' src='@ImageID@'/>".Replace("@ImageID@", this.ImageID.IsNullOrEmpty() ? "../images/image.png" : this.ImageID);
		}
	}

	[EnableInBuilder(BuilderType.BotForm)]
	[HideInToolBox]
	public class EbCardHtmlField : EbCardField
	{
		[EnableInBuilder(BuilderType.BotForm)]
		[PropertyEditor(PropertyEditorType.String)]
		public string ContentHTML { get; set; }

		public EbCardHtmlField() { }

		public override string GetBareHtml()
		{
			return @"<div class='card-contenthtml'> @ContentHTML@ </div>".Replace("@ContentHTML@", this.ContentHTML.IsNullOrEmpty() ? "" : this.ContentHTML);
		}
	}

	[EnableInBuilder(BuilderType.BotForm)]
	[HideInToolBox]
	public class EbCardNumericField : EbCardField
	{
		[EnableInBuilder(BuilderType.BotForm)]
		[PropertyEditor(PropertyEditorType.Number)]
		public string Value { get; set; }

		public EbCardNumericField() { }

		public override string GetBareHtml()
		{
			return @"<input class='card-numeric' type='number' value='@Value@'>".Replace("@Value@", this.Value.IsNullOrEmpty() ? "1" : this.Value);
		}
	}

	[EnableInBuilder(BuilderType.BotForm)]
	[HideInToolBox]
	public class EbCardTextField : EbCardField
	{
		[EnableInBuilder(BuilderType.BotForm)]
		[PropertyEditor(PropertyEditorType.String)]
		public string Text { get; set; }

		public EbCardTextField() { }

		public override string GetBareHtml()
		{
			return @"<input class='card-text' type='text' value='@Text@'>".Replace("@Text@", this.Text.IsNullOrEmpty() ? "" : this.Text);
		}
	}

	[EnableInBuilder(BuilderType.BotForm)]
	[HideInToolBox]
	public class EbCardTitleField : EbCardField
	{
		[EnableInBuilder(BuilderType.BotForm)]
		[PropertyEditor(PropertyEditorType.String)]
		public string Title { get; set; }

		public EbCardTitleField() { }

		public override string GetBareHtml()
		{
			return @"<div class='card-title'>@Text@</div>".Replace("@Text@", this.Title.IsNullOrEmpty() ? "" : this.Title);
		}
	}
}













//+======================================================================================================================================================================


//	using ExpressBase.Common;
//using ExpressBase.Common.Extensions;
//using ExpressBase.Common.Objects;
//using ExpressBase.Common.Objects.Attributes;
//using ExpressBase.Common.Structures;
//using ExpressBase.Objects.Objects.DVRelated;
//using ExpressBase.Objects.ServiceStack_Artifacts;
//using ServiceStack;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Threading.Tasks;

//namespace ExpressBase.Objects
//{
//	[EnableInBuilder(BuilderType.BotForm)]
//	public class EbCards : EbControl
//	{
//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.Collection)]
//		public List<EbCard> CardCollection { get; set; }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[OSE_ObjectTypes(EbObjectTypes.iDataSource)]
//		[PropertyEditor(PropertyEditorType.ObjectSelector)]
//		public string DataSourceId { get; set; }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[HideInPropertyGrid]
//		public ColumnColletion Columns { get; set; }

//		//[EnableInBuilder(BuilderType.BotForm)]
//		//[PropertyEditor(PropertyEditorType.Boolean)]
//		////[OnChangeExec(@"if(this.IsItemCard === true){pg.ShowProperty('Price')}
//		////else{pg.HideProperty('Price')}")]
//		//public bool Summarize { get; set; }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.Boolean)]
//		public bool MultiSelect { get; set; }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.Collection)]
//		public List<EbButton> Buttons { get; set; }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.Collection)]
//		public List<EbCardField> Fields { get; set; }


//		public EbCards()
//		{
//			this.CardCollection = new List<EbCard>();
//			this.Fields = new List<EbCardField>();
//			this.Buttons = new List<EbButton>();
//			this.Buttons.Add(new EbButton { Text = "Select" });
//		}

//		[OnDeserialized]
//		public void OnDeserializedMethod(StreamingContext context)
//		{
//			this.BareControlHtml = this.GetBareHtml();
//			this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
//		}
//		public override string GetToolHtml()
//		{
//			return @"<div eb-type='@toolName' class='tool'><i class='fa fa-window-restore'></i>@toolName</div>".Replace("@toolName", this.GetType().Name.Substring(2));
//		}

//		public void InitFromDataBase(JsonServiceClient ServiceClient)
//		{
//			//this.DataSourceId = "eb_roby_dev-eb_roby_dev-2-1105-1828";
//			//RowColletion ds = (ServiceClient.Get<DataSourceDataResponse>(new DataSourceDataRequest { RefId = this.DataSourceId })).Data;
//			//string _html = string.Empty;

//			//foreach (EbDataRow cardRow in ds)
//			{
//				//EbCard Card = new EbCard(Fields);
//				//Card.Name = cardRow[0].ToString().Trim();
//				//Card.Label = cardRow[1].ToString();
//				//Card.ContentHTML = cardRow[2].ToString();
//				//Card.ImageID = cardRow[3].ToString();
//				//Card.IsItemCard = this.MultiSelect;
//				//Card.Quantity = 1;
//				//Card.Price = 115;
//				//Card.Buttons.Add(new EbButton { Text = "Add to Cart" });
//				//this.CardCollection.Add(Card);
//			}
//		}

//		public override string GetJsInitFunc()
//		{
//			return @"this.Init = function(id)
//					{
//						//this.CardCollection.$values.push(new EbObjects.EbCard(id + '_EbCard0'));
//					};";
//		}

//		public string ButtonsString
//		{
//			get
//			{
//				string html = @"<div class='cards-btn-cont'>";
//				foreach (EbButton ec in this.Buttons)
//					html += ec.GetHtml();
//				return html + "</div>";
//			}
//			set { }
//		}

//		public override string GetDesignHtml()
//		{
//			//this.CardCollection.Add(new EbCard(Fields));
//			return GetHtml().RemoveCR().DoubleQuoted();
//		}

//		public override string GetHtml()
//		{
//			return @"<div id='cont_@name@' Ctype='Cards' class='Eb-ctrlContainer' style='@hiddenString'>
//						@GetBareHtml@
//					</div>"
//						.Replace("@name@", this.Name ?? "@name@")
//						.Replace("@GetBareHtml@", this.GetBareHtml());
//		}

//		public override string GetBareHtml()
//		{
//			string html = @"<div id='@name@'><div class='cards-cont'> 
//							<img class='card-img' src='../images/image.png'/>
//							</div><div class='cards-footer'>".Replace("@name@", this.Name ?? "@name@");
//			html += this.getCartHtml() + this.ButtonsString;
//			html += "</div>";
//			return html;
//		}

//		public string getCartHtml()
//		{
//			string html = @"<div><div style='font-size: 15px;'><b> Shopping Cart : </b></div>
//								<table class='table table-striped'>
//									<thead>
//										<tr><th>Name</th><th>Quantity</th><th>Price</th><th></th></tr>
//									</thead>
//									<tbody></tbody>
//								</table>
//							</div>";
//			return html;
//		}
//	}


//	/// ////////////////////////////////

//	[EnableInBuilder(BuilderType.BotForm)]
//	[HideInToolBox]
//	//[GenerateDynamicMetaJsFunc("genCardmeta")]
//	public class EbCard : EbControl
//	{
//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.Collection)]
//		public List<EbButton> Buttons { get; set; }

//		public string ButtonsString
//		{
//			get
//			{
//				string html = @"<div class='card-btn-cont'>";
//				foreach (EbButton ec in this.Buttons)
//					html += ec.GetHtml();
//				return html + "</div>";
//			}
//			set { }
//		}

//		public EbCard()
//		{
//			this.Buttons = new List<EbButton>();
//		}

//		//public EbCard(List<EbCardField> Fields)
//		//{
//		//	this.Buttons = new List<EbButton>();
//		//}

//		//public override string GetDesignHtml()
//		//{
//		//	return GetBareHtml().RemoveCR().DoubleQuoted();
//		//}


//		//public override string GetBareHtml()
//		//{
//		//	string html = @"<div id='@name@' class='card-cont' style='width:100%;'>
//		//						<img class='card-img' src='@ImageID@'/>
//		//						<div class='card-bottom'>
//		//							<div id='@name@Lbl' class='card-label' style='@LabelBackColor  @LabelForeColor font-weight: bold'> @Label@ </div>
//		//							<div class='card-content'>
//		//								@ContentHTML@
//		//							</div>
//		//							@CardHtml@
//		//							@ButtonCollection@
//		//						</div>
//		//					</div>"
//		//		   .Replace("@ButtonCollection@", this.ButtonsString)
//		//		   .Replace("@name@", this.Name)//this.IsItemCard ? this.Name : "this.EbSid"
//		//		   .Replace("@ContentHTML@", this.ContentHTML) //"Chat has become the center of the smartphone universe, so it makes sense that bots are being used to deliver information in a convenient and engaging manner. But how do brands or media companies")//
//		//		   .Replace("@Label@", this.Label)//"TechCrunch")//
//		//		   .Replace("@ImageID@", this.ImageID.IsNullOrEmpty() ? "../images/image.png" : this.ImageID)//"https://tctechcrunch2011.files.wordpress.com/2016/03/chat-bot.jpg?w=738")//
//		//		   .Replace("@CardHtml@", this.GetItemCardHtml());//this.IsItemCard ? this.GetItemCardHtml() : ""
//		//	return html;
//		//}

//		//public override string GetHtml()
//		//{
//		//	return GetBareHtml();
//		//}

//		//public string GetItemCardHtml()
//		//{
//		//	string html = @"<div style='width: 50%; display: inline-block;'>
//		//						Quantity : <input class='item-quantity' type='number' value='1' min='1' max='10' style='width: 50%;'>
//		//					</div>
//		//					<div style='width: 45%; display: inline-block;'>
//		//						Price : <input class='item-price' type='text' value='@Price@' readonly style='width: 50%;'>
//		//					</div>"
//		//			.Replace("@name@", this.Name)
//		//			.Replace("@Price@", "215.50");
//		//	return html;
//		//}
//	}

//	[EnableInBuilder(BuilderType.BotForm)]
//	public abstract class EbCardField : EbControl
//	{
//		[EnableInBuilder(BuilderType.BotForm)]
//		[HideInPropertyGrid]
//		public ColumnColletion Columns { get; set; }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns", 1)]
//		[OnChangeExec(@"
//console.log(100);
//if (this.Columns.$values.length === 0 ){
//    pg.MakeReadOnly('DbFieldMap');
//}
//else {
//    pg.MakeReadWrite('DbFieldMap');
//}
//            ")]
//		public EbDataColumn DbFieldMap { get; set; }

//		[EnableInBuilder(BuilderType.BotForm)]
//		public bool Summarize { get; set; }
//	}


//	[EnableInBuilder(BuilderType.BotForm)]
//	//[PropertyEditor(PropertyEditorType.xxx)]
//	[HideInToolBox]
//	public class EbCardImageField : EbCardField
//	{
//		public EbCardImageField() { }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.ImageSeletor)]
//		public string ImageID { get; set; }

//		public override string GetBareHtml()
//		{
//			return @"<img class='card-img' src='@ImageID@'/>".Replace("@ImageID@", this.ImageID.IsNullOrEmpty() ? "../images/image.png" : this.ImageID);
//		}
//	}

//	[EnableInBuilder(BuilderType.BotForm)]
//	[HideInToolBox]
//	public class EbCardHtmlField : EbCardField
//	{
//		public EbCardHtmlField() { }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.String)]
//		public string ContentHTML { get; set; }

//		public override string GetBareHtml()
//		{
//			return @"<div>@ContentHTML@</div>".Replace("@ImageID@", this.ContentHTML.IsNullOrEmpty() ? "" : this.ContentHTML);
//		}
//	}

//	[EnableInBuilder(BuilderType.BotForm)]
//	[HideInToolBox]
//	public class EbCardNumericField : EbCardField
//	{
//		public EbCardNumericField() { }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.Number)]
//		public string Value { get; set; }

//		//[HideInPropertyGrid]
//		//public new EbDataColumn DbFieldMap { get; set; }

//		public override string GetBareHtml()
//		{
//			return @"<input class='card-numeric' type='number' value='@Value@'>".Replace("@Value@", this.Value.IsNullOrEmpty() ? "1" : this.Value);
//		}
//	}

//	[EnableInBuilder(BuilderType.BotForm)]
//	[HideInToolBox]
//	public class EbCardTextField : EbCardField
//	{
//		public EbCardTextField() { }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.String)]
//		public string Text { get; set; }

//		public override string GetBareHtml()
//		{
//			return @"<input class='card-text' type='text' value='@Text@'>".Replace("@Text@", this.Text.IsNullOrEmpty() ? "" : this.Text);
//		}
//	}









//	[EnableInBuilder(BuilderType.BotForm)]
//	[HideInToolBox]
//	//[PropertyEditor(PropertyEditorType.Date)]
//	public class EbCardDateField : EbCardField
//	{
//		public EbCardDateField() { }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.DateTime)]
//		public DateTime? Max { get; set; }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.DateTime)]
//		public DateTime? Min { get; set; }

//		[EnableInBuilder(BuilderType.BotForm)]
//		[PropertyEditor(PropertyEditorType.DateTime)]
//		public string Date { get; set; }
//	}

//	public enum EBControlType
//	{
//		Image = 0,
//		Html,
//		TextBox,
//		NumericBox,
//		RadioGroup,
//		CheckBox,
//		Date,
//		DropDown,
//	}
//}

//<div ><button id='BtnSelect@name@' class='cardselectbtn btn btn-default' style='width:100%; vertical-align: bottom;'>Select</button></div>

//[PropertyGroup("Appearance")]
//[EnableInBuilder(BuilderType.BotForm)]
//[PropertyEditor(PropertyEditorType.FontSelector)]
//public EbFont Font{ get; set; }