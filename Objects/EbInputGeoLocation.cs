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
    class EbInputGeoLocation : EbControl
    {
        public EbInputGeoLocation() { }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        [PropertyEditor(PropertyEditorType.Expandable)]
        public LatLng Position { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        public string ContentHTML { get; set; }

        public override string GetDesignHtml()
        {
            return GetHtml().RemoveCR().DoubleQuoted();
        }

        public override string GetBareHtml()
        {
            return @" 
                <div class='location-cont'>
                    <div id='@name@_Cont' class='location-box picker-box' style='display:block;'>
                        <div class='locinp-cont'>
                            <div class='locinp-wraper'><span class='locinp-span'>Latitude</span><input id='@name@lat' class='locinp' type='text'/></div>
                            <div class='locinp-wraper'><span class='locinp-span'>Longitude</span><input id='@name@long' class='locinp' type='text'/></div>
                            <div class='locinp-wraper-address'><span class='locinp-span'>Address</span><input id='@name@address' class='locinp' type='text'/></div>
                        </div>
                        <div id='@name@' class='map-div'></div>
                        <div class='loc-bottom'>
                            <div id='@name@Lbl' class='loc-label' style='@LabelBackColor@  @LabelForeColor@ font-weight: bold'> @Label@ </div><button class='choose-btn'>Choose</button>
                            <div class='loc-content'>
                                @ContentHTML@
                            </div>
                        </div>
                    </div>  
                </div>"
.Replace("@name@", (this.Name != null) ? this.Name : "@name@")
.Replace("@LabelBackColor@", this.LabelBackColor)
.Replace("@LabelForeColor@", this.LabelForeColor)
.Replace("@Label@", this.Label)
.Replace("@ContentHTML@", this.ContentHTML);
            ;
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
}