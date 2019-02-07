using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace ProductionTracker.Web.Reports
{
    public class CrystalReportGenerator
    {
        public CrystalReportGenerator()
        {

        }

        public static void GenerateReportInPDF(DataTable tb, string reportName)
        {
            using (var reportDocument = new ReportDocument())
            {
                var reportPath = HttpContext.Current.Server.MapPath("~/") + "Reports//" +  reportName;
                reportDocument.Load(reportPath);
                reportDocument.Database.Tables[0].SetDataSource(tb);
                
                reportDocument.ExportToHttpResponse(ExportFormatType.PortableDocFormat, HttpContext.Current.Response, false, reportName);
            }
        }

        public static void DownloadReportInExcel(DataTable tb, string reportName)
        {
            using (var reportDocument = new ReportDocument())
            {
                var reportPath = HttpContext.Current.Server.MapPath("~/") + "Reports//" + reportName;
                reportDocument.Load(reportPath);
                reportDocument.Database.Tables[0].SetDataSource(tb);
                reportDocument.ExportToHttpResponse(ExportFormatType.ExcelWorkbook, HttpContext.Current.Response, false, reportName);
            }
        }

        public static void DownloadReportInMsWord(DataTable tb, string reportName)
        {
            using (var reportDocument = new ReportDocument())
            {
                var reportPath = HttpContext.Current.Server.MapPath("~/") + "Reports//" + reportName;
                reportDocument.Load(reportPath);
                reportDocument.Database.Tables[0].SetDataSource(tb);
                reportDocument.ExportToHttpResponse(ExportFormatType.WordForWindows, HttpContext.Current.Response, false, reportName);
            }
        }

        // Convert To Data TABLE by the following Method
        public static DataTable LINQToDataTable<T>(IEnumerable<T> linqList)
        {
            var dtReturn = new DataTable();
            PropertyInfo[] columnNameList = null;

            if (linqList == null) return dtReturn;

            foreach (T t in linqList)
            {
                // Use reflection to get property names, to create table, Only first time, others will follow 
                if (columnNameList == null)
                {
                    columnNameList = ((Type)t.GetType()).GetProperties();

                    foreach (PropertyInfo columnName in columnNameList)
                    {
                        Type columnType = columnName.PropertyType;

                        if ((columnType.IsGenericType) && (columnType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            columnType = columnType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(columnName.Name, columnType));
                    }
                }

                DataRow dataRow = dtReturn.NewRow();

                foreach (PropertyInfo columnName in columnNameList)
                {
                    dataRow[columnName.Name] =
                        columnName.GetValue(t, null) == null ? DBNull.Value : columnName.GetValue(t, null);
                }

                dtReturn.Rows.Add(dataRow);
            }
            return dtReturn;
        }
    }
}