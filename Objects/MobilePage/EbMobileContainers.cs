﻿using ExpressBase.Common;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Objects.Objects.MobilePage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ExpressBase.Objects
{
    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileContainer : EbMobilePageBase
    {

    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileForm : EbMobileContainer
    {
        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override string Name { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public List<EbMobileControl> ChildControls { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public string TableName { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [Alias("Auto Deploy Visualization")]
        public bool AutoDeployMV { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public string AutoGenMVRefid { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [OSE_ObjectTypes(EbObjectTypes.iWebForm)]
        [Alias("Web Form")]
        public string WebFormRefId { set; get; }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_mob_form_container mob_container dropped' tabindex='1' eb-type='EbMobileForm' id='@id'>
                        <div class='eb_mob_container_inner'>
                        </div>
                    </div>".RemoveCR().DoubleQuoted();
        }

        public Dictionary<string, List<TableColumnMeta>> GetTableMetaCollection(IVendorDbTypes vDbTypes)
        {
            Dictionary<string, List<TableColumnMeta>> meta = new Dictionary<string, List<TableColumnMeta>>();
            try
            {
                meta.Add(this.TableName, new List<TableColumnMeta>());

                foreach (EbMobileControl ctrl in this.ChildControls)
                {
                    if (ctrl is EbMobileFileUpload)
                        continue;
                    else if (ctrl is EbMobileTableLayout)
                    {
                        foreach (EbMobileTableCell cell in (ctrl as EbMobileTableLayout).CellCollection)
                        {
                            foreach (EbMobileControl tctrl in cell.ControlCollection)
                            {
                                if (tctrl is EbMobileFileUpload)
                                    continue;
                                else
                                    AppendMeta(meta[this.TableName], tctrl, vDbTypes);
                            }
                        }
                    }
                    else if (ctrl is EbMobileDataGrid)
                    {
                        var grid = (ctrl as EbMobileDataGrid);
                        meta.Add(grid.TableName, new List<TableColumnMeta>());
                        meta[grid.TableName].Add(new TableColumnMeta
                        {
                            Name = this.TableName + "_id",
                            Type = vDbTypes.GetVendorDbTypeStruct(EbDbTypes.Int32)
                        });

                        foreach (EbMobileControl gctrl in grid.ChildControls)
                            AppendMeta(meta[grid.TableName], gctrl, vDbTypes);

                        AppendDefaultMeta(meta[grid.TableName], vDbTypes);
                    }
                    else
                        AppendMeta(meta[this.TableName], ctrl, vDbTypes);
                }
                AppendDefaultMeta(meta[this.TableName], vDbTypes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return meta;
        }

        private void AppendMeta(List<TableColumnMeta> source, EbMobileControl control, IVendorDbTypes vDbTypes)
        {
            source.Add(new TableColumnMeta
            {
                Name = control.Name,
                Type = vDbTypes.GetVendorDbTypeStruct((EbDbTypes)control.EbDbType)
            });
        }

        private void AppendDefaultMeta(List<TableColumnMeta> metaList, IVendorDbTypes vDbTypes)
        {
            try
            {
                metaList.Add(new TableColumnMeta { Name = "eb_created_by", Type = vDbTypes.Decimal, Label = "Created By" });
                metaList.Add(new TableColumnMeta { Name = "eb_created_at", Type = vDbTypes.DateTime, Label = "Created At" });
                metaList.Add(new TableColumnMeta { Name = "eb_lastmodified_by", Type = vDbTypes.Decimal, Label = "Last Modified By" });
                metaList.Add(new TableColumnMeta { Name = "eb_lastmodified_at", Type = vDbTypes.DateTime, Label = "Last Modified At" });
                metaList.Add(new TableColumnMeta { Name = "eb_del", Type = vDbTypes.Boolean, Default = "F" });
                metaList.Add(new TableColumnMeta { Name = "eb_void", Type = vDbTypes.Boolean, Default = "F", Label = "Void ?" });
                metaList.Add(new TableColumnMeta { Name = "eb_loc_id", Type = vDbTypes.Int32, Label = "Location" });
                metaList.Add(new TableColumnMeta { Name = "eb_device_id", Type = vDbTypes.String, Label = "Device Id" });
                metaList.Add(new TableColumnMeta { Name = "eb_appversion", Type = vDbTypes.String, Label = "App Version" });
                metaList.Add(new TableColumnMeta { Name = "eb_created_at_device", Type = vDbTypes.DateTime, Label = "Sync Time" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileDashBoard : EbMobileContainer
    {
        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override string Name { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public List<EbMobileDashBoardControls> ChildControls { get; set; }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_mob_dashboard_container mob_container dropped' tabindex='1' eb-type='EbMobileDashBoard' id='@id'>
                        <div class='eb_mob_container_inner'>
                        </div>
                    </div>".RemoveCR().DoubleQuoted();
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobilePdf : EbMobileContainer
    {
        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override string Name { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [OSE_ObjectTypes(EbObjectTypes.iReport)]
        public string Template { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.ScriptEditorCS)]
        [HelpText("sql query to get data from offline database")]
        [Alias("Offline Query")]
        public EbScript OfflineQuery { set; get; }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_mob_pdf_container mob_container dropped' tabindex='1' eb-type='EbMobilePdf' id='@id'>
                        <div class='eb_mob_container_inner'>
                        </div>
                    </div>".RemoveCR().DoubleQuoted();
        }
    }
}
