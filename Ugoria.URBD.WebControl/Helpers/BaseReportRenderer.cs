using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ugoria.URBD.WebControl.Models;
using MvcContrib.UI.Grid;

namespace Ugoria.URBD.WebControl.Helpers
{
    public class BaseReportRenderer<T> : HtmlTableGridRenderer<T> where T : class, IBaseReportView
    {
        private int listsCount = 0;
        private int countOnList = 0;
        private List<Queue<IReportPacketView>> lists = new List<Queue<IReportPacketView>>();
        private int currentList = 0;
        protected override void RenderCellValue(GridColumn<T> column, GridRowViewData<T> rowData)
        {
            if (column.ColumnType == typeof(IEnumerable<IReportPacketView>))
            {
                if (lists[currentList].Count == 0)
                    RenderText("</td><td></td><td>");
                else
                {
                    IReportPacketView firstPacket = lists[currentList].Dequeue();
                    RenderText(String.Format("{0}</td><td>{1:dd.MM.yyyy HH:mm:ss}</td><td>{2:0.00}", firstPacket.Filename, firstPacket.DateCreated, firstPacket.Size / 1024f));
                }
                currentList++;
            }
            else
                base.RenderCellValue(column, rowData);
        }

        protected override void RenderRowEnd()
        {
            base.RenderRowEnd();
            if (countOnList > 1)
            {
                IReportPacketView packet = null;
                for (int i = 1; i < countOnList; i++)
                {
                    RenderText("<tr>");
                    foreach (Queue<IReportPacketView> queue in lists)
                    {
                        if (queue.Count == 0)
                            RenderText("<td></td><td></td><td></td>");
                        else
                        {
                            packet = queue.Dequeue();
                            RenderText(String.Format("<td>{0}</td><td>{1:dd.MM.yyyy HH:mm:ss}</td><td>{2:0.00}</td>", packet.Filename, packet.DateCreated, packet.Size / 1024f));
                        }
                    }
                    RenderText("</tr>");
                }
            }
            currentList = 0;
        }

        protected override void RenderRowStart(GridRowViewData<T> rowData)
        {
            /*switch (listsCount)
            {
                case 2:
                    lists.Add(new Queue<IReportPacketView>(rowData.Item.LoadPackets ?? new List<IReportPacketView>()));
                    lists.Add(new Queue<IReportPacketView>(rowData.Item.UnloadPackets ?? new List<IReportPacketView>()));
                    countOnList = Math.Max(lists[0].Count, lists[1].Count);
                    break;
                case 1:
                    lists.Add(new Queue<IReportPacketView>(rowData.Item.LoadPackets ?? new List<IReportPacketView>()));
                    countOnList = lists[0].Count;
                    break;
                case 0:
                    countOnList = 0;
                    break;
            }*/
            base.RenderRowStart(rowData);
        }

        protected override void RenderStartCell(GridColumn<T> column, GridRowViewData<T> rowData)
        {
            if (listsCount > 0 && column.ColumnType != typeof(IEnumerable<IReportPacketView>))
                column.Attributes<T>(rowspan => countOnList);
            base.RenderStartCell(column, rowData);
        }

        protected override void RenderHeadStart()
        {
            listsCount = GridModel.Columns.Count(c => c.ColumnType == typeof(IEnumerable<IReportPacketView>));
            base.RenderHeadStart();
        }

        protected override void RenderHeaderCellStart(GridColumn<T> column)
        {
            if (column.ColumnType == typeof(IEnumerable<IReportPacketView>))
                column.HeaderAttributes<T>(colspan => 3);
            if (column.ColumnType != typeof(IEnumerable<IReportPacketView>))
                column.HeaderAttributes<T>(rowspan => 2);            
            base.RenderHeaderCellStart(column);
        }
        protected override void RenderHeadEnd()
        {            
            if (listsCount > 0)
            {
                RenderText("</tr>");
                RenderText("<tr>");
                for (int i = 0; i < listsCount; i++)
                {
                    RenderText("<th>Файл</th><th>Дата файла</th><th>Размер файла</th>");
                }                
            }
            base.RenderHeadEnd();
        }
    }
}