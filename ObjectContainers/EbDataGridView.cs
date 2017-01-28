﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    [ProtoBuf.ProtoContract]
    public class EbDataGridView : EbControlContainer
    {
        [ProtoBuf.ProtoMember(1)]
        public int DataSourceId { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public int PageSize { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public EbDataGridViewColumnCollection Columns { get; set; }

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

        public override string GetHtml()
        {
            return @"
<style>
.tablecontainer {
    width:100%;
    height:auto;
    border:solid 1px;
    display:inline-block;
    overflow-x:auto;
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
.dataTables_filter {
     display: none;
}

</style>
    <div class='tablecontainer'>
        <div style='width:auto; border:solid 1px yellow;'>
              <div  style=' height:40px; border:solid 1px blue;'>
                <input id='$$$$$$$_btnfilter' type='button' value='Filter' />
                <input id='$$$$$$$_btntotalpage' type='button' value='Total Page' />
              </div>
                    <h3>@@@@@@@</h3>
               <div id='$$$$$$$_loadingdiv' class='loadingdiv'>
                    <img id='$$$$$$$_loading-image' src='/images/ajax-loader.gif' alt='Loading...' />
               </div>
               <table id='$$$$$$$_tbl' style=' border:solid 1px red;' class='display compact'></table>
          </div>
     </div>
<script>
$('#$$$$$$$_tbl').append( $('<tfoot/>') );
$('#$$$$$$$_loadingdiv').show();
var pageTotal=0;   
$.get('/ds/columns/#######?format=json', function (data)
{
    var ids=[];
    var cols = [];
    var searchTextCollection=[];
    var search_colnameCollection=[];
    var order_colname='';
    var searchText='';
    var select_collection=[];
    var j=1;
    if (data != null){
        $.each(data.columns,
            function(i, value) { 
                _d = value.columnIndex.toString();
                _t = value.columnName;
                _c='dt-left';
                _v=true;
                if(value.columnName=='id')
                    _v=false;
                switch(value.type){
                    case 'System.Int32, System.Private.CoreLib': _c='dt-right'; break;
                    case 'System.Decimal, System.Private.CoreLib':_c='dt-right'; break;
                    case 'System.Int16, System.Private.CoreLib': _c='dt-right'; break;
                    case 'System.DateTime, System.Private.CoreLib':_c='dt-center'; break;
                    case 'System.Boolean, System.Private.CoreLib':_c='dt-center'; break;
                }
                if(value.columnIndex==0)                   
                    cols.push({'data':null, 'render': function ( data, type, row ) {return '<input type=\'checkbox\' id='+ 'chk'+j++ +' class=\'select - checkbox\'>'}});                
                cols.push({ 'data': _d, 'title': _t, 'className': _c,'visible': _v ,
                            'render': function ( data, type, full ) {
                                if(value.columnName==='sys_cancelled'){  
                                    if(data==false) return data;
                                    else return '<img id=\'cancel\' src=\'D:\\ExpressBase.Core\\ExpressBase.ServiceStack\\wwwroot\\images\\cancel-button-no-line-md.png\' style=\'width: 25px; \'/>';
                                }
                                 if(value.columnName=='sys_locked'){  
                                    if(data==false) return data;
                                    else return '<img id=\'lock\' src=\'D:\\ExpressBase.Core\\ExpressBase.ServiceStack\\wwwroot\\images\\Austin-Locksmith.png\' style=\'width: 25px; \'/>';
                                }
                                if(value.type=='System.Decimal, System.Private.CoreLib'){
                                      return parseFloat(data).toFixed(2);
                                }
                               return data;
                            }
               }); 
        });
    }
    $('#$$$$$$$_tbl').dataTable(
    {
       
        autoWidth: false,
        &&&&&&&,
        serverSide: true,
        processing: true,
        language: { processing: '<div></div><div></div><div></div><div></div><div></div><div></div><div></div>'},
        columns:cols, 
        order: [],
        deferRender: true,
        select: {
            style: 'os',
            selector: 'td:not(:last-child)' // no row selection on last column
        },
        ajax: {
            url: '/ds/data/#######?format=json',
            data: function(dq) { 
                    delete dq.columns; 
                    if(search_colnameCollection.length!==0){
                        dq.search_col='';
                        $.each(search_colnameCollection,function(i, value) {
                            if(dq.search_col=='')
                                dq.search_col=value;
                            else
                                dq.search_col=dq.search_col+','+value;
                        });
                    }   
                    if(order_colname!=='')
                        dq.order_col=order_colname; 
                    if(searchTextCollection.length!=0){
                        dq.searchtext='';
                        $.each(searchTextCollection,function(i, value) {
                            if(dq.searchtext=='')
                                dq.searchtext=value;
                            else
                                dq.searchtext=dq.searchtext+','+value;
                        });              
                    }
                    if(select_collection.length!=0){
                        dq.selectedvalue='';
                        $.each(select_collection,function(i, value) {
                            if(dq.selectedvalue=='')
                                dq.selectedvalue=value;
                            else
                                dq.selectedvalue=dq.selectedvalue+','+value;
                        });              
                    }
                },
            dataSrc: function(dd) {return dd.data; }
        },
        fnRowCallback: function( nRow, aData, iDisplayIndex, iDisplayIndexFull ) {
             $.each(data.columns,function(i, value) { 
                if(value.columnName==='sys_row_color'){
                    rgb=(aData[value.columnIndex]).toString();
                    var r=rgb.slice(0,-6);
                    r=parseInt(r);
                    if(r<=9)
                        fl='0';
                    r=r.toString(16);
                    if(fl==='0')
                        r='0'+r;

                    var g=rgb.slice(3,-3);
                    g=parseInt(g);
                    if(g<=9)
                        fl='0';
                    g=g.toString(16);
                    if(fl==='0')
                        g='0'+g;
                    var b=rgb.slice(6,9);
                    b=parseInt(b);
                    if(b<=9)
                        fl='0';
                    b=b.toString(16);
                    if(fl==='0')
                        b='0'+b;
                    rgb=r+g+b;
                    //alert(rgb);
                     $(nRow).css('background-color', '#' + rgb);
                }
                if(value.columnName==='sys_cancelled'){
                    var tr=aData[value.columnIndex];
                    if(tr==true)
                        $(nRow).css('color', '#f00');
                }
            });
         },
       fnFooterCallback: function ( nRow, aaData, iStart, iEnd, aiDisplay ) {
            $.each(data.columns,function(j, value) { 
               if(value.columnName!='id' && (value.type==='System.Decimal, System.Private.CoreLib' || value.type==='System.Int32, System.Private.CoreLib' || value.type==='System.Int16, System.Private.CoreLib')){               
                    var p=$('#footer1_select'+value.columnName).val();
                    if(p=='Sum'){
                        var api = $('#$$$$$$$_tbl').dataTable().api(), data;
			            var intVal = function ( i ) {
				            return typeof i === 'number' ? i : 0;
			            };
			            pageTotal = api
				            .column( j+1, { page: 'current'} )
				            .data()
				            .reduce( function (a, b) {
					            return intVal(a) + intVal(b);
				            }, 0 );
                    }
                    if(p=='Avg'){                             
                        var api = $('#$$$$$$$_tbl').dataTable().api();
			            var intVal = function ( i ) {
				            return typeof i === 'number' ? i : 0;
			            };
			            pageTotal =api
				            .column( j+1, { page: 'current'} )
				            .data()
				            .reduce( function (a, b) {
					            return intVal(a) + intVal(b);
				            }, 0 );
                        pageTotal=pageTotal / api
				            .column( j+1, { page: 'current'} )
				            .data().length;
                     }
                    var idd= 'footer1_txt' + value.columnName;                
                    if ($('#$$$$$$$_tbl tfoot tr:eq(0) th:eq('+j+')').children().length ==2)
                        ($('#$$$$$$$_tbl tfoot tr:eq(0) th:eq('+j+')').children('input')[0]).value=pageTotal.toFixed(2);
                    else
                        $('#$$$$$$$_tbl tfoot tr:eq(0) th:eq('+j+')').append('<input type=\'text\' value='+pageTotal.toFixed(2)+' id='+idd+' style=\'text-align:right;width: 100px;\'>');               
                }
                else
                    $('#$$$$$$$_tbl tfoot tr:eq(0) th:eq('+j+')').html('');
            });
        },
   });

    $('#$$$$$$$_tbl tfoot').append( $('#$$$$$$$_tbl thead tr').clone());  
  
    $('#$$$$$$$_tbl tfoot tr:eq(0) th').each( function (idx) {
        var title = $(this).text();
        var idd='footer1_select'+title;
        if(idx!=0){                 
            $(this).html('<select id='+idd+' width=\'60\'><option value=\'Sum\' selected=\'selected\'>Sum</option><option value=\'Avg\'> Avg </option></select>');
        }
    } );

    var tfoot = $('#$$$$$$$_tbl tfoot');
    $(tfoot).append($('#$$$$$$$_tbl thead tr').clone());  

    $('#$$$$$$$_tbl tfoot tr:eq(1) th').each( function (idx) {
        var idd= 'footer2_txt' + $(this).text();
        var idds= 'footer2_select' + $(this).text();
        var t='<span hidden>'+$(this).text()+'</span>'
        if(idx!=0){
            if(data.columns[idx].type=='System.Int32, System.Private.CoreLib'|| data.columns[idx].type=='System.Int16, System.Private.CoreLib'){
                $(this).html(t+'<select id='+idds+' width=\'60\' style=\'display:none;\'><option value=\'Sum\' selected=\'selected\'>Sum</option><option value=\'Avg\'> Avg </option></select><input type=\'text\' id='+idd+' style=\'width: 100px;display:none;\' />');
                //alert($(this).text());
            }
            else if(data.columns[idx].type=='System.Decimal, System.Private.CoreLib'){                
                $(this).html(t+'<select id='+idds+' width=\'60\' style=\'display:none;\'><option value=\'Sum\' selected=\'selected\'>Sum</option><option value=\'Avg\'> Avg </option></select><input type=\'text\' id='+idd+' style=\'width: 100px;display:none;\' />');               
            }
            else
                $(this).html('');
        }
    } );
        
    $('#$$$$$$$_loadingdiv').hide();

    var rgb='';
    var fl='';

    $('#$$$$$$$_tbl thead tr th').each( function (idx) {
        var title = $(this).text();
        var idd= 'header_txt1' + title;  
        var idds='header_select'+title;
        if(idx!=0){
            var t = '<span>' + title + '</span>';
            if(data.columns[idx].type=='System.Int32, System.Private.CoreLib'|| data.columns[idx].type=='System.Int16, System.Private.CoreLib'){                
                $(this).html(t+'<br/><select id='+idds+' width=\'30\' style=\' padding-left:5px;display:none;\'><option value=\'<\'> < </option><option value=\' > \'> > </option><option value=\'=\' selected=\'selected\'> = </option><option value=\'<=\'> <= </option><option value=\'>=\'> >= </option><option value=\'B\'> B </option></select><input type=\'number\' id='+idd+' width=\'100\' style=\'display:none;\'/>');                
            }
            else if(data.columns[idx].type=='System.String, System.Private.CoreLib')
                $(this).html(t+'<br/><input type=\'text\' id='+idd+' style=\'min-width: 160px;display:none;\'/>');
            else if(data.columns[idx].type=='System.DateTime, System.Private.CoreLib'){
                $(this).html(t+'<br/><select id='+idds+' width=\'30\' style=\'display:none;\'><option value=\'<\'> < </option><option value=\' > \'> > </option><option value=\' = \' selected=\'selected\'> = </option><option value=\'<=\'> <= </option><option value=\'>=\'> >= </option><option value=\'B\'> B </option></select><input type=\'date\' id='+idd+' width=\'100\' style=\'display:none;\'/>');                
            }
            else if(data.columns[idx].type=='System.Decimal, System.Private.CoreLib'){                
                $(this).html(t+'<br/><select id='+idds+' width=\'30\' style=\'display:none;\'><option value=\'<\'> < </option><option value=\' > \'> > </option><option value=\' = \' selected=\'selected\'> = </option><option value=\'<=\'> <= </option><option value=\'>=\'> >= </option><option value=\'B\'> B </option></select><input type=\'number\' id='+idd+' width=\'100\' style=\'display:none;\'/>');               
            }
        }            
    });
    
    $('#$$$$$$$_tbl thead').on( 'click', 'th', function(event) {
        if($(this).children().length==0)
            var headtitle=$(this).text();
        else
            var headtitle = $(this).children().eq(0).text();
        order_colname=headtitle;
     });

     $('#$$$$$$$_tbl thead tr th').on('click','input',function(event) {
        event.stopPropagation();
     }); 

    $('#$$$$$$$_tbl thead tr th').on('click','select',function(event) {
        event.stopPropagation();
     }); 

    $('#$$$$$$$_tbl thead tr th input').keypress(function (e) {
        //alert($(this).siblings('span').text());
        searchTextCollection=[];
        search_colnameCollection=[];
        select_collection=[];
         if(e.which == 13){            
            $('#$$$$$$$_tbl thead tr th input').each( function (idx) {
                if($(this).val()!=''){
                    if($.inArray($(this).siblings('span').text(), search_colnameCollection) == -1){
                        searchTextCollection.push($(this).val());
                        search_colnameCollection.push($(this).siblings('span').text());
                        if($(this).prev('select').length==1){
                            if($(this).prev('select').val()=='B'){
                                if($(this).next().val()!=''){
                                    searchTextCollection.splice( $.inArray($(this).val(),searchTextCollection) ,1 );
                                    searchTextCollection.push($(this).val()+'@'+$(this).next().val());
                                }
                            }
                            select_collection.push($(this).prev('select').val());
                        }
                        else
                            select_collection.push('null');
                    }
                }
            });
        }
     });

    $('#$$$$$$$_tbl thead th select').on('change',function(e){
        var idd='header_txt2'+$(this).siblings('span').text();
        if($(this).val()=='B'){
           if($(this).next('input').attr('type') == 'date')
                $(this).next().after($('<input type=\'date\'id='+idd+' style=\'min-width: 160px;\'/>'));
           else
                $(this).next().after($('<input type=\'number\'id='+idd+' style=\'min-width: 160px;\'/>'));
        }
        else
           $(this).next().next().remove();
    });


    $('#$$$$$$$_tbl tbody').on('click', '.checkbox', function(event){        
       if (document.getElementById(event.target.id).checked) {           
            var row = $(this).closest('tr');
            var data = $('#$$$$$$$_tbl').dataTable().fnGetData(row);
            ids.push(data[0]);
        }
        else {
            var row = $(this).closest('tr');
            var data = $('#$$$$$$$_tbl').dataTable().fnGetData(row);
            ids.splice(ids.indexOf(data[0]),1);
        }        
    });

    $('#$$$$$$$_tbl tfoot tr:eq(0) th select').on('change',function(e){
        $.each(data.columns,function(j, value) {
            if(e.target.id=='footer1_select'+value.columnName){
                var p=e.target.value;                                
                if(p=='Sum'){
                    var api = $('#$$$$$$$_tbl').dataTable().api();
			        var intVal = function ( i ) {
				        return typeof i === 'number' ? i : 0;
			        };
			        pageTotal =api
				        .column( j+1, { page: 'current'} )
                        .data()
				        .reduce( function (a, b) {
					        return intVal(a) + intVal(b);
				        }, 0 );                                        
                }
                if(p=='Avg'){                                   
                    var api = $('#$$$$$$$_tbl').dataTable().api();
			        var intVal = function ( i ) {
				        return typeof i === 'number' ? i : 0;
			        };
			        pageTotal =api
				        .column( j+1, { page: 'current'} )
				        .data()
				        .reduce( function (a, b) {
					        return intVal(a) + intVal(b);
				        }, 0 );
                    pageTotal=pageTotal / api
				        .column( j+1, { page: 'current'} )
				        .data().length;
                }               
                ($('#$$$$$$$_tbl tfoot tr:eq(0) th:eq('+j+')').children('input')[0]).value=pageTotal.toFixed(2);                
            }
        });
     }); 

    $('#$$$$$$$_tbl tfoot tr:eq(1) th select').on('change',function(e){
        $.each(data.columns,function(j, value) {
            if(e.target.id=='footer2_select'+value.columnName){             
            }
        });
     }); 

    $('#$$$$$$$_btnfilter').click(function(obj){
        $('#$$$$$$$_tbl thead tr th').each( function (idx) {
            var title = $(this).children('span').text();
            var idd1='header_txt1' + title; 
            var idd2='header_txt2' + title; 
            var idds='header_select'+title;
            $('#'+idd1).toggle();  
            $('#'+idd2).toggle();
            $('#'+idds).toggle();           
        });   
    });

    $('#$$$$$$$_btntotalpage').click(function(obj){alert('haaa');
        $('#$$$$$$$_tbl tfoot tr:eq(1) th').each( function (idx) {
            var title = $(this).children('span').text();
            var idd= 'footer2_txt' + title;
            var idds= 'footer2_select' + title;
            $('#'+idd).toggle(); 
            $('#'+idds).toggle();           
        });   
    });
});
      
</script>
".Replace("#######", this.DataSourceId.ToString().Trim())
.Replace("$$$$$$$", this.Name)
.Replace("@@@@@@@", this.Label)
.Replace("&&&&&&&", this.GetLengthMenu());
        }
    }

    [ProtoBuf.ProtoContract]
    public class EbDataGridViewColumn : EbControl
    {
        [ProtoBuf.ProtoMember(1)]
        public int Width { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public EbDataGridViewColumnType ColumnType { get; set; }

        public EbDataGridViewColumn()
        {
            this.Width = 100;
            this.ColumnType = EbDataGridViewColumnType.Text;
        }
    }

    [ProtoBuf.ProtoContract]
    public class EbDataGridViewColumnCollection : ObservableCollection<EbDataGridViewColumn>
    {
    }
}
