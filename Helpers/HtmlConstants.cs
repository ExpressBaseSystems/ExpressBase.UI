﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Objects.Helpers
{
    public static class HtmlConstants
    {
        public const string Label = @"";

        public const string HelpText = @"";

        public const string CONTROL_WRAPER_HTML4WEB = @"
        <div id='cont_@name@'  ebsid='@ebsid@' class='Eb-ctrlContainer' ebsid='@name@' Ctype='@type@' style='@hiddenString'>
            <span class='eb-ctrl-label' ui-label id='@nameLbl'>@Label@ </span>
                <div  class='@name@Wraper'>
                    @barehtml@
                </div>
            <span class='helpText' ui-helptxt >@HelpText@ </span>
        </div>";
    }
}
