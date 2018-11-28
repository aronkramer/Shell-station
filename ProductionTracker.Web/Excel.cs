﻿using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ProductionTracker.Web
{
    public static class Excel
    {
        private static List<string> _erros = new List<string>();
        public static DataTable ClosedXmlImportSheet(string fileName)
        {
            try
            {
                var datatable = new DataTable();
                var workbook = new XLWorkbook(fileName);
                var xlWorksheet = workbook.Worksheet(1);
                var range = xlWorksheet.Range(xlWorksheet.FirstCellUsed(), xlWorksheet.LastCellUsed());

                var col = range.ColumnCount();
                var row = range.RowCount();

                datatable.Clear();
                for (var i = 1; i <= col; i++)
                {
                    var column = xlWorksheet.Cell(1, i);
                    datatable.Columns.Add(column.Value.ToString());
                }

                var firstHeadRow = 0;
                foreach (var item in range.Rows())
                {
                    if (firstHeadRow != 0)
                    {
                        var array = new object[col];
                        for (var y = 1; y <= col; y++)
                        {
                            array[y - 1] = item.Cell(y).Value;
                        }

                        datatable.Rows.Add(array);
                    }
                    firstHeadRow++;
                }
                return datatable;
            }
            catch
            {
                return null;
            }
        }
    }
}