﻿using System.Collections.Generic;
using System.ComponentModel;

namespace ExpressBase.UI
{
    [ProtoBuf.ProtoContract]
    public enum EbObjectType
    {
        Form,
        View,
        DataSource,
    }

    [ProtoBuf.ProtoContract]
    [ProtoBuf.ProtoInclude(1000, typeof(EbControl))]
    [ProtoBuf.ProtoInclude(1001, typeof(EbDataSource))]
    public class EbObject
    {
        [Browsable(false)]
        public int Id { get; set; }

        [ProtoBuf.ProtoMember(1)]
        [Browsable(false)]
        public EbObjectType EbObjectType { get; set; }

        [ProtoBuf.ProtoMember(2)]
        [Browsable(false)]
        public string TargetType { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public virtual string Name { get; set; }

        public EbObject() { }
    }

    [ProtoBuf.ProtoContract]
    [ProtoBuf.ProtoInclude(2000, typeof(EbButton))]
    [ProtoBuf.ProtoInclude(2001, typeof(EbTableLayout))]
    [ProtoBuf.ProtoInclude(2002, typeof(EbChart))]
    [ProtoBuf.ProtoInclude(2003, typeof(EbDataGridView))]
    public class EbControl : EbObject
    {
        [ProtoBuf.ProtoMember(4)]
        public List<EbControl> Controls { get; set; }

        [ProtoBuf.ProtoMember(5)]
        public virtual string Label { get; set; }

        [ProtoBuf.ProtoMember(6)]
        public virtual string HelpText { get; set; }

        [ProtoBuf.ProtoMember(7)]
        public virtual string ToolTipText { get; set; }

        [ProtoBuf.ProtoMember(8)]
        public virtual int CellPositionRow { get; set; }

        [ProtoBuf.ProtoMember(9)]
        public virtual int CellPositionColumn { get; set; }

        public EbControl() { }

        public virtual string GetHtml() { return string.Empty; }
    }

    [ProtoBuf.ProtoContract]
    public class EbForm : EbControl
    {
        [Browsable(false)]
        public override int CellPositionRow { get; set; }

        [Browsable(false)]
        public override int CellPositionColumn { get; set; }

        public EbForm() { }
    }

    [ProtoBuf.ProtoContract]
    public class EbButton : EbControl
    {
        public EbButton() { }
    }

    [ProtoBuf.ProtoContract]
    public class EbTableLayout : EbControl
    {
        [ProtoBuf.ProtoMember(1)]
        public int RowCount { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public int ColumnCount { get; set; }

        public EbTableLayout() { }

        public override string GetHtml()
        {
            string html = GetTable(ColumnCount, RowCount);

            if (base.Controls != null)
            {
                foreach (EbControl ec in base.Controls)
                    html = html.Replace(string.Format("{0}_{1}_{2}", this.Name, ec.CellPositionColumn, ec.CellPositionRow), ec.GetHtml());
            }

            return html;
        }

        private string GetTable(int col, int row)
        {
            HtmlTable ht = new HtmlTable(this.Name);

            for (int r = 0; r < row; r++)
            {
                HtmlRow hr = new HtmlRow(this.Name, r);

                for (int c = 0; c < col; c++)
                    hr.Cells.Add(new HtmlCell(this.Name, c, r));

                ht.Rows.Add(hr);
            }

            return ht.GetHtml();
        }
    }

    [ProtoBuf.ProtoContract]
    public class EbChart : EbControl
    {
        [ProtoBuf.ProtoMember(1)]
        public string ChartType { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public int DataSourceId { get; set; }

        public EbChart() { }

        public override string GetHtml()
        {
            return @"
<div style='margin-top: 25px; margin-bottom: 25px; margin-right: 25px; margin-left: 25px;'>
<!--    <div id='loadingdiv' style='height: auto; width: auto; display: none;'>
        <img id='loading-image' src='/images/ajax-loader.gif' alt='Loading...' />
    </div> -->
    <div>
        <select id='ctype'>
            <option value='line'>Line</option>
            <option value='line'>Pie</option>
            <option value='line'>Doughnut</option>
        </select>
    </div>
    <canvas id='$$$$$$$'></canvas>
</div>
<style>
#loadingdiv {
   width: 100%;
   height: 100%;
   top: 0;
   left: 0;
   position: fixed;
   display: block;
   opacity: 0.7;
   background-color: #fff;
   z-index: 99;
   text-align: center;
}

#loading-image {
  position: absolute;
  top: 100px;
  left: 240px;
  z-index: 100;
}
</style>
<script>
//$('#loadingdiv').show();
$.get('/ds/data/#######?format=json', function(data) 
{
    var Ydatapoints = [];
    var Xdatapoints = [];
    $.each(data.data, function(i, value) {
        Xdatapoints.push(value[1]);
        Ydatapoints.push(value[2]);
    });
    var ctx = document.getElementById('$$$$$$$');
    Chart.defaults.global.hover.mode = 'nearest';
    var myChart = new Chart(ctx, {
        type: '@@@@@@@',
        data: {
            labels: Xdatapoints,
            datasets: [{
                label: '# Exchange Rates',
                xLabels:['one', 'two'],
                data: Ydatapoints,
                backgroundColor: [
                    'rgba(255, 99, 132, 0.8)',
                    'rgba(54, 162, 235, 0.8)',
                    'rgba(255, 206, 86, 0.8)',
                    'rgba(75, 192, 192, 0.8)',
                    'rgba(153, 102, 255, 0.8)',
                    'rgba(255, 159, 64, 0.8)'
                ],
                borderColor: [
                    'rgba(255,99,132,1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 206, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(153, 102, 255, 1)',
                    'rgba(255, 159, 64, 1)'
                ],
                borderWidth: 4
            }]
        },
        zoom: { enabled: true },
        options: {
            hover: { mode: 'index' },
            scales: { yAxes: [{ responsive: true, ticks: { beginAtZero: true } }] }
        }
    });
});
</script>
"
.Replace("@@@@@@@", ((string.IsNullOrEmpty(this.ChartType)) ? "bar" : this.ChartType))
.Replace("#######", this.DataSourceId.ToString())
.Replace("$$$$$$$", this.Name);
        }
    }

    [ProtoBuf.ProtoContract]
    public class EbDataGridView : EbControl
    {
        [ProtoBuf.ProtoMember(1)]
        public int DataSourceId { get; set; }

        public override string GetHtml()
        {
            return @"
<div>
<table id='example' style='width:100%' class='display'></table>
</div>
<script>
var cols = [];
$.get('/ds/columns/#######?format=json', function (data)
{
    if (data != null)
        $.each(data.columns, 
            function(i, value) { 
                cols.push({ 'data': value.columnIndex.toString(), 'title': value.columnName }); });

    $('#example').dataTable(
    {
        lengthMenu: [[100, 500, 1000, 2500, 5000, -1], [100, 500, 1000, 2500, 5000, 'All']],
        serverSide: true,
        processing: true,
        language: { processing: '<div></div><div></div><div></div><div></div><div></div><div></div><div></div>', },
        columns: cols,
        order: [],
        ajax: {
            url: '/ds/data/#######?format=json',
            data: function(dq) { delete dq.columns; },
            dataSrc: function(dd) { return dd.data; }
        },
    });
    $('#example_filter input').unbind();
    $('#example_filter input').bind('keyup', function(e) {
        if(e.keyCode == 13) {
            $('#example').dataTable().fnFilter(this.value);
        }
    });
});
</script>
".Replace("#######", this.DataSourceId.ToString().Trim());
        }
    }

    [ProtoBuf.ProtoContract]
    public class EbDataSource : EbObject
    {
        [ProtoBuf.ProtoMember(1)]
        public string Sql { get; set; }
    }
}