﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    public enum enumPaging
    {
        numbers,
        simple,
        full,
        simple_numbers,
        full_numbers,
        first_last_numbers,
    }
    [ProtoBuf.ProtoContract]
    public class EbDataGridView : EbControlContainer
    {
        [ProtoBuf.ProtoMember(1)]
        public int DataSourceId { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public int PageSize { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public EbDataGridViewColumnCollection Columns { get; set; }

        [ProtoBuf.ProtoMember(4)]
        public bool HideCheckbox { get; set; }

        [ProtoBuf.ProtoMember(5)]
        public bool HideSerial { get; set; }

        [ProtoBuf.ProtoMember(6)]
        public int ScrollY { get; set; }

        [ProtoBuf.ProtoMember(7)]
        public enumPaging PagingType { get; set; }

        [ProtoBuf.ProtoMember(8)]
        public bool LoadOnDemand{ get; set; }

        [ProtoBuf.ProtoMember(9)]
        public int FilterDialogId { get; set; }


        public string GetCols()
        {
            return this.Columns.GetColumnDefJs(this.Name, this.HideSerial, this.HideCheckbox);
        }

        public string GetFilterControls()
        {
            List<string> _lsRet = new List<string>();

            if (this.Columns.SerialColumnAdded) _lsRet.Add("<th>&nbsp;</th>");
            if (this.Columns.CheckBoxColumnAdded) _lsRet.Add("<th>&nbsp;</th>");

            StringBuilder _ls = new StringBuilder();

            foreach (EbDataGridViewColumn column in this.Columns)
            {
                _ls.Clear();

                if (!column.Hidden)
                {
                    var span = string.Format("<span hidden>{0}</span>", column.Name);

                    string htext_class = string.Format("{0}_htext", this.Name);

                    string data_colum = string.Format("data-colum='{0}'", column.Name);
                    string data_table = string.Format("data-table='{0}'", this.Name);

                    string header_select = string.Format("{0}_{1}_hdr_sel", this.Name, column.Name);
                    string header_text1 = string.Format("{0}_{1}_hdr_txt1", this.Name, column.Name);
                    string header_text2 = string.Format("{0}_{1}_hdr_txt2", this.Name, column.Name);

                    _ls.Append("<th style='padding: 0px; margin: 0px'>");

                    if (column.ColumnType == EbDataGridViewColumnType.Numeric)
                        _ls.Append(span + getFilterForNumeric(header_text1, header_select, data_table, htext_class, data_colum, header_text2));
                    else if (column.ColumnType == EbDataGridViewColumnType.Text)
                        _ls.Append(span + getFilterForString(header_text1, header_select, data_table, htext_class, data_colum, header_text2));
                    else if (column.ColumnType == EbDataGridViewColumnType.DateTime)
                        _ls.Append(span + getFilterForDateTime(header_text1, header_select, data_table, htext_class, data_colum, header_text2));
                    else if (column.ColumnType == EbDataGridViewColumnType.Boolean)
                        _ls.Append(span + getFilterForBoolean(column.Name));
                    else
                        _ls.Append(span);

                    _ls.Append("</th>");
                }
                //else
                //    _ls.Append("<th style='display:none'></th>");

                _lsRet.Add(_ls.ToString());
            }
            if (this.Columns.EbVoidColumnAdded) _lsRet.Add("<th style='padding: 0px; margin: 0px; text-align:center;'>" + getFilterForBoolean("sys_cancelled")+ "</th>");
            if (this.Columns.EbLineGraphColumnAdded) _lsRet.Add("<th>&nbsp;</th>");
            if (this.Columns.EbLockColumnAdded) _lsRet.Add("<th style='padding: 0px; margin: 0px; text-align:center;'>" + getFilterForBoolean("sys_locked") + "</th>");
            //if (this.Columns.EbToggleColumnAdded) _lsRet.Add("<th style='padding: 0px; margin: 0px'>" + getFilterForBoolean("sys_deleted") + "</th>");
            _ls.Clear();
            _ls = null;

            return Newtonsoft.Json.JsonConvert.SerializeObject(_lsRet);
        }

        public string GetAggregateControls(int footer_id)
        {
            List<string> _ls = new List<string>();

            if (this.Columns.CheckBoxColumnAdded) _ls.Add("&nbsp;");
            if(this.Columns.SerialColumnAdded) _ls.Add("&nbsp;");
            foreach (EbDataGridViewColumn column in this.Columns)
            {
                if (!column.Hidden)
                {
                    var ext = column.ExtendedProperties as EbDataGridViewNumericColumnProperties;

                    string footer_select_id = string.Format("{0}_{1}_ftr_sel{2}", this.Name, column.Name, footer_id);
                    string fselect_class = string.Format("{0}_fselect", this.Name);

                    string data_colum = string.Format("data-column='{0}'", column.Name);
                    string data_table = string.Format("data-table='{0}'", this.Name);
                    string data_decip = string.Format("data-decip={0}", (ext != null) ? ext.DecimalPlaces : 0);

                    string footer_txt = string.Format("{0}_{1}_ftr_txt{2}", this.Name, column.Name, footer_id);

                    if (ext != null)
                    {
                        if (column.ColumnType == EbDataGridViewColumnType.Numeric)
                        {
                            if (ext.Sum || ext.Average)
                            {
                                _ls.Add(string.Format(@"
<div class='input-group'>
    <div class='input-group-btn'>
        <button type='button' class='btn btn-default dropdown-toggle' data-toggle='dropdown' id='{1}'>&sum;</button>
        <ul class='dropdown-menu'>
          <li ><a href ='#' onclick='fselect_func({6}, this,{5});' data-sum='Sum' {2} {3} {4}>&sum;</a></li>
          <li><a href ='#' onclick='fselect_func({6}, this,{5});' {2} {3} {4}>&mnplus;</a></li>
        </ul>
    </div>
    <input type='text' class='form-control' id='{0}' disabled style='text-align: right;'>
</div>", footer_txt, footer_select_id, data_table, data_colum, data_decip, this.ScrollY, this.Name));
                            }
                            else
                                _ls.Add("&nbsp;");
                        }
                        else
                            _ls.Add("&nbsp;");
                    }
                    else
                        _ls.Add("&nbsp;");
            }
                //else
                //    _ls.Add("ttttttt");
        }

            if (this.Columns.EbVoidColumnAdded) _ls.Add("&nbsp;");
            if (this.Columns.EbLineGraphColumnAdded) _ls.Add("&nbsp;");
            if (this.Columns.EbLockColumnAdded) _ls.Add("&nbsp;");
           // if (this.Columns.EbToggleColumnAdded) _ls.Add("&nbsp;");
            return Newtonsoft.Json.JsonConvert.SerializeObject(_ls);
        }
        
        public string GetAggregateInfo()
        {
            List<AggregateInfo> _ls = new List<AggregateInfo>();

            foreach (EbDataGridViewColumn column in this.Columns)
            {
                if (!column.Hidden)
                {
                    var ext = column.ExtendedProperties as EbDataGridViewNumericColumnProperties;

                    if (ext != null)
                    {
                        if (column.ColumnType == EbDataGridViewColumnType.Numeric && (ext.Sum || ext.Average))
                            _ls.Add(new AggregateInfo { colname = column.Name, coltype = "N", deci_val = ext.DecimalPlaces });
                    }
                }
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(_ls);
        }

        public EbDataGridView()
        {
            this.Columns = new EbDataGridViewColumnCollection();
            this.Columns.CollectionChanged += Columns_CollectionChanged;
        }

        public delegate void ColumnsChangedHandler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e);
        public event ColumnsChangedHandler ColumnsChanged;
        private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ColumnsChanged != null)
                ColumnsChanged(sender, e);
        }

        //[[100, 500, 1000, 2500, 5000, -1], [100, 500, 1000, 2500, 5000, 'All']]
        private string GetLengthMenu()
        {
            string sLengthMenu = "paging: false";

            if (this.PageSize > 0)
            {
                int[] ia = new int[10];
                for (int i = 0; i < 10; i++)
                    ia[i] = (this.PageSize * (i + 1));

                sLengthMenu = "lengthMenu: " + string.Format("[[{0}, -1], [{0}, 'All']]", string.Join(", ", ia));
            }

            return sLengthMenu;
        }

        private string GetScrollYOption()
        {
            return (this.ScrollY > 0) ? string.Format("scrollY: '{0}', scrollX: true ", this.ScrollY) : "fixedHeader: { footer: true }";
        }

        private string GetSelectOption()
        {
            string __select = string.Empty;
            if (!this.HideCheckbox && !this.HideSerial)
                __select = "select: { style: 'os', selector: 'td:nth-child(1),td:nth-child(2)'}";
            else if(!this.HideSerial || !this.HideCheckbox)
                __select = "select: { style: 'os', selector: 'td:nth-child(1)'}";
            else 
                __select = "select: false";
            return __select;
        }

        public string GetFooter()
        {
            string ftr = string.Empty;
            ftr = "<tfoot>";
            if(this.Columns.CheckBoxColumnAdded)
                ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";
            if(this.Columns.SerialColumnAdded)
                ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";
           
            foreach (EbDataGridViewColumn colum in this.Columns)
            {
                if (!colum.Hidden)
                    ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";
                else
                    ftr += "<th style=\"display:none;\"></th>";
            }
            if (this.Columns.EbVoidColumnAdded)
                ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";
            if (this.Columns.EbLineGraphColumnAdded)
                ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";
            if (this.Columns.EbLockColumnAdded)
                ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";
            //if (this.Columns.EbToggleColumnAdded)
            //    ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";

            ftr += "<tr>";
            if (this.Columns.CheckBoxColumnAdded)
                ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";
            if (this.Columns.SerialColumnAdded)
                ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";

            foreach (EbDataGridViewColumn colum in this.Columns)
            {
                if (!colum.Hidden)
                    ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";
                else
                    ftr += "<th style=\"display:none;\"></th>";
            }
            if (this.Columns.EbVoidColumnAdded)
                ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";
            if (this.Columns.EbLineGraphColumnAdded)
                ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";
            if (this.Columns.EbLockColumnAdded)
                ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";
            //if (this.Columns.EbToggleColumnAdded)
            //    ftr += "<th style=\"padding: 0px; margin: 0px\"></th>";
            ftr += "</tr>";
            ftr += "</tfoot>";
            return ftr;
        }

        public string getFilterForNumeric( string header_text1,string header_select, string data_table,string htext_class,string data_colum,string header_text2)
        {
            string coltype = "data-coltyp='numeric'";
            string drptext = string.Empty;
            drptext = string.Format(@"
<div class='input-group'>
    <div class='input-group-btn'>
        <button type='button' class='btn btn-default dropdown-toggle' data-toggle='dropdown' id='{4}'> = </button>
        <ul class='dropdown-menu'>
          <li ><a href ='#' onclick='setLiValue(this);' {1} {3}>=</a></li>
          <li><a href ='#' onclick='setLiValue(this);' {1} {3}><</a></li>
          <li><a href='#' onclick='setLiValue(this);' {1} {3}>></a></li>
          <li><a href='#' onclick='setLiValue(this);' {1} {3}><=</a></li>
          <li><a href='#' onclick='setLiValue(this);' {1} {3}>>=</a></li>
          <li ><a href='#' onclick='setLiValue(this);' {1} {3}>B</a></li>
        </ul>
    </div>
    <input type='number' class='form-control {2}' id='{0}' onkeypress='call_filter(event, this);' {1}  {3} {6}>
    <span class='input-group-btn'></span>
    <input type='number' class='form-control {2}' id='{5}' style='visibility: hidden' onkeypress='call_filter(event, this);' {1}  {3} {6}>
</div> ", header_text1, data_table, htext_class, data_colum, header_select, header_text2, coltype)
;
            return drptext;
        }

        public string getFilterForDateTime(string header_text1, string header_select, string data_table, string htext_class, string data_colum, string header_text2)
        {
            string coltype = "data-coltyp='date'";
            string filter = string.Format(@"
<div class='input-group'>
    <div class='input-group-btn'>
        <button type='button' class='btn btn-default dropdown-toggle' data-toggle='dropdown' id='{4}'> = </button>
        <ul class='dropdown-menu'>
          <li ><a href ='#' onclick='setLiValue(this);' {1} {3}>=</a></li>
          <li><a href ='#' onclick='setLiValue(this);' {1} {3}><</a></li>
          <li><a href='#' onclick='setLiValue(this);' {1} {3}>></a></li>
          <li><a href='#' onclick='setLiValue(this);' {1} {3}><=</a></li>
          <li><a href='#' onclick='setLiValue(this);' {1} {3}>>=</a></li>
          <li ><a href='#' onclick='setLiValue(this);' {1} {3}>B</a></li>
        </ul>
    </div>
    <input type='date' class='form-control {2}' id='{0}' onkeypress='call_filter(event, this);' {1}  {3} {6}>
    <span class='input-group-btn'></span>
    <input type='date' class='form-control {2}' id='{5}' style='visibility: hidden' onkeypress='call_filter(event, this);' {1}  {3} {6}>
</div> ", header_text1, data_table, htext_class, data_colum, header_select, header_text2, coltype);
            return filter;
        }

        public string getFilterForString(string header_text1, string header_select, string data_table, string htext_class, string data_colum, string header_text2)
        {
            string drptext = string.Empty;
            drptext = string.Format(@"
<div class='input-group'>
    <div class='input-group-btn'>
        <button type='button' class='btn btn-default dropdown-toggle' data-toggle='dropdown' id='{4}'>x*</button>
        <ul class='dropdown-menu'>
          <li ><a href ='#' onclick='setLiValue(this);' {1} {3}>x*</a></li>
          <li><a href ='#' onclick='setLiValue(this);' {1} {3}>*x</a></li>
          <li><a href='#' onclick='setLiValue(this);' {1} {3}>*x*</a></li>
          <li><a href='#' onclick='setLiValue(this);' {1} {3}>=</a></li>
        </ul>
    </div>
    <input type='text' class='form-control {2}' id='{0}' onkeypress='call_filter(event, this);' {1}  {3}>
</div> ", header_text1, data_table, htext_class, data_colum, header_select, header_text2)
;
            return drptext;
        }

        public string getFilterForBoolean(string colum)
        {
            var filter = string.Empty;
            string id = this.Name+"_"+ colum + "_hdr_txt1";
            string cls = this.Name + "_hchk";
            filter = string.Format("<input type='checkbox' id='{1}' data-colum='{0}' onchange='toggleInFilter(this);' data-coltyp='boolean' data-table='{2}' class='{3} {2}_htext'>", colum, id, this.Name, cls);
            return filter;
        }

        public override string GetHead()
        {
             return @"$('[data-toggle=\'tooltip\']').tooltip(); ";
        }

        private int FilterBH = 0;

        private EbForm __filterForm;
        public void SetFilterForm(EbForm filterForm)
        {
            this.__filterForm = filterForm;
        }

        private string filters
        {
            get
            {
                string rs = "";
                int max=0;
                if(this.__filterForm != null)
                {
                    rs = @"<div class='collapse collapse in' style='margin-top:10px;' id='filterBox'>
                                <div class='well well-sm' style='position:relative; height:@FilterBHpx; padding-top:40px;padding-bottom:40px;'>";
                    foreach (EbControl c in this.__filterForm.Controls)
                    {
                        if (c.Top >= max)
                        {
                            max = (c.Top + c.Height);
                        }
                        c.Top += 10;
                        rs += c.GetHtml();
                    }
                    this.FilterBH += max;
                    rs += @"</div></div>";
                }
                
                return rs;
            }
        }

        public override string GetHtml()
        {
            return @"
<style>



.tablecontainer {
    width:100%;
    height:auto;
    display:inline-block;
    padding:1px;
}
.loadingdiv {
    vertical-align:middle;
    margin: 5% 50%;
    display: none;
}
.numericcol{
    float:right;
}
.toolbar {
    float:left;
}

#@tableId th.resizing {
    cursor: e-resize;
}

td.resizer {
  position: absolute;
  top: 0;
  right: -8px;
  bottom: 0;
  left: auto;
  width: 16px;    
  cursor: e-resize;   
    background-color:red;    
}

.dataTables_scrollHead {
padding-bottom: 250px; margin-bottom: -250px;
}
.dataTables_scrollFoot{
padding-bottom: 250px; margin-bottom: -250px;
}
.linepadding{
padding:0px!important;
}
td.dt-center { text-align: center; }
th.dt-center { text-align: right; }
td.dt-body-right { text-align: right; }
.dt-buttons {visibility:hidden;}
th { font-size: 14px; }
td { font-size: 12px; }
.progress {
    margin-bottom: 0px !important;
}

</style>
<div class='tablecontainer' id='@tableId_container'>
      <div>
        <div class='btn-group' id='@tableId_filterdiv'>
            <a class='btn btn-default' onclick='showOrHideFilter(this,@scrolly);' name='filterbtn' style='display: none;' data-table='@tableId' data-toggle='tooltip' title='On\/Off Filter'><i class='fa fa-filter' aria-hidden='true'></i></a>
            <button type='button' class='btn btn-default dropdown-toggle' data-toggle='dropdown' name='filterbtn' style='display: none;'>
            <span class='caret'></span>  <!-- caret --></button>
            <ul class='dropdown-menu' role='menu'>
                <li><a href = '#' onclick= clearFilter('@tableId')> Clear Filter</a></li>
            </ul>
        </div>
    <button type='button' id='@tableId_btntotalpage' class='btn btn-default' style='display: none;' onClick='showOrHideAggrControl(this,@scrolly);' data-table='@tableId'>&sum;</button>
        <div id='btnGo' class='btn btn-default' >GO</div>
        <div id='@tableId_fileBtns' style='display: inline-block;'>
            <div id='btnCopy' class='btn btn-default'  name='filebtn' style='display: none;' data-toggle='tooltip' title='Copy to Clipboard' onclick= CopyToClipboard('@tableId') ><i class='fa fa-clipboard' aria-hidden='true'></i></div>
            <div class='btn-group'>
                <div id='btnPrint' class='btn btn-default'  name='filebtn' style='display: none;'  data-toggle='tooltip' title='Print' onclick= ExportToPrint('@tableId')><i class='fa fa-print' aria-hidden='true'></i></div>
                    <div class='btn btn-default dropdown-toggle' data-toggle='dropdown' name='filebtn' style='display: none;'>
                        <span class='caret'></span>  <!-- caret --></div>
                        <ul class='dropdown-menu' role='menu'>
                            <li><a href = '#' onclick= printAll('@tableId')> Print All</a></li>
                            <li><a href = '#' onclick= printSelected('@tableId')> Print Selected</a></li>
                        </ul>
            </div>
            <div id='btnExcel' class='btn btn-default'  name='filebtn' style='display: none;' data-toggle='tooltip' title='Excel' onclick= ExportToExcel('@tableId')><i class='fa fa-file-excel-o' aria-hidden='true'></i></div>
            <div id='btnPdf' class='btn btn-default'    name='filebtn' style='display: none;'  data-toggle='tooltip' title='Pdf' onclick= ExportToPdf('@tableId')><i class='fa fa-file-pdf-o' aria-hidden='true'></i></div>
            <div id='btnCsv' class='btn btn-default'    name='filebtn' style='display: none;' data-toggle='tooltip' title='Csv' onclick= ExportToCsv('@tableId')><i class='fa fa-file-text-o' aria-hidden='true'></i></div>
           </div>
            @collapsBtn
    </div>
    <div style='width:auto;'>
        @filters  
         <h3>@tableViewName</h3>
        <div id='@tableId_loadingdiv' class='loadingdiv'>
            <img id='@tableId_loading-image' src='/images/ajax-loader.gif' alt='Loading...' />
        </div>
               
        <table id='@tableId' class='table table-striped table-bordered'></table>
    </div>
</div>
   <!-- Modal for Graph-->
  <div class='modal fade' id='graphmodal' role='dialog'>
    <div class='modal-dialog modal-lg'>
    
      <!-- Modal content-->
      <div class='modal-content'>
        <div class='modal-header'>
          <button type = 'button' class='close' data-dismiss='modal'>&times;</button>
          <h4 class='modal-title'><center>Graph</center></h4>
        </div>
        <div class='modal-body'>
            <div id='$$$$$$$_canvasDiv' class='dygraph-Wrapper'>
                <div id='graphdiv' style='width:100%;height:500px;'></div>
            </div>  
        </div>
     </div>
    </div>
 </div>

<!-- Modal for Settings-->
  <div class='modal fade' id='settingsmodal' role='dialog'>
    <div class='modal-dialog modal-lg'>
    
      <!-- Modal content-->
      <div class='modal-content'>
        <div class='modal-header'>
          <button type = 'button' class='close' data-dismiss='modal'>&times;</button>
          <h4 class='modal-title'><center>Settings</center></h4>
        </div>
        <div class='modal-body'>
            
        </div>
     </div>
    </div>
 </div>

<script>
var flag=true;
var DtF = true;
var @tableId;
$('#btnGo').click(function(){
    if(DtF){
        DtF = false;
        initTable_@tableId();
        $('#filterBox').collapse('hide');
    }
    else
        @tableId.ajax.reload();
});


function initTable_@tableId(){

    $('#@tableId_loadingdiv').show();
    $('#@tableId').append( $('@tfoot') );

    $.get('@servicestack_url/ds/columns/@dataSourceId?format=json&Token=' + getToken() + '&Params=' + encodeURIComponent(JSON.stringify(getFilterValues())), { crossDomain: 'true' }, function (data){
        var @tableId_ids=[];
        var @tableId_order_col = '';
        var @tableId_order_dir = 0;
        var @tableId__datacolumns = data.columns;
        @tableId= $('#@tableId').DataTable(
        {
            dom:'<\'col-sm-2\'l><\'col-sm-2\'i><\'col-sm-4\'B><\'col-sm-4\'p>tr',
            buttons: ['copy', 'csv', 'excel', 'pdf','print', {
                            extend: 'print',
                            exportOptions: {
                                modifier: {
                                    selected: true
                                }}}],
            @scrollYOption,
            responsive:true,
            keys: true,
            autoWidth: false,
            @lengthMenu,
            serverSide: true,
            processing:true,
            language: { processing: '<div class=\'fa fa-spinner fa-pulse  fa-3x fa-fw\'></div>',
                        info:'_START_ - _END_ / _TOTAL_'},
            pagingType:'@pagingType',
            columns:@columnsRender, 
            order: [],
            deferRender: true,
            filter: true,
            //@selectOption,
            retrieve: true,
            ajax: {
                url: '@servicestack_url/ds/data/@dataSourceId',
                type: 'POST',
                data: function(dq) { 
                    delete dq.columns; delete dq.order; delete dq.search;
                    dq.Id = @dataSourceId;
                    dq.Token = getToken();
                    dq.TFilters = JSON.stringify(repopulate_filter_arr('@tableId'));
                    dq.Params = JSON.stringify(getFilterValues());
                    dq.OrderByCol = @tableId_order_col; 
                    dq.OrderByDir = @tableId_order_dir;
                },
                dataSrc: function(dd) {
                        return dd.data;
                }
            },
        
            fnRowCallback: function( nRow, aData, iDisplayIndex, iDisplayIndexFull ) {
                colorRow(nRow, aData, iDisplayIndex, iDisplayIndexFull, data.columns);
            },

            fnFooterCallback: function ( nRow, aaData, iStart, iEnd, aiDisplay ) {
                summarize2(@tableId, '@tableId', @eb_agginfo,@scrolly);
            },
            drawCallback:function ( settings ) {
                $('tbody [data-toggle=toggle]').bootstrapToggle();
                @tableId.columns.adjust();
            },
            initComplete:function ( settings,json ) {
                $('thead:eq(0) tr:eq(1) [type=checkbox]').prop('indeterminate',true); 
                @tableId.columns.adjust();
            }
            //drawCallback: function ( settings ) {
            //    var api = this.api();
            //    var rows = api.rows( { page: 'current'} ).nodes();
            //    var last = null;
            
            //    api.column(3, { page: 'current'} ).data().each(function(group, i) {
            //        if (last !== group)
            //        {
            //            $(rows).eq(i).before(
            //                '<tr class=\'group\'><td colspan=\'8\'>' + group + '</td></tr>'
            //            );

            //            last = group;
            //        }
            //    } );
            //}
        });

        $.fn.dataTable.ext.errMode = 'throw';

        jQuery.fn.dataTable.Api.register( 'sum()', function ( ) {
            return this.flatten().reduce( function ( a, b ) {
                if ( typeof a === 'string' ) {
                    a = a.replace(/[^\d.-]/g, '') * 1;
                }
                if ( typeof b === 'string' ) {
                    b = b.replace(/[^\d.-]/g, '') * 1;
                }
 
                return a + b;
            }, 0 );
        } );

        jQuery.fn.dataTable.Api.register( 'average()', function () {
            var data = this.flatten();
            var sum = data.reduce( function ( a, b ) {
                return (a*1) + (b*1); // cast values in-case they are strings
            }, 0 );
  
            return sum / data.length;
        });

        if( @eb_agginfo.length>0 ) {
            createFooter('@tableId', @eb_footer1, @scrolly, 0);
            createFooter('@tableId', @eb_footer2, @scrolly, 1);
        }

        $('#@tableId_loadingdiv').hide();
        
        $('#@tableId_fileBtns [name=filebtn]').css('display', 'inline-block');
        $('#@tableId_filterdiv [name=filterbtn]').css('display', 'inline-block');
   
        createFilterRowHeader('@tableId', @eb_filter_controls, @scrolly);

        $('#@tableId_container thead').on('click','th',function(){
            var col = $(this).children('span').text();
            var dir = $(this).attr('aria-sort');
            if(col !== '') {
                @tableId_order_col = col;
                @tableId_order_dir = (dir === 'undefined') ? 1 : ((dir === 'ascending') ? 2 : 1);
            }
        });

        if(@bserial){
            @tableId.on( 'draw.dt', function () {
                @tableId.column(0).nodes().each( function (cell, i) {
                    cell.innerHTML = i+1;
                } );
            } );
        }

    });
        
    new ResizeSensor(jQuery('#@tableId_container'), function() {
        if ( $.fn.dataTable.isDataTable( '#@tableId' ) )
            @tableId.columns.adjust();
    });
}    
</script>"
.Replace("@dataSourceId", this.DataSourceId.ToString().Trim())
.Replace("@tableId", this.Name)
.Replace("@tableViewName", ((string.IsNullOrEmpty(this.Label)) ? "&lt;ReportLabel Undefined&gt;" : this.Label))
.Replace("@lengthMenu", this.GetLengthMenu())
.Replace("@columnsRender", this.GetCols())
.Replace("@eb_filter_controls", this.GetFilterControls())
.Replace("@eb_footer1", this.GetAggregateControls(1))
.Replace("@eb_footer2", this.GetAggregateControls(2))
.Replace("@eb_agginfo", this.GetAggregateInfo())
.Replace("@bserial", this.Columns.SerialColumnAdded.ToString().ToLower())
.Replace("@scrolly", this.ScrollY.ToString())
.Replace("@scrollYOption", this.GetScrollYOption())
.Replace("@tfoot", this.GetFooter())
.Replace("@pagingType", this.PagingType.ToString())
.Replace("@servicestack_url", "https://expressbaseservicestack.azurewebsites.net")
.Replace("@filters", this.filters)
.Replace("@FilterBH", this.FilterBH.ToString())
.Replace("@collapsBtn",(this.__filterForm!=null) ?  @"<div id = 'btnCollapse' class='btn btn-default' data-toggle='collapse' data-target='#filterBox' aria-expanded='true' aria-controls='filterBox'>
                    <i class='fa fa-chevron-down' aria-hidden='true'></i>
                </div>" : string.Empty )
.Replace("@selectOption", this.GetSelectOption());
        }
    }

    public class AggregateInfo
    {
        public string colname { get; set; }
        public string coltype { get; set; }
        public int deci_val{ get; set;}
    }

    [ProtoBuf.ProtoContract]
    public class EbDataGridViewColumn : EbControl
    {
        private EbDataGridViewColumnType _columnType = EbDataGridViewColumnType.Null;

        [ProtoBuf.ProtoMember(1)]
        public EbDataGridViewColumnType ColumnType
        {
            get { return _columnType; }
            set
            {
                if (value == EbDataGridViewColumnType.Numeric)
                    this.ExtendedProperties = new EbDataGridViewNumericColumnProperties();
                else if (value == EbDataGridViewColumnType.DateTime)
                    this.ExtendedProperties = new EbDataGridViewDateTimeColumnProperties();
                else if (value == EbDataGridViewColumnType.Boolean)
                    this.ExtendedProperties = new EbDataGridViewBooleanColumnProperties();
                else if(value == EbDataGridViewColumnType.Text)
                    this.ExtendedProperties = new EbDataGridViewColumnProperties();

                _columnType = value;
            }
        }

        [ProtoBuf.ProtoMember(2)]
#if NET462
        [TypeConverter(typeof(ExpandableObjectConverter))]
#endif
        public EbDataGridViewColumnProperties ExtendedProperties { get; set; }

        public EbDataGridViewColumn()
        {
            this.Width = 100;
        }

        public string GetColumnDefJs()
        {
            string script = "{";
            script += "data: " + "(_.find(data.columns, {'columnName': '{0}'})).columnIndex".Replace("{0}", this.Name);
            script += string.Format(",title: '{0}<span hidden>{1}</span>'", (this.Label != null) ? this.Label : this.Name, this.Name);
            script += ",className: '" + this.GetClassName() + "'";
            script += ",visible: " + (!this.Hidden).ToString().ToLower();
            script += ",width: " + this.Width.ToString();
            script += ",render: " + this.GetRenderFunc();
            script += ",name: '" + this.Name + "'";
            script += "},";

            return script;
        }

        private string GetClassName()
        {
            string _c = string.Empty;

            if (this.ColumnType == EbDataGridViewColumnType.Boolean)
                _c = "dt-body-center";
            else if (this.ColumnType == EbDataGridViewColumnType.Numeric)
                _c = "dt-body-right";
            else
                _c = "dt-body-left";

            return _c;
        }

        private string GetRenderFunc(){
            string _r = string.Empty;
            string _fwrapper = "function( data, type, row, meta ) { {0} }";

            if (this.ColumnType == EbDataGridViewColumnType.Numeric)
            {
                var ext = this.ExtendedProperties as EbDataGridViewNumericColumnProperties;

                if (ext.ShowProgressbar)
                    _r = "return renderProgressCol(data);";
                else
                {
                    if (ext != null)
                    {
                        var deci_places = (ext.DecimalPlaces > 0) ? ext.DecimalPlaces : 2;

                        if (!ext.Localize)
                            _r = string.Format("return parseFloat(data).toFixed({0});", deci_places);
                        else
                        {
                            if (!ext.IsCurrency)
                                _r = "return parseFloat(data).toLocaleString('en-US', { maximumSignificantDigits: {0} });".Replace("{0}", deci_places.ToString());
                            else
                                _r = "return parseFloat(data).toLocaleString('en-US', { style: 'currency', currency: 'USD', maximumSignificantDigits: {0} });".Replace("{0}", deci_places.ToString());
                        }
                    }
                    else
                        _r = "return data;";
                }

                _r = _fwrapper.Replace("{0}", _r);
            }
            else if (this.ColumnType == EbDataGridViewColumnType.DateTime)
                _r = _fwrapper.Replace("{0}", "return moment.unix(data).format('MM/DD/YYYY');");
            else if (this.ColumnType == EbDataGridViewColumnType.Null)
            {
                var ext = this.ExtendedProperties as EbDataGridViewBooleanColumnProperties;
                _r = _fwrapper.Replace("{0}", "return renderToggleCol(data,@ext);".Replace("@ext",ext.IsEditable.ToString().ToLower()));
            }
            else if (this.ColumnType == EbDataGridViewColumnType.Chart)
                _r = _fwrapper.Replace("{0}", "return lineGraphDiv(data);");
            else
                _r = _fwrapper.Replace("{0}", "return data;");

            return _r;
        }
    }

    [ProtoBuf.ProtoContract]
    [ProtoBuf.ProtoInclude(1, typeof(EbDataGridViewNumericColumnProperties))]
    [ProtoBuf.ProtoInclude(2, typeof(EbDataGridViewDateTimeColumnProperties))]
    [ProtoBuf.ProtoInclude(3, typeof(EbDataGridViewBooleanColumnProperties))]
    public class EbDataGridViewColumnProperties
    {

    }

    [ProtoBuf.ProtoContract]
    public class EbDataGridViewBooleanColumnProperties : EbDataGridViewColumnProperties
    {
        [ProtoBuf.ProtoMember(1)]
        public bool IsEditable { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class EbDataGridViewNumericColumnProperties : EbDataGridViewColumnProperties
    {
        [ProtoBuf.ProtoMember(1)]
        public int DecimalPlaces { get; set; }

        [ProtoBuf.ProtoMember(2)]
        [Description("Comma/delimeter separated localized display of number/value.")]
        public bool Localize { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public bool IsCurrency { get; set; }

        [ProtoBuf.ProtoMember(4)]
        public bool Sum { get; set; }

        [ProtoBuf.ProtoMember(5)]
        public bool Average { get; set; }

        [ProtoBuf.ProtoMember(6)]
        public bool Max { get; set; }

        [ProtoBuf.ProtoMember(7)]
        public bool Min { get; set; }

        [ProtoBuf.ProtoMember(8)]
        public bool ShowProgressbar { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class EbDataGridViewDateTimeColumnProperties : EbDataGridViewColumnProperties
    {
    }

    [ProtoBuf.ProtoContract]
    public class EbDataGridViewColumnCollection : ObservableCollection<EbDataGridViewColumn>
    {
        internal bool SerialColumnAdded { get; set; }
        internal bool CheckBoxColumnAdded { get; set; }
        internal bool EbVoidColumnAdded { get; set; }
        internal bool EbLineGraphColumnAdded { get; set; }
        internal bool EbLockColumnAdded { get; set; }
        internal bool EbToggleColumnAdded { get; set; }

        internal int ActualCount
        {
            get
            {
                return this.Count + (SerialColumnAdded ? 1 : 0) + (CheckBoxColumnAdded ? 1 : 0) + (EbVoidColumnAdded? 1:0) + (this.EbLineGraphColumnAdded ? 1: 0);
            }
        }

        public EbDataGridViewColumn this[string columnName]
        {
            get
            {
                foreach (EbDataGridViewColumn col in this)
                {
                    if (col.Name == columnName)
                        return col;
                }

                return null;
            }
        }

        public bool Contains(string columnName)
        {
            foreach (EbDataGridViewColumn col in this)
            {
                if (col.Name == columnName)
                    return true;
            }

            return false;
        }

        internal string GetColumnDefJs(string tableid, bool bHideSerial, bool bHideCheckBox)
        {
            string script = "[";

            if (!bHideSerial)
                script += this.GetSerialColumnDefJs();

            if (!bHideCheckBox)
                script += this.GetCheckBoxColumnDefJs(tableid);

            foreach (EbDataGridViewColumn column in this)
            {
                //if (column.Name == "data_graph")
                //    script += GetLineGraphColumnDefJs(column);
                //else
                    script += column.GetColumnDefJs();
            }

            if (!this.Contains("sys_cancelled"))//change to eb_void
            {
                script += GetEbVoidColumnDefJs();
            }
            
            if (!this.Contains("sys_locked"))//change to eb_lock
            {
                script += GetEbLockColumnDefJs();
            }

            //if (!this.Contains("sys_deleted"))//change to eb_lock
            //{
            //    script += GetEbToggleColumnDefJs();
            //}
            return script + "]";
        }

        private string GetSerialColumnDefJs()
        {
            this.SerialColumnAdded = true;
            return "{ width:10, searchable: false, orderable: false ,targets: 0 },";
        }

        private string GetCheckBoxColumnDefJs(string tableid)
        {
            // className:'select-checkbox',
            this.CheckBoxColumnAdded = true;
            return "{ data: null, title: \"<input id='{0}_select-all' type='checkbox' onclick='clickAlSlct(event, this);' data-table='{0}'/>\"".Replace("{0}", tableid)
                + ", width: 10, render: function( data2, type, row, meta ) { return renderCheckBoxCol({0}, data.columns, '{0}', row,meta); }, orderable: false },".Replace("{0}", tableid);
        }

        private string GetEbVoidColumnDefJs()
        {
            //data: (_.find(data.columns, {'columnName': 'sys_cancelled'})).columnIndex,
            this.EbVoidColumnAdded = true;
            return "{data: (_.find(data.columns, {'columnName': 'sys_cancelled'})).columnIndex, title: \"<i class='fa fa-ban fa-1x' aria-hidden='true'></i><span hidden>sys_cancelled</span>\" "
             + ", width: 10 , className:'dt-center', render: function( data2, type, row, meta ) { return renderEbVoidCol(data2); } },";
            
        }

        private string GetLineGraphColumnDefJs(EbDataGridViewColumn column)//edit
        {
            this.EbLineGraphColumnAdded = true;
            string script = "{ data: " + "(_.find(data.columns, {'columnName': '{0}'})).columnIndex".Replace("{0}", column.Name);
            return script + @", width: 30, render: function(data2, type, row, meta) { return lineGraphDiv(data.columns, data2, meta, '" + column.Name + "'); }, orderable: true, className:'linepadding' }, ";
        }

        private string GetEbLockColumnDefJs()
        {
            this.EbLockColumnAdded = true;
            return "{ data: (_.find(data.columns, {'columnName': 'sys_locked'})).columnIndex, title: \"<i class='fa fa-lock fa-1x' aria-hidden='true' ></i><span hidden>sys_locked</span>\" "
                + ", width: 10, className:'dt-center', render: function( data2, type, row, meta ) { return renderLockCol(data2); } },";
        }

        private string GetEbToggleColumnDefJs()
        {
            //data: (_.find(data.columns, {'columnName': 'sys_deleted'})).columnIndex,
            this.EbToggleColumnAdded = true;
            return "{data: (_.find(data.columns, {'columnName': 'sys_deleted'})).columnIndex, title:\"sys_deleted<span hidden>sys_deleted</span>\", width: 10, render: function( data2, type, row, meta ) { return renderToggleCol(data2); } },";
        }
    }
}
