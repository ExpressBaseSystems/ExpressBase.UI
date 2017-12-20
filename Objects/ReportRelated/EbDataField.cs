﻿using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ExpressBase.Objects.ReportRelated
{
    public abstract class EbDataField : EbReportField
    {
        public virtual void NotifyNewPage(bool status) { }

        public override void DrawMe(PdfContentByte canvas, float reportHeight, float printingTop, float detailprintingtop, string column_val)
        {
            var ury = reportHeight - (printingTop + this.Top + detailprintingtop);
            var lly = reportHeight - (printingTop + this.Top + this.Height + detailprintingtop);

            ColumnText ct = new ColumnText(canvas);
            ct.Canvas.SetColorFill(GetColor(this.ForeColor));
            ct.SetSimpleColumn(new Phrase(column_val), this.Left, lly, this.Width + this.Left, ury, 15, Element.ALIGN_LEFT);
            ct.Go();
        }
    }

    [EnableInBuilder(BuilderType.Report)]
    public class EbDataFieldText : EbDataField
    {

        public override string GetDesignHtml()
        {
            return "<div class='EbCol dropped' $type='@type' eb-type='DataFieldText' id='@id' style='border: @Border px solid;border-color: @BorderColor ; width: @Width px; background-color:@BackColor ; color:@ForeColor ; height: @Height px; position: absolute; left: @Left px; top: @Top px;text-align: @TextAlign ;'> @Title </div>".RemoveCR().DoubleQuoted();
        }

        public override string GetJsInitFunc()
        {
            return @"
    this.Init = function(id)
        {
    this.Height =25;
    this.Width= 200;
    this.ForeColor = '#201c1c';
    this.Border = 1;
    this.BorderColor = '#aaaaaa'
};";
        }
    }

    [EnableInBuilder(BuilderType.Report)]
    public class EbDataFieldDateTime : EbDataField
    {
        public override string GetDesignHtml()
        {
            return "<div class='EbCol dropped' $type='@type' eb-type='DataFieldDateTime' id='@id' style='border: @Border px solid;border-color: @BorderColor ; width: @Width px; background-color:@BackColor ; color:@ForeColor ; height: @Height px; position: absolute; left: @Left px; top: @Top px;text-align: @TextAlign;'> @Title </div>".RemoveCR().DoubleQuoted();
        }

        public override string GetJsInitFunc()
        {
            return @"
    this.Init = function(id)
        {
    this.Height =25;
    this.Width= 200;
    this.ForeColor = '#201c1c';
    this.Border = 1;
    this.BorderColor = '#aaaaaa'
};";
        }
    }

    [EnableInBuilder(BuilderType.Report)]
    public class EbDataFieldBoolean : EbDataField
    {
        public override string GetDesignHtml()
        {
            return "<div class='EbCol dropped' $type='@type' eb-type='DataFieldBoolean' id='@id' style='border: @Border px solid;border-color: @BorderColor ; width: @Width px; background-color:@BackColor ; color:@ForeColor ; height: @Height px; position: absolute; left: @Left px; top: @Top px;text-align: @TextAlign;'> @Title </div>".RemoveCR().DoubleQuoted();
        }

        public override string GetJsInitFunc()
        {
            return @"
    this.Init = function(id)
        {
    this.Height =25;
    this.Width= 200;
    this.ForeColor = '#201c1c';
    this.Border = 1;
    this.BorderColor = '#aaaaaa'
};";
        }
    }

    [EnableInBuilder(BuilderType.Report)]
    public class EbDataFieldNumeric : EbDataField
    {
        [EnableInBuilder(BuilderType.Report)]
        [PropertyGroup("General")]
        public int DecimalPlaces { get; set; }

        [EnableInBuilder(BuilderType.Report)]
        [PropertyGroup("General")]
        public bool InLetters { get; set; }

        public override string GetDesignHtml()
        {
            return "<div class='EbCol dropped' $type='@type' eb-type='DataFieldNumeric' id='@id' style='border: @Border px solid;border-color: @BorderColor ; width: @Width px; background-color:@BackColor ; color:@ForeColor ; height: @Height px; position: absolute; left: @Left px; top: @Top px;text-align: @TextAlign;'> @Title </div>".RemoveCR().DoubleQuoted();
        }

        public override string GetJsInitFunc()
        {
            return @"
    this.Init = function(id)
        {
    this.Height =25;
    this.Width= 200;
    this.ForeColor = '#201c1c';
    this.Border = 1;
    this.BorderColor = '#aaaaaa'
};";
        }
        public override void DrawMe(PdfContentByte canvas, float reportHeight, float printingTop, float detailprintingtop, string column_val)
        {
            var ury = reportHeight - (printingTop + this.Top + detailprintingtop);
            var lly = reportHeight - (printingTop + this.Top + this.Height + detailprintingtop);            
            if (this.DecimalPlaces > 0)
                column_val = Convert.ToDecimal(column_val).ToString("F" + this.DecimalPlaces);
            if (this.InLetters)
            {
                NumberToEnglish numToE = new NumberToEnglish();
                column_val = numToE.changeCurrencyToWords(column_val);
            }

            ColumnText ct = new ColumnText(canvas);
            ct.Canvas.SetColorFill(GetColor(this.ForeColor));
            ct.SetSimpleColumn(new Phrase(column_val), this.Left, lly, this.Width + this.Left, ury, 15, Element.ALIGN_RIGHT);
            ct.Go();
        }
        public class NumberToEnglish
        {
            public String changeNumericToWords(double numb)
            {
                String num = numb.ToString();
                return changeToWords(num, false);
            }
            public String changeCurrencyToWords(String numb)
            {
                return changeToWords(numb, true);
            }
            public String changeNumericToWords(String numb)
            {
                return changeToWords(numb, false);
            }
            public String changeCurrencyToWords(double numb)
            {
                return changeToWords(numb.ToString(), true);
            }
            private String changeToWords(String numb, bool isCurrency)
            {
                String val = "", wholeNo = numb, points = "", andStr = "", pointStr = "";
                String endStr = (isCurrency) ? ("Only") : ("");
                try
                {
                    int decimalPlace = numb.IndexOf(".");
                    if (decimalPlace > 0)
                    {
                        wholeNo = numb.Substring(0, decimalPlace);
                        points = numb.Substring(decimalPlace + 1);
                        if (Convert.ToInt32(points) > 0)
                        {
                            andStr = (isCurrency) ? ("and") : ("point");// just to separate whole numbers from points/cents
                            endStr = (isCurrency) ? ("Dirhams " + endStr) : ("");
                            pointStr = translateCents(points);
                        }
                    }
                    val = String.Format("{0} {1}{2} {3}", TranslateWholeNumber(wholeNo).Trim(), andStr, pointStr, endStr);
                }
                catch {; }
                return val;
            }
            private String TranslateWholeNumber(String number)
            {
                string word = "";
                try
                {
                    bool beginsZero = false;//tests for 0XX
                    bool isDone = false;//test if already translated
                    double dblAmt = (Convert.ToDouble(number));
                    //if ((dblAmt > 0) && number.StartsWith("0"))
                    if (dblAmt > 0)
                    {//test for zero or digit zero in a nuemric
                        beginsZero = number.StartsWith("0");
                        int numDigits = number.Length;
                        int pos = 0;//store digit grouping
                        String place = "";//digit grouping name:hundres,thousand,etc...
                        switch (numDigits)
                        {
                            case 1://ones' range
                                word = ones(number);
                                isDone = true;
                                break;
                            case 2://tens' range
                                word = tens(number);
                                isDone = true;
                                break;
                            case 3://hundreds' range
                                pos = (numDigits % 3) + 1;
                                place = " Hundred ";
                                break;
                            case 4://thousands' range
                            case 5:
                            case 6:
                                pos = (numDigits % 4) + 1;
                                place = " Thousand ";
                                break;
                            case 7://millions' range
                            case 8:
                            case 9:
                                pos = (numDigits % 7) + 1;
                                place = " Million ";
                                break;
                            case 10://Billions's range
                                pos = (numDigits % 10) + 1;
                                place = " Billion ";
                                break;
                            //add extra case options for anything above Billion...
                            default:
                                isDone = true;
                                break;
                        }
                        if (!isDone)
                        {//if transalation is not done, continue...(Recursion comes in now!!)
                            word = TranslateWholeNumber(number.Substring(0, pos)) + place + TranslateWholeNumber(number.Substring(pos));
                            //check for trailing zeros
                            if (beginsZero) word = " and " + word.Trim();
                        }
                        //ignore digit grouping names
                        if (word.Trim().Equals(place.Trim())) word = "";
                    }
                }
                catch {; }
                return word.Trim();
            }
            private String tens(String digit)
            {
                int digt = Convert.ToInt32(digit);
                String name = null;
                switch (digt)
                {
                    case 10:
                        name = "Ten";
                        break;
                    case 11:
                        name = "Eleven";
                        break;
                    case 12:
                        name = "Twelve";
                        break;
                    case 13:
                        name = "Thirteen";
                        break;
                    case 14:
                        name = "Fourteen";
                        break;
                    case 15:
                        name = "Fifteen";
                        break;
                    case 16:
                        name = "Sixteen";
                        break;
                    case 17:
                        name = "Seventeen";
                        break;
                    case 18:
                        name = "Eighteen";
                        break;
                    case 19:
                        name = "Nineteen";
                        break;
                    case 20:
                        name = "Twenty";
                        break;
                    case 30:
                        name = "Thirty";
                        break;
                    case 40:
                        name = "Fourty";
                        break;
                    case 50:
                        name = "Fifty";
                        break;
                    case 60:
                        name = "Sixty";
                        break;
                    case 70:
                        name = "Seventy";
                        break;
                    case 80:
                        name = "Eighty";
                        break;
                    case 90:
                        name = "Ninety";
                        break;
                    default:
                        if (digt > 0)
                        {
                            name = tens(digit.Substring(0, 1) + "0") + " " + ones(digit.Substring(1));
                        }
                        break;
                }
                return name;
            }
            private String ones(String digit)
            {
                int digt = Convert.ToInt32(digit);
                String name = "";
                switch (digt)
                {
                    case 1:
                        name = "One";
                        break;
                    case 2:
                        name = "Two";
                        break;
                    case 3:
                        name = "Three";
                        break;
                    case 4:
                        name = "Four";
                        break;
                    case 5:
                        name = "Five";
                        break;
                    case 6:
                        name = "Six";
                        break;
                    case 7:
                        name = "Seven";
                        break;
                    case 8:
                        name = "Eight";
                        break;
                    case 9:
                        name = "Nine";
                        break;
                }
                return name;
            }
            private String translateCents(String cents)
            {
                String cts = "", digit = "", engOne = "";
                for (int i = 0; i < cents.Length; i++)
                {
                    digit = cents[i].ToString();
                    if (digit.Equals("0"))
                    {
                        engOne = "Zero";
                    }
                    else
                    {
                        engOne = ones(digit);
                    }
                    cts += " " + engOne;
                }
                return cts;
            }
        }
    }

    [EnableInBuilder(BuilderType.Report)]
    public class EbDataFieldNumericSummary : EbDataFieldNumeric
    {
        [EnableInBuilder(BuilderType.Report)]
        [HideInPropertyGrid]
        public string DataField { get; set; }

        [EnableInBuilder(BuilderType.Report)]
        public SummaryFunctionsNumeric Function { get; set; }

        [EnableInBuilder(BuilderType.Report)]
        [HideInPropertyGrid]
        public bool ResetOnNewPage { get; set; }

        private int Count { get; set; }

        private decimal Sum { get; set; }

        private decimal Max { get; set; }

        private decimal Min { get; set; }

        public decimal SummarizedValue
        {
            get
            {
                if (this.Function == SummaryFunctionsNumeric.Sum)
                    return this.Sum;
                else if (this.Function == SummaryFunctionsNumeric.Average && this.Count > 0)
                    return this.Sum / this.Count;
                else if (this.Function == SummaryFunctionsNumeric.Count)
                    return this.Count;
                else if (this.Function == SummaryFunctionsNumeric.Max)
                    return this.Max;
                else if (this.Function == SummaryFunctionsNumeric.Min)
                    return this.Min;

                return 0;
            }
        }

        public void Summarize(dynamic value)
        {
            this.Count++;
            value = Convert.ToDecimal(value);

            if (this.Function == SummaryFunctionsNumeric.Sum || this.Function == SummaryFunctionsNumeric.Average)
            {
                if (this.Function == SummaryFunctionsNumeric.Sum || this.Function == SummaryFunctionsNumeric.Average)
                    this.Sum += value;
            }

            if (this.Count > 1)
            {
                if (this.Function == SummaryFunctionsNumeric.Max)
                    this.Max = (this.Max > value) ? this.Max : value;
                else if (this.Function == SummaryFunctionsNumeric.Min)
                    this.Min = (this.Min < value) ? this.Min : value;
            }
            else
            {
                if (this.Function == SummaryFunctionsNumeric.Max)
                    this.Max = value;
                else if (this.Function == SummaryFunctionsNumeric.Min)
                    this.Min = value;
            }
        }

        public override void NotifyNewPage(bool status)
        {
            if (status && this.ResetOnNewPage)
                this.Sum = 0;
        }

        public override string GetDesignHtml()
        {
            return "<div class='dropped' $type='@type' eb-type='DataFieldNumericSummary' id='@id' style='border: @Border px solid;border-color: @BorderColor ; width: @Width px; background-color:@BackColor ; color:@ForeColor ; height: @Height px; position: absolute; left: @Left px; top: @Top px;text-align: @TextAlign;'> @Title </div>".RemoveCR().DoubleQuoted();
        }
        public override string GetJsInitFunc()
        {
            return @"
    this.Init = function(id)
        {
    this.Height =25;
    this.Width= 200;
    this.ForeColor = '#201c1c';
    this.Border = 1;
    this.BorderColor = '#aaaaaa'
};";
        }
        public override void DrawMe(PdfContentByte canvas, float reportHeight, float printingTop, float detailprintingtop, string column_val)
        {
            var ury = reportHeight - (printingTop + this.Top + detailprintingtop);
            var lly = reportHeight - (printingTop + this.Top + this.Height + detailprintingtop);
            if (this.DecimalPlaces > 0)
                column_val = Convert.ToDecimal(column_val).ToString("F" + this.DecimalPlaces);
            if (this.InLetters)
            {
                NumberToEnglish numToE = new NumberToEnglish();
                column_val = numToE.changeCurrencyToWords(column_val);
            }

            ColumnText ct = new ColumnText(canvas);
            ct.Canvas.SetColorFill(GetColor(this.ForeColor));
            ct.SetSimpleColumn(new Phrase(column_val), this.Left, lly, this.Width + this.Left, ury, 15, Element.ALIGN_RIGHT);
            ct.Go();
        }      
    }
    [EnableInBuilder(BuilderType.Report)]
    public class EbDataFieldTextSummary : EbDataFieldText
    {
        [EnableInBuilder(BuilderType.Report)]
        [HideInPropertyGrid]
        public string DataField { get; set; }

        [EnableInBuilder(BuilderType.Report)]
        public SummaryFunctionsText Function { get; set; }


        [EnableInBuilder(BuilderType.Report)]
        [HideInPropertyGrid]
        public bool ResetOnNewPage { get; set; }

        private int Count { get; set; }

        private string Max { get; set; } = "";

        private string Min { get; set; } = "";

        public dynamic SummarizedValue
        {
            get
            {
                if (this.Function == SummaryFunctionsText.Count)
                    return this.Count;
                else if (this.Function == SummaryFunctionsText.Max)
                    return this.Max;
                else if (this.Function == SummaryFunctionsText.Min)
                    return this.Min;

                return 0;
            }
        }

        public void Summarize(dynamic value)
        {
            value = value.ToString();
            this.Count++;
            if (this.Count > 1)
            {
                if (this.Function == SummaryFunctionsText.Max)
                    this.Max = (this.Max.CompareTo(value) > 0) ? this.Max : value;
                else if (this.Function == SummaryFunctionsText.Min)
                    this.Min = (this.Min.CompareTo(value) > 0) ? value : this.Min;
            }
            else
            {
                if (this.Function == SummaryFunctionsText.Max)
                    this.Max = value;
                else if (this.Function == SummaryFunctionsText.Min)
                    this.Min = value;
            }
        }

        public override string GetDesignHtml()
        {
            return "<div class='dropped' $type='@type' eb-type='DataFieldTextSummary' id='@id' style='border: @Border px solid;border-color: @BorderColor ; width: @Width px; background-color:@BackColor ; color:@ForeColor ; height: @Height px; position: absolute; left: @Left px; top: @Top px;text-align: @TextAlign;'> @Title </div>".RemoveCR().DoubleQuoted();
        }

        public override string GetJsInitFunc()
        {
            return @"
    this.Init = function(id)
        {
    this.Height =25;
    this.Width= 200;
    this.ForeColor = '#201c1c';
    this.Border = 1;
    this.BorderColor = '#aaaaaa'
};";
        }
    }

    [EnableInBuilder(BuilderType.Report)]
    public class EbDataFieldDateTimeSummary : EbDataFieldDateTime
    {
        [EnableInBuilder(BuilderType.Report)]
        [HideInPropertyGrid]
        public string DataField { get; set; }

        [EnableInBuilder(BuilderType.Report)]
        public SummaryFunctionsDateTime Function { get; set; }

        [EnableInBuilder(BuilderType.Report)]
        [HideInPropertyGrid]
        public bool ResetOnNewPage { get; set; }

        private int Count { get; set; }

        private DateTime Max { get; set; }

        private DateTime Min { get; set; }

        public dynamic SummarizedValue
        {
            get
            {
                if (this.Function == SummaryFunctionsDateTime.Count)
                    return this.Count;
                else if (this.Function == SummaryFunctionsDateTime.Max)
                    return this.Max;
                else if (this.Function == SummaryFunctionsDateTime.Min)
                    return this.Min;

                return 0;
            }
        }

        public void Summarize(dynamic value)
        {
            value = Convert.ToDateTime(value);
            this.Count++;
            if (this.Count > 1)
            {
                if (this.Function == SummaryFunctionsDateTime.Max)
                    this.Max = (DateTime.Compare(this.Max, value) > 0) ? this.Max : value;
                if (this.Function == SummaryFunctionsDateTime.Min)
                    this.Min = (DateTime.Compare(this.Min, value) > 0) ? value : this.Min;
            }
            else
            {
                if (this.Function == SummaryFunctionsDateTime.Max)
                    this.Max = value;
                if (this.Function == SummaryFunctionsDateTime.Min)
                    this.Min = value;
            }
        }

        public override string GetDesignHtml()
        {
            return "<div class='dropped' $type='@type' eb-type='DataFieldDateTimeSummary' id='@id' style='border: @Border px solid;border-color: @BorderColor ; width: @Width px; background-color:@BackColor ; color:@ForeColor ; height: @Height px; position: absolute; left: @Left px; top: @Top px;text-align: @TextAlign;'> @Title </div>".RemoveCR().DoubleQuoted();
        }
        public override string GetJsInitFunc()
        {
            return @"
    this.Init = function(id)
        {
    this.Height =25;
    this.Width= 200;
    this.ForeColor = '#201c1c';
    this.Border = 1;
    this.BorderColor = '#aaaaaa'
};";
        }
    }

    [EnableInBuilder(BuilderType.Report)]
    public class EbDataFieldBooleanSummary : EbDataFieldBoolean
    {
        [EnableInBuilder(BuilderType.Report)]
        [HideInPropertyGrid]
        public string DataField { get; set; }

        [EnableInBuilder(BuilderType.Report)]
        public SummaryFunctionsBoolean Function { get; set; }


        [EnableInBuilder(BuilderType.Report)]
        [HideInPropertyGrid]
        public bool ResetOnNewPage { get; set; }

        private int Count { get; set; }

        public decimal SummarizedValue
        {
            get
            {
                if (this.Function == SummaryFunctionsBoolean.Count)
                    return this.Count;
                return 0;
            }
        }

        public void Summarize()
        {
            this.Count++;
        }

        public override string GetDesignHtml()
        {
            return "<div class='dropped' $type='@type' eb-type='DataFieldBooleanSummary' id='@id' style='border: @Border px solid;border-color: @BorderColor ; width: @Width px; background-color:@BackColor ; color:@ForeColor ; height: @Height px; position: absolute; left: @Left px; top: @Top px;text-align: @TextAlign ;'> @Title </div>".RemoveCR().DoubleQuoted();
        }
        public override string GetJsInitFunc()
        {
            return @"
    this.Init = function(id)
        {
    this.Height =25;
    this.Width= 200;
    this.ForeColor = '#201c1c';
    this.Border = 1;
    this.BorderColor = '#aaaaaa'
};";
        }
    }
}

