﻿using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ExpressBase.Objects
{
    [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
    public class EbLocations : EbControl
    {
        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbLocation> LocationCollection { get; set; }

        public EbLocations()
        {
            this.LocationCollection = new List<EbLocation>();
        }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
        }

        public override string GetJsInitFunc()
        {
            return @"
this.Init = function(id)
{
    this.LocationCollection.push(new EbObjects.EbLocation(id + '_loc0'));
    this.LocationCollection.push(new EbObjects.EbLocation(id + '_loc1'));
};";
        }

        private string getOptButtons()
        {
            string optHTML =string.Empty;

            foreach (EbLocation ec in this.LocationCollection)
                optHTML += "<div class='loc-opt-btn' tabindex='0' style='width:@width@%;'>@optName@</div>"
.Replace("@optName@", (ec.ShortName == null || ec.ShortName.Trim() == string.Empty) ? ec.Label.Split(" ")[0] : ec.ShortName)
.Replace("@width@",(100/this.LocationCollection.Count).ToString());

            return optHTML;
        }

        public override string GetDesignHtml()
        {
            this.LocationCollection.Add(new EbLocation());
            this.LocationCollection.Add(new EbLocation());
            return GetHtml().RemoveCR().DoubleQuoted();
        }

        public override string GetBareHtml()
        {
            string html = @"
                <div id='@name@' class='location-cont'>
                    <div class='loc-opt-cont'>
                            @options@
                    </div>"
.Replace("@name@", (this.Name != null) ? this.Name : "@name@")
.Replace("@options@", this.getOptButtons());

            foreach (EbLocation ec in this.LocationCollection)
                html += ec.GetHtml();

            return html + "</div>"
.Replace("@name@", (this.Name != null) ? this.Name : "@name@");
        }

        public override string GetHtml()
        {
            return @"
            <div id='cont_@name@' Ctype='Locations' class='Eb-ctrlContainer' style='@hiddenString'>
                @GetBareHtml@
            </div>"
.Replace("@name@", (this.Name != null) ? this.Name : "@name@")
.Replace("@GetBareHtml@", this.GetBareHtml());
        }

    }

    /// ////////////////////////////////

    [EnableInBuilder(BuilderType.BotForm)]
    [HideInToolBox]
    public class EbLocation : EbControl
    {

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        [PropertyEditor(PropertyEditorType.Expandable)]
        public LatLng Position { get; set; }
        //public LatLng Lat_Long { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        [PropertyEditor(PropertyEditorType.Expandable)]
        public string ShortName { get; set; }

        public EbLocation() { }

        public override string GetBareHtml()
        {
            return @"
<div id='@name@_Cont' class='location-box'>
    <div id='@name@' class='map-div'></div>
    <div class='loc-bottom'>
        <div id='@name@Lbl' class='loc-label' style='@LabelBackColor  @LabelForeColor font-weight: bold'> @Label@ </div>
        <div class='loc-content'>
            @ContentHTML@
        </div>
    </div>
</div>"
.Replace("@name@", (this.Name != null) ? this.Name : "@name@")
.Replace("@Label@", this.Label);
        }

        public override string GetHtml()
        {
            return GetBareHtml();
        }
    }

    [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
    public class LatLng
    {
        public LatLng() { }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        [PropertyEditor(PropertyEditorType.Number)]
        public Decimal Latitude { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        [PropertyEditor(PropertyEditorType.Number)]
        public Decimal Longitude { get; set; }

        public string GetHtml()
        {
            return "";
        }
    }
}
